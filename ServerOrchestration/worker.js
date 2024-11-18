require('dotenv').config();
const axios = require('axios');
const express = require('express');
const osUtils = require('os-utils');
//const os = require('os');
const Docker = require('dockerode');
const { execSync } = require('child_process');
const jwt = require('jsonwebtoken');
const fs = require('fs');
const path = require('path');

const app = express();
app.use(express.json());

const docker = new Docker();

function checkEnvVariables(requiredVariables) {
    const missingVariables = requiredVariables.filter(variable => !process.env[variable]);

    if (missingVariables.length > 0) {
        console.error(`Missing required environment variables: ${missingVariables.join(', ')}`);
        process.exit(1); // Stop the application if variables are missing
    } else {
        console.log('All required environment variables are set.');
    }
}

// check all env variables have been provided
checkEnvVariables(["DO_API_TOKEN", "DO_API_URL", "DOCKER_USERNAME", "DOCKER_PASSWORD", "DOCKER_IMAGE",
    "WORKER_PORT", "INSTANCE_START_PORT", "URI_MANAGER", "HEARTBEAT_SECRET",
    "HEARTBEAT_NONCE_EXPIRY_TIME_MIN", "NUMBER_GAMES", "GAME_TIME_LIMIT_MIN"]);

// digital ocean
//const DO_API_TOKEN = process.env.DO_API_TOKEN;
//const DO_API_URL = process.env.DO_API_URL;
//const DOMAIN_NAME = process.env.DOMAIN_NAME; // Adjust to your main domain

// docker
const DOCKER_USERNAME = process.env.DOCKER_USERNAME;
const DOCKER_PASSWORD = process.env.DOCKER_PASSWORD;
const DOCKER_IMAGE = process.env.DOCKER_IMAGE;

// ports and manager uri
const WORKER_PORT = parseInt(process.env.WORKER_PORT) || 3000;
const INSTANCE_START_PORT = parseInt(process.env.INSTANCE_START_PORT);
const URI_MANAGER = process.env.URI_MANAGER;

// heartbeat
const HEARTBEAT_SECRET = process.env.HEARTBEAT_SECRET;
const HEARTBEAT_NONCE_EXPIRY_TIME_MS = parseInt(process.env.HEARTBEAT_NONCE_EXPIRY_TIME_MIN) * 60 * 1000;

// game config
const MAX_INSTANCES = parseInt(process.env.MAX_INSTANCES);
const GAME_TIME_LIMIT_MS = parseInt(process.env.GAME_TIME_LIMIT_MIN) * 60 * 1000;

const usedHeartbeatNonces = new Set();

let _ipAddressExternal = null;
let _commonName = null;
let _region = "asia";
let _numberGames = parseInt(process.env.NUMBER_GAMES);
let _instances = [];
let _workerName = "";
let _clientCA = "";

function getArgValue(argName) {
    const argIndex = process.argv.indexOf(argName);
    if (argIndex > -1 && argIndex < process.argv.length - 1) {
        return process.argv[argIndex + 1];
    }
    return null;
}

// 0. set name
function readWorkerName() {
    _workerName = getArgValue('-workername');
    console.log(`Set name to ${_workerName}`);

    // Write the _workerName to a file called "workerNameTest.txt"
    //const filePath = path.join(__dirname, 'workerNameTest.txt');
    //fs.writeFileSync(filePath, _workerName, 'utf8');
    //console.log(`Worker name written to ${filePath}`);
}

// 1. get data center and set region from it
async function setDatacenterRegion() {
    try {
        const response = await axios.get('http://169.254.169.254/metadata/v1/region');
        const datacenter = response.data;
        console.log(`Droplet is running in data center: ${datacenter}`);

        let region = "unknown";

        switch (datacenter) {
            // America data centers
            case "nyc1":
            case "nyc2":
            case "nyc3":
            case "sfo1":
            case "sfo2":
            case "sfo3":
            case "tor1":
            case "iad1":
                region = "america";
                break;

            // Asia data centers
            case "sgp1":
            case "sgp2":
            case "sgp3":
            case "blr1":
            case "hnd1":
                region = "asia";
                break;

            // Europe data centers
            case "ams2":
            case "ams3":
            case "fra1":
            case "fra2":
            case "lon1":
            case "lon2":
                region = "europe";
                break;

            default:
                console.warn(`Datacenter ${datacenter} not recognized. Defaulting to "unknown" region.`);
                break;
        }

        console.log(`Mapped region: ${region}`);
        _region = region;
    } catch (error) {
        console.error('Error getting data center region:', error.message);
    }
}


// 1. allow ports
function allowPortsInUFW() {
    // allow the main port we're listening on
    try {
        execSync(`ufw allow ${WORKER_PORT}/tcp`);
        console.log(`Allowed port ${WORKER_PORT}/tcp in UFW.`);
    } catch (error) {
        console.error(`Error allowing port ${WORKER_PORT}/tcp in UFW:`, error.message);
    }

    for (let i = 0; i < _numberGames; i++) {
        const port = INSTANCE_START_PORT + i;
        try {
            execSync(`ufw allow ${port}/tcp`);
            console.log(`Allowed port ${port}/tcp in UFW.`);
        } catch (error) {
            console.error(`Error allowing port ${port}/tcp in UFW:`, error.message);
        }
    }
}

// 2. grab our workers ip address
async function getExternalIp() {
    try {
        const response = await axios.get('https://api.ipify.org?format=json');
        _ipAddressExternal = response.data.ip;
        console.log(`Server External IP: ${_ipAddressExternal}`);
    } catch (error) {
        console.error('Error getting external IP:', error.message);
    }
}

// 3. request a domain name and cert
async function requestCert() {
    try {
        const response = await axios.post(URI_MANAGER + "/requestcert", { ipAddress: _ipAddressExternal, workerName: _workerName ? _workerName : "fail" });
        _commonName = response.data.commonName;

        // we now want to store _clientCA
        try {
            _clientCA = fs.readFileSync(`/root/worker/chain.pem`, 'utf8');
        } catch (error) {
            console.error(`Error reading clientCA from /etc/letsencrypt/live/${commonName}/chain.pem: ${error.message}`);
            return res.status(500).json({ message: 'Failed to read clientCA from file' });
        }

        console.log(response.data.message + " and received cert.pfx");
    } 
    catch (error) {
        console.error("Could not get cert from manager app. Error: " + error);
    }
}

// 4. pull latest docker image
async function pullDockerImageWithLogin() {
    try {
        console.log("Logging in to Docker...");
        execSync(`docker login -u ${DOCKER_USERNAME} -p ${DOCKER_PASSWORD}`);
        console.log("Docker login succeeded.");

        console.log('Pulling Docker image ' + DOCKER_IMAGE);
        execSync(`docker pull ${DOCKER_IMAGE}`);
        console.log('Successfully pulled Docker image');
    } catch (error) {
        console.error('Error during Docker operations:', error.message);
    }
}

// 5. clean up any existing running containers
async function cleanupRunningContainers() {
    console.log("Clean up existing containers...");
    try {
        // List all containers
        const containers = await docker.listContainers();

        // Filter containers that match our naming convention
        const gameContainers = containers.filter(container => container.Names.some(name => name.includes('unity_game_instance_')));

        // Stop and remove each container
        for (const containerInfo of gameContainers) {
            const container = docker.getContainer(containerInfo.Id);
            await container.stop();
            await container.remove();
            console.log(`Stopped and removed existing container: ${containerInfo.Names[0]}`);
        }

        console.log('Cleanup complete. All relevant containers have been stopped and removed.');
    } catch (error) {
        console.error('Error cleaning up running containers:', error.message);
    }
}

// Function to request unique Game ID from manager.js
async function requestGameId() {
    try {
        console.log("Request gameid with region: " + _region);
        const response = await axios.post(URI_MANAGER + '/getgameid', {
            region: _region
        });
        return response.data.gameId;
    } catch (error) {
        console.error('Error requesting game ID from manager.js, it may not be running yet. ' + error.message);
    }
}

// 7. function to start a container instance
async function createUnityInstance(instanceIndex, gamePort, gameId = null) {
    if (!gameId) {
        gameId = await requestGameId();
        if (!gameId) {
            console.error('Failed to get Game ID for instance', instanceIndex);
            return;
        }
    }

    const containerName = `unity_game_instance_${instanceIndex}`;
    const cpuLimit = (90 / MAX_INSTANCES) * 0.01;

    try {
        const container = await docker.createContainer({
            Image: DOCKER_IMAGE,
            name: containerName,
            Env: [
                `WORKER_PORT=${WORKER_PORT}`,
                `GAME_PORT=${gamePort}`,
                `GAME_ID=${gameId}`,
                `IP_ADDRESS=${_ipAddressExternal}`,
                `HEARTBEAT_SECRET=${process.env.HEARTBEAT_SECRET}`
            ],
            ExposedPorts: {
                [`${gamePort}/tcp`]: {}
            },
            HostConfig: {
                PortBindings: {
                    [`${gamePort}/tcp`]: [{ HostPort: `${gamePort}` }]
                },
                CpuQuota: Math.round(cpuLimit * 100000),
                Memory: 1024 * 1024 * 1024,
                Binds: [
                    `/root/worker/cert.pfx:/usr/local/unity_server/cert.pfx`, // Mount to the Unity executable folder,
                    `/root/worker/privkey.pem:/usr/local/unity_server/privkey.pem`,
                    `/root/worker/cert.pem:/usr/local/unity_server/cert.pem`,
                    `/root/worker/chain.pem:/usr/local/unity_server/chain.pem`
                ]
            }
        });

        await container.start();
        console.log(`Started Docker container for instance ${instanceIndex}:`, container.id);

        _instances[instanceIndex] = {
            gameId,
            gamePort,
            containerName,
            containerId: container.id,
            status: 'running',
            playerCount: 0,
            isFirstPlayerJoined: false,
            countdownTimer: null,
            timeLeftMs: GAME_TIME_LIMIT_MS,
            isDestroying: false,
            isPublic: false,
        };
    } catch (error) {
        console.error(`Error starting Docker container for instance ${instanceIndex}:`, error.message);
    }
}

// Function to destroy a Unity server instance using Docker SDK
async function destroyUnityInstance(instanceIndex) {
    const instance = _instances[instanceIndex];
    if (!instance || instance.isDestroying) return;

    try {
        instance.isDestroying = true;
        const container = docker.getContainer(instance.containerId);
        await container.stop();
        await container.remove();

        console.log(`Destroyed Docker container ${instance.containerName}`);

        // Start a new instance on the same port
        await createUnityInstance(instanceIndex, instance.gamePort);

    } catch (error) {
        console.error(`Error destroying Docker container ${instance.containerName}:`, error.message);
    }
}

// Function to start Unity instances initially
async function startUnityInstances() {
    for (let i = 0; i < _numberGames; i++) {
        const gamePort = INSTANCE_START_PORT + i;
        await createUnityInstance(i, gamePort);
    }
}

// recurring functions
// send a worker heartbeat
async function sendWorkerHeartbeat() {
    const numCores = osUtils.cpuCount(); // Get the number of CPU cores

    osUtils.cpuUsage(async (cpuUsagePerCore) => {
        const totalCpuUsage = (cpuUsagePerCore * numCores * 100).toFixed(2); // Calculate total CPU usage as a percentage

        try {
            const response = await axios.post(URI_MANAGER + '/workerheartbeat', {
                region: _region,
                workerName: _workerName,
                ipAddress: _ipAddressExternal,
                commonName: _commonName,
                clientCA: _clientCA,
                instances: _instances.map(({ gameId, gamePort, playerCount, isFirstPlayerJoined, status, timeLeftMs, isPublic }) => ({
                    gameId,
                    gamePort,
                    playerCount,
                    isFirstPlayerJoined,
                    status,
                    timeLeftMs,
                    isPublic
                })),
                cpuUsage: totalCpuUsage, // Report total CPU usage
            });
            console.log('Worker heartbeat sent:', response.status);
        } catch (error) {
            console.error('Error sending worker heartbeat, the manager node may not be running yet. ' + error.message);
        }
    });
}

// Function to monitor game instances for inactivity and mark them for destruction
function monitorGameInstances() {
    const indexesToDestroy = [];

    _instances.forEach((instance, index) => {
        if (instance && instance.isFirstPlayerJoined) {
            // If no players are left, mark the instance for destruction
            if (instance.playerCount <= 0 && !instance.isDestroying) {
                console.log(`Player count is 0 for game ID ${instance.gameId}, marking instance for destruction.`);
                indexesToDestroy.push(index);
            }
        }
    });

    // Destroy instances marked for destruction
    indexesToDestroy.forEach(index => {
        destroyUnityInstance(index);
    });
}

// Function to handle countdown for game instances
function countdownGameInstances() {
    const indexesToDestroy = [];

    _instances.forEach((instance, index) => {
        if (instance && instance.isFirstPlayerJoined && instance.timeLeftMs > 0) {
            instance.timeLeftMs -= 5000;

            if (instance.timeLeftMs <= 0 && !instance.isDestroying) {
                console.log(`Time expired for game ID ${instance.gameId}, marking instance for destruction.`);
                indexesToDestroy.push(index);
            }
        }
    });

    // Destroy instances marked for destruction
    indexesToDestroy.forEach(index => {
        destroyUnityInstance(index);
    });
}

// Function to clean up containers on application exit
async function onExit() {
    console.log('\nGracefully shutting down...');
    await cleanupRunningContainers();
    process.exit();
}

// Handle process termination (Ctrl + C)
process.on('SIGINT', onExit);
process.on('SIGTERM', onExit);

// token verification
function verifyToken(token, expectedPayload, secret) {
    try {
        const decoded = jwt.verify(token, secret);
        if (decoded.gameId !== expectedPayload.gameId ||
            decoded.nonce !== expectedPayload.nonce ||
            decoded.playerCount !== expectedPayload.playerCount) {
            throw new Error("Payload mismatch");
        }
        return decoded;
    } catch (error) {
        console.error("JWT verification failed:", error.message);
        return null;
    }
}

// Endpoint to receive instance heartbeats
app.post('/instanceheartbeat', (req, res) => {
    // 1. store body data
    const { gameId, playerCount, isPublic, nonce, token } = req.body;

    // 2. Check if nonce has been used
    if (usedHeartbeatNonces.has(nonce)) {
        return res.status(400).json({ message: "Replay attack detected" });
    }

    // 3. Verify token
    const decoded = verifyToken(token, { gameId, playerCount, isPublic, nonce }, HEARTBEAT_SECRET);
    if (!decoded) {
        return res.status(401).json({ message: "Invalid token" });
    }

    // 4. Add nonce to the used list and set an expiry
    usedHeartbeatNonces.add(nonce);
    setTimeout(() => usedHeartbeatNonces.delete(nonce), HEARTBEAT_NONCE_EXPIRY_TIME_MS);

    // 5. Update instance data and respond
    const instance = _instances.find(inst => inst && inst.gameId === gameId);
    if (instance) {
        instance.playerCount = playerCount;
        instance.isPublic = isPublic;
        if (playerCount > 0) {
            instance.isFirstPlayerJoined = true;
        }
    }

    console.log(`Instance hearbeat received for ${gameId}`);
    res.status(200).json({ message: 'Instance heartbeat received' });
});


async function main() {
    readWorkerName();
    allowPortsInUFW();

    await setDatacenterRegion();
    await getExternalIp();
    await requestCert();
    await pullDockerImageWithLogin();
    await cleanupRunningContainers();

    await startUnityInstances();

    setInterval(sendWorkerHeartbeat, 3000);
    setInterval(monitorGameInstances, 5000);
    setInterval(countdownGameInstances, 5000);
    

    app.listen(WORKER_PORT, () => {
        console.log(`Worker server listening on port ${WORKER_PORT}`);
    });
}

main();
