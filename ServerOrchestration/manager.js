// manager.js
require('dotenv').config();
const express = require('express');
const bodyParser = require('body-parser');
const path = require('path');
const fs = require('fs');
const { execSync } = require('child_process');
const axios = require('axios');

const app = express();
app.use(bodyParser.json());
app.use(express.static('public')); // Serves static files like index.html

const PORT = 3000;
const MAX_LIVE_INSTANCES = parseInt(process.env.MAX_LIVE_INSTANCES);
const CERTS_PATH = '/etc/letsencrypt/live';
const MANAGER_IP = process.env.MANAGER_IP;
const DO_API_TOKEN = process.env.DO_API_TOKEN;
const DO_API_URL = process.env.DO_API_URL;
const DOMAIN = 'playdropt.io';

const workers = {};
const workerTimers = {};
const WORKER_TIMEOUT_MS = 30 * 1000;
const MAX_PLAYERS = 3;
const instanceLocks = new Set();
const allGameIds = new Set();
const _bookingCount = {};
const _bookingCountDateNow = {};    // this is the Date.now() when a booking was incremented

// configs for setting worker spares to have available
let _spareWorkersAsia = 2;
let _spareWorkersAmerica = 0;
let _spareWorkersEurope = 0;
let _maxWorkersAsia = 3;
let _maxWorkersAmerica = 3;
let _maxWorkersEurope = 3;

const SERVER_PARAM_PASSWORD = 'dr0pty32';

// if we are over the max spare workers, we do a cleanup after CLEANUP_TIME_LIMIT elapses
const CLEANUP_TIME_LIMIT = 60 * 60 * 1000;

// Timer to track cleanup operations
let cleanupTimer = {
    Asia: null,
    America: null,
    Europe: null,
};

// Helper to generate worker names
function generateWorkerName(index) {
    return `worker-${String(index).padStart(4, '0')}`;
}

// 1. ensure we have correct number of dns records up to cert_pool_size
async function createDNSRecords() {
    try {
        const response = await axios.get(`${DO_API_URL}/${DOMAIN}/records`, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${DO_API_TOKEN}`
            }
        });

        let existingRecords = response.data.domain_records.filter(record => record.type === 'A' && record.name.startsWith('worker-'));
        console.log(`Found existing DNS records: ${existingRecords.map(record => record.name).join(', ')}`);

        // Create missing DNS records up to CERT_POOL_SIZE
        for (let i = 1; i <= MAX_LIVE_INSTANCES; i++) {
            const workerName = generateWorkerName(i);
            if (!existingRecords.some(record => record.name === workerName)) {
                console.log(`Creating DNS record for ${workerName}.${DOMAIN}`);
                await axios.post(`${DO_API_URL}/${DOMAIN}/records`, {
                    type: 'A',
                    name: workerName,
                    data: MANAGER_IP,
                    ttl: 1800
                }, {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${DO_API_TOKEN}`
                    }
                });
                console.log(`Created DNS record for ${workerName}.${DOMAIN}`);
            }
        }

        // Remove extra DNS records if there are more than CERT_POOL_SIZE
        if (existingRecords.length > MAX_LIVE_INSTANCES) {
            for (let i = MAX_LIVE_INSTANCES; i < existingRecords.length; i++) {
                const record = existingRecords[i];
                console.log(`Deleting extra DNS record ${record.name}.${DOMAIN}`);
                await axios.delete(`${DO_API_URL}/${DOMAIN}/records/${record.id}`, {
                    headers: {
                        'Authorization': `Bearer ${DO_API_TOKEN}`
                    }
                });
                console.log(`Deleted DNS record ${record.name}.${DOMAIN}`);
            }
        }
    } catch (error) {
        console.error(`Error ensuring DNS records: ${error.response ? error.response.data.message : error.message}`);
        throw error;
    }
}

// 2. Point all DNS records to the MANAGER_IP
async function setDNSRecordsToManagerIP() {
    try {
        const response = await axios.get(`${DO_API_URL}/${DOMAIN}/records`, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${DO_API_TOKEN}`
            }
        });

        const existingRecords = response.data.domain_records.filter(record => record.type === 'A' && record.name.startsWith('worker-'));
        for (const record of existingRecords) {
            if (record.data !== MANAGER_IP) {
                console.log(`Updating DNS record ${record.name}.${DOMAIN} to point to ${MANAGER_IP}`);
                await axios.put(`${DO_API_URL}/${DOMAIN}/records/${record.id}`, {
                    type: 'A',
                    name: record.name,
                    data: MANAGER_IP,
                    ttl: 1800
                }, {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${DO_API_TOKEN}`
                    }
                });
                console.log(`Updated DNS record ${record.name}.${DOMAIN} to ${MANAGER_IP}`);
            }
        }
    } catch (error) {
        console.error(`Error updating DNS records: ${error.response ? error.response.data.message : error.message}`);
        throw error;
    }
}

// 3. Wait for all DNS records to propagate to MANAGER_IP
async function checkDNSPropagation(timeout = 60000) {
    const domains = Array.from({ length: MAX_LIVE_INSTANCES }, (_, i) => `${generateWorkerName(i + 1)}.${DOMAIN}`);

    console.log(`Awaiting DNS propagation for domains: ${domains.join(', ')}`);

    const startTime = Date.now();

    while (Date.now() - startTime < timeout) {
        let allValid = true;
        for (const domain of domains) {
            try {
                const result = execSync(`dig +short A ${domain}`).toString().trim();
                if (result !== MANAGER_IP) {
                    console.log(`Waiting for DNS propagation for ${domain}`);
                    allValid = false;
                    break;
                }
            } catch (error) {
                console.log(`DNS check failed for ${domain}: ${error.message}`);
                allValid = false;
                break;
            }
        }

        if (allValid) {
            console.log('All DNS records propagated successfully.');
            return;
        }

        await new Promise(resolve => setTimeout(resolve, 5000)); // Wait 5 seconds before the next check
    }

    console.error('Timeout reached. DNS propagation not confirmed for all domains.');
    throw new Error('DNS propagation timeout');
}

// 4. Ensure we have Let's Encrypt certs that match the DNS records
function matchCertificatesToDNSRecords() {
    console.log("Matching certificates to DNS records...");
    let certs = [];
    try {
        certs = fs.readdirSync(CERTS_PATH).filter(file => file.startsWith('worker-'));
        console.log("Current certificates in /etc/letsencrypt/live:", certs);
    } catch (error) {
        console.error(`Error reading directory ${CERTS_PATH}: ${error.message}`);
    }

    const existingCerts = new Set(certs);

    // Create certificates for DNS records that do not have a corresponding certificate
    for (let i = 1; i <= MAX_LIVE_INSTANCES; i++) {
        const workerName = generateWorkerName(i);
        const domain = `${workerName}.${DOMAIN}`;

        if (!existingCerts.has(workerName)) {
            try {
                execSync(`certbot certonly --nginx --non-interactive --agree-tos --key-type rsa -d ${domain} --email dropt.tfg@gmail.com`);
                console.log(`Certificate created successfully for ${domain}`);
            } catch (error) {
                console.error(`Failed to create certificate for ${domain}: ${error.message}`);
            }
        }
    }

    // Revoke and delete extra certificates if there are more than CERT_POOL_SIZE
    if (certs.length > MAX_LIVE_INSTANCES) {
        for (let i = MAX_LIVE_INSTANCES + 1; i <= certs.length; i++) {
            const workerName = generateWorkerName(i);
            const domain = `${workerName}.${DOMAIN}`;
            const certName = certs[i];
            console.log(`Revoking and deleting extra certificate for ${certName}`);
            try {
                execSync(`certbot revoke --cert-path ${CERTS_PATH}/${domain}/cert.pem --non-interactive --reason cessationOfOperation`);
                execSync(`rm -rf ${CERTS_PATH}/${domain}`);
                console.log(`Certificate for ${domain} revoked and deleted.`);
            } catch (error) {
                console.error(`Failed to revoke/delete certificate for ${domain}: ${error}`);
            }
        }
    }
}

// 5. now create pfx certs
function createPFXCerts() {
    console.log("Creating PFX certificates...");
    try {
        // Read the /etc/letsencrypt/live directory for worker-xxxx folders
        const certDirs = fs.readdirSync(CERTS_PATH).filter(dir => dir.startsWith('worker-'));

        for (const dir of certDirs) {
            const certDirPath = path.join(CERTS_PATH, dir);

            // Check if all required files exist before creating the PFX
            const privkeyPath = path.join(certDirPath, 'privkey.pem');
            const certPath = path.join(certDirPath, 'cert.pem');
            const chainPath = path.join(certDirPath, 'chain.pem');
            const pfxPath = path.join(certDirPath, 'cert.pfx');

            if (fs.existsSync(privkeyPath) && fs.existsSync(certPath) && fs.existsSync(chainPath)) {
                try {
                    execSync(`openssl pkcs12 -export -out ${pfxPath} -inkey ${privkeyPath} -in ${certPath} -certfile ${chainPath} -password pass:test -legacy`);
                    console.log(`PFX file created successfully for ${dir}`);
                } catch (error) {
                    console.error(`Failed to create PFX for ${dir}: ${error.message}`);
                }
            } else {
                console.warn(`Missing required files in ${dir}. Skipping PFX creation.`);
            }
        }
    } catch (error) {
        console.error(`Error creating PFX certificates: ${error.message}`);
    }
}

// Function to update or create a DNS record for a subdomain
async function updateDNSRecord(subdomain, ipAddress) {
    const fqdn = `${subdomain}.${DOMAIN}`;

    try {
        // Fetch existing DNS records
        const response = await axios.get(`${DO_API_URL}/${DOMAIN}/records`, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${DO_API_TOKEN}`
            }
        });

        const existingRecords = response.data.domain_records;
        const record = existingRecords.find(rec => rec.type === 'A' && rec.name === subdomain);

        if (record) {
            // Update the existing A record
            console.log(`Updating DNS record for ${fqdn} to point to ${ipAddress}`);
            await axios.put(`${DO_API_URL}/${DOMAIN}/records/${record.id}`, {
                type: 'A',
                name: subdomain,
                data: ipAddress,
                ttl: 1800 // Set TTL to 30 minutes
            }, {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${DO_API_TOKEN}`
                }
            });
            console.log(`Updated DNS record for ${fqdn} to ${ipAddress}`);
        } else {
            // Create a new A record if it doesn't exist
            console.log(`Creating new DNS record for ${fqdn} pointing to ${ipAddress}`);
            await axios.post(`${DO_API_URL}/${DOMAIN}/records`, {
                type: 'A',
                name: subdomain,
                data: ipAddress,
                ttl: 1800 // Set TTL to 30 minutes
            }, {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${DO_API_TOKEN}`
                }
            });
            console.log(`Created new DNS record for ${fqdn}`);
        }
    } catch (error) {
        console.error(`Error updating DNS record for ${fqdn}: ${error.response ? error.response.data.message : error.message}`);
        throw error;
    }
}


// Endpoint for worker droplets to request a certificate and get it transferred
app.post('/requestcert', async (req, res) => {
    const { ipAddress, workerName } = req.body;

    console.log("Received /requestcert from ip: " + ipAddress + ", workerName: " + workerName);

    if (!ipAddress) {
        return res.status(400).json({ message: 'IP address is required' });
    }

    // Try to find the cert that matches our worker name
    const fqdn = `${workerName}.${DOMAIN}`;
    const pfxPath = path.join(CERTS_PATH, fqdn, `cert.pfx`);
    const privkeyPath = path.join(CERTS_PATH, fqdn, `privkey.pem`);
    const certPath = path.join(CERTS_PATH, fqdn, `cert.pem`);
    const chainPath = path.join(CERTS_PATH, fqdn, `chain.pem`);

    if (fs.existsSync(pfxPath)) {
        try {
            console.log("Updating DNSRecord for: " + workerName + " to ip: " + ipAddress);

            // Update DNS record to point to the worker's IP address
            await updateDNSRecord(workerName, ipAddress);

            console.log("Transferring cert to worker at: " + ipAddress);

            // Execute SCP command to transfer the certificate
            execSync(`scp -o StrictHostKeyChecking=no ${pfxPath} ${privkeyPath} ${certPath} ${chainPath} root@${ipAddress}:/root/worker/`, {
                stdio: 'inherit'
            });

            console.log("Send to worker commonName: " + fqdn);

            return res.status(200).json({ commonName: fqdn, message: `Certificate assigned and transferred for ${fqdn}` });
        } catch (error) {
            console.error(`Failed to update DNS or transfer PFX file for ${fqdn} to ${ipAddress}: ${error.message}`);
            return res.status(500).json({ message: 'Failed to update DNS or transfer certificate' });
        }
    }

    res.status(404).json({ message: 'No available certificate found' });
});

// Helper function to get the image ID by name
async function getImageIdByName(imageName) {
    try {
        const response = await axios.get('https://api.digitalocean.com/v2/images', {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${DO_API_TOKEN}`,
            },
            params: {
                private: true, // Include private images such as snapshots
            },
        });

        const image = response.data.images.find(img => img.name === imageName);
        if (image) {
            return image.id;
        } else {
            console.error(`Image with name ${imageName} not found.`);
            return null;
        }
    } catch (error) {
        console.error(`Failed to retrieve image ID for ${imageName}: ${error.message}`);
        if (error.response) {
            console.error(`Response status: ${error.response.status}`);
            console.error(`Response data: ${JSON.stringify(error.response.data, null, 2)}`);
        }
        return null;
    }
}


// Helper to create a new droplet (DigitalOcean API)
async function createDroplet(region, newWorkerName) {

    var dataCenter = 'sgp1';
    if (region == 'america') dataCenter = 'sfo3';
    if (region == 'europe') dataCenter = 'fra1';

    const imageName = 'template-dropt-worker'; // Your image name here
    const imageId = await getImageIdByName(imageName);

    try {
        console.log(`Create droplet ${newWorkerName} in data center ${dataCenter}`);
        const response = await axios.post('https://api.digitalocean.com/v2/droplets', {
            name: newWorkerName,
            region: dataCenter,
            size: 's-1vcpu-1gb', // Example size, modify as needed
            image: imageId, // Replace with your actual snapshot ID
            ssh_keys: ['44:2a:0c:4d:8c:9b:26:8f:a3:92:0a:d4:f6:c9:12:b5'],
            user_data: `#!/bin/bash
                cd /root/worker
                tmux new-session -d -s worker 'node worker.js -workername ${newWorkerName}'
                `,
        }, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${DO_API_TOKEN}`,
            },
        });
        console.log(`Created droplet for ${newWorkerName} in datacenter ${dataCenter}: ${response.data.droplet.id}`);
        
    } catch (error) {
        console.error(`Failed to create droplet for ${newWorkerName} in datacenter ${dataCenter}: ${error.message}`);
        if (error.response) {
            console.error(`Response status: ${error.response.status}`);
            console.error(`Response data: ${JSON.stringify(error.response.data, null, 2)}`);
        }
    }

}

// Function to monitor and ensure enough spare workers are available
async function monitorSpareWorkers() {
    console.log("Monitoring spare workers...");

    // Define region configuration mappings
    const regionConfigs = [
        { region: 'asia', spareWorkers: _spareWorkersAsia, maxWorkers: _maxWorkersAsia },
        { region: 'america', spareWorkers: _spareWorkersAmerica, maxWorkers: _maxWorkersAmerica },
        { region: 'europe', spareWorkers: _spareWorkersEurope, maxWorkers: _maxWorkersEurope },
    ];

    for (const config of regionConfigs) {
        const { region, spareWorkers, maxWorkers } = config;

        // Get the count of workers in the region
        const spawningWorkers = Object.values(workers)
            .filter(worker => worker.region === region && worker.state === 'spawning');
        const readyWorkers = Object.values(workers)
            .filter(worker => worker.region === region && worker.state === 'ready');
        const playingWorkers = Object.values(workers)
            .filter(worker => worker.region === region && worker.state === 'playing');

        console.log(`Region ${region}, spawningWorkers: ${spawningWorkers.length}, readyWorkers: ${readyWorkers.length}, playingWorkers: ${playingWorkers.length}`);

        // Check if we need to spawn more workers
        if ((readyWorkers.length + spawningWorkers.length) < spareWorkers &&
            (readyWorkers.length + spawningWorkers.length + playingWorkers.length) < maxWorkers) {

            const workersToSpawn = spareWorkers - (readyWorkers.length + spawningWorkers.length);
            console.log(`Spawning ${workersToSpawn} new workers for ${region}`);

            for (let i = 0; i < workersToSpawn; i++) {
                const newWorkerName = generateWorkerName(Object.keys(workers).length+1);
                workers[newWorkerName] = { workerName: newWorkerName, region: region, state: "spawning" };
                await createDroplet(region, newWorkerName);
            }
        }

        // Start cleanup timer if there are too many free workers
        if (readyWorkers.length > spareWorkers && !cleanupTimer[region]) {
            console.log(`Too many free workers in ${region}. Starting cleanup timer.`);
            cleanupTimer[region] = setTimeout(() => {
                console.log(`Cleanup timer elapsed for ${region}. Cleaning up excess workers.`);
                cleanupExcessWorkers(region, spareWorkers);
                cleanupTimer[region] = null; // Reset timer
            }, CLEANUP_TIME_LIMIT); 
        }
    }
}

// Function to cleanup excess workers in a specific region
function cleanupExcessWorkers(region, spareWorkers) {
    const freeWorkers = Object.values(workers).filter(worker => worker.region === region && worker.state === 'ready');
    const excessCount = freeWorkers.length - spareWorkers;

    if (excessCount > 0) {
        console.log(`Destroying ${excessCount} excess workers in ${region}`);
        freeWorkers.slice(0, excessCount).forEach(worker => {
            destroyWorker(worker.workerName, worker.ipAddress); // Implement the logic for destroying a worker droplet
        });
    }
}

async function getDropletIdByIP(ipAddress) {
    try {
        // Fetch the list of droplets from DigitalOcean
        const response = await axios.get('https://api.digitalocean.com/v2/droplets', {
            headers: {
                'Authorization': `Bearer ${DO_API_TOKEN}`,
            }
        });

        // Loop through the droplets and find the one with the matching IP address
        const droplets = response.data.droplets;
        for (const droplet of droplets) {
            const dropletIPs = droplet.networks.v4.map(network => network.ip_address);
            if (dropletIPs.includes(ipAddress)) {
                console.log(`Found droplet with IP ${ipAddress}. Droplet ID: ${droplet.id}`);
                return droplet.id;
            }
        }

        console.log(`No droplet found with IP ${ipAddress}`);
        return null;
    } catch (error) {
        console.error(`Error fetching droplet ID for IP ${ipAddress}: ${error.message}`);
        return null;
    }
}

// Function to destroy a worker droplet (example implementation)
async function destroyWorker(workerName, ipAddress) {
    try {
        const dropletId = await getDropletIdByIP(ipAddress); // Implement logic to get droplet ID by IP
        if (dropletId) {
            await axios.delete(`https://api.digitalocean.com/v2/droplets/${dropletId}`, {
                headers: {
                    'Authorization': `Bearer ${DO_API_TOKEN}`,
                },
            });
            console.log(`Destroyed droplet with IP: ${ipAddress} and workerName: ${workerName}`);
            delete workers[workerName]; // Remove worker from the list
        }
    } catch (error) {
        console.error(`Failed to destroy droplet with IP ${ipAddress}: ${error.message}`);
    }
}


// Original functionality below
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

app.get('/getstatus', (req, res) => {
    const status = Object.values(workers).flatMap(worker =>
        worker.instances && Array.isArray(worker.instances)
            ? worker.instances.map(instance => ({
                gameId: instance.gameId,
                region: worker.region,
                workerIpAddress: worker.ipAddress,
                gamePort: instance.gamePort,
                playerCount: instance.playerCount,
                isFirstPlayerJoined: instance.isFirstPlayerJoined,
                timeLeft: instance.timeLeftMs / 1000, // Convert to seconds
                workerCpuUsage: worker.cpuUsage,
                isPublic: instance.isPublic
            }))
            : [] // Return an empty array if worker.instances is undefined or not an array
    );

    res.json({ availableGames: status });
});


app.post('/workerheartbeat', (req, res) => {
    let { region, workerName, ipAddress, commonName, clientCA, instances, cpuUsage } = req.body;

    let isFirstPlayerJoined = instances.some(inst => inst.isFirstPlayerJoined);

    workers[workerName] = {  
        region,
        workerName,
        ipAddress,
        state: isFirstPlayerJoined ? "playing" : "ready",
        commonName,
        clientCA,
        instances,
        cpuUsage,
    };

    console.log(`Worker heartbeat received from ${ipAddress}, region ${region}`);

    if (workerTimers[workerName]) {
        clearTimeout(workerTimers[workerName]);
    }

    workerTimers[workerName] = setTimeout(() => {
        console.log(`Worker ${workerName} did not send a heartbeat in time and has been removed.`);
        delete workers[workerName];
        delete workerTimers[workerName];
    }, WORKER_TIMEOUT_MS);

    res.status(200).json({ message: 'Worker heartbeat received' });
});

app.post('/getgameid', (req, res) => {
    const { region } = req.body;
    if (!region) {
        return res.status(400).json({ message: 'Region is required' });
    }

    const prefix = region === 'america' ? 'US' : region === 'europe' ? 'EU' : region === 'asia' ? 'AS' : '';

    // generate a unique game id
    let gameId;
    do {
        gameId = prefix + Array(4).fill().map(() => String.fromCharCode(65 + Math.floor(Math.random() * 26))).join('');
    } while (allGameIds.has(gameId));

    allGameIds.add(gameId);
    res.json({ gameId });
});

app.post('/getgame', (req, res) => {
    let { gameId, region } = req.body;

    gameId = gameId.toUpperCase();

    // EMPTY GAME REQUESTS
    if (!gameId || gameId == "") {

        // REGION CHECK
        if (!region || (region != 'asia' && region != 'america' && region != 'europe')) {
            return res.status(400).json({
                gameId: "",
                ipAddress: "",
                gamePort: "",
                commonName: "",
                clientCA: "",
                responseCode: 400,
                message: 'Please specify a region when calling /getgame.'
            });
        }

        for (const worker of Object.values(workers)) {
            if (worker.region === region) {
                // check no first player join, no booking count yet, and not locked
                const emptyInstance = worker.instances.find(inst =>
                    !inst.isFirstPlayerJoined &&
                    !_bookingCount[inst.gameId.toUpperCase()] &&
                    !instanceLocks.has(inst.gameId.toUpperCase())
                );

                if (emptyInstance) {
                    const upperCaseGameId = emptyInstance.gameId.toUpperCase();

                    instanceLocks.add(upperCaseGameId);
                    _bookingCount[upperCaseGameId] = 1;
                    _bookingCountDateNow[upperCaseGameId] = Date.now();
                    instanceLocks.delete(upperCaseGameId);

                    return res.status(200).json({
                        gameId: upperCaseGameId,
                        ipAddress: worker.ipAddress,
                        gamePort: emptyInstance.gamePort,
                        commonName: worker.commonName,
                        clientCA: worker.clientCA,
                        responseCode: 200,
                        message: "Joined an empty game"
                    });
                }
            }
        }

        return res.status(503).json({
            gameId: "",
            ipAddress: "",
            gamePort: "",
            commonName: "",
            clientCA: "",
            responseCode: 503,
            message: `No empty instances available for ${region} region`
        });
    }

    // JOIN GAME REQUESTS
    var isTargetInstanceFull = false;

    // iterate over all workers
    for (const worker of Object.values(workers)) {

        // find instance that matches the join gameId
        const instance = worker.instances.find(inst => inst.gameId.toUpperCase() === gameId);

        // double check we got a valid instance and we are not over booked
        if (instance) {
            const upperCaseGameId = instance.gameId.toUpperCase();

            if (_bookingCount[upperCaseGameId] < MAX_PLAYERS) {
                // increment booking
                _bookingCount[upperCaseGameId] += 1;
                _bookingCountDateNow[upperCaseGameId] = Date.now();

                // return success
                return res.status(200).json({
                    gameId: instance.gameId,
                    ipAddress: worker.ipAddress,
                    gamePort: instance.gamePort,
                    commonName: worker.commonName,
                    clientCA: worker.clientCA,
                    responseCode: 200,
                    message: "Joined an existing game"
                });
            } else {
                isTargetInstanceFull = true;
            }
        }
    }

    // GAME AT CAPACITY
    return res.status(403).json({
        gameId: "",
        ipAddress: "",
        gamePort: "",
        commonName: "",
        clientCA: "",
        responseCode: 403,
        message: isTargetInstanceFull ? `Game ${gameId} at max player capacity` : 'No matching instance found'
    });
});

// Helper function to destroy a droplet by name
async function destroyDropletByName(dropletName) {
    try {
        // Fetch the list of droplets from DigitalOcean
        const response = await axios.get('https://api.digitalocean.com/v2/droplets', {
            headers: {
                'Authorization': `Bearer ${DO_API_TOKEN}`,
            }
        });

        // Find the droplet with the matching name
        const droplet = response.data.droplets.find(d => d.name === dropletName);
        if (droplet) {
            await axios.delete(`https://api.digitalocean.com/v2/droplets/${droplet.id}`, {
                headers: {
                    'Authorization': `Bearer ${DO_API_TOKEN}`,
                }
            });
            console.log(`Destroyed droplet with name: ${dropletName}`);
        } else {
            console.log(`No droplet found with name: ${dropletName}`);
        }
    } catch (error) {
        console.error(`Failed to destroy droplet with name ${dropletName}: ${error.message}`);
    }
}

// Function to clear workers, clear spawningWorkers, and destroy matching droplets
async function clearAllWorkersAndDroplets() {
    // Clear the workers object
    Object.keys(workers).forEach(key => delete workers[key]);
    console.log("Cleared workers object.");

    // Clear the spawningWorkers set
    //spawningWorkers.clear();
    console.log("Cleared spawningWorkers set.");

    try {
        // Fetch the list of droplets from DigitalOcean
        const response = await axios.get('https://api.digitalocean.com/v2/droplets', {
            headers: {
                'Authorization': `Bearer ${DO_API_TOKEN}`,
            }
        });

        // Destroy any droplet with a name starting with "worker-"
        const workerDroplets = response.data.droplets.filter(d => d.name.startsWith('worker-'));
        for (const droplet of workerDroplets) {
            await destroyDropletByName(droplet.name);
        }

        if (workerDroplets.length === 0) {
            console.log("No worker droplets found to destroy.");
        }
    } catch (error) {
        console.error(`Error fetching droplets: ${error.message}`);
    }
}

// this function resets bookingCounts to the matching worker count
function trySetBookingCountsToPlayerCounts() {
    // iterate over all workers
    for (const worker of Object.values(workers)) {
        // check if worker.instances is valid
        if (!worker.instances) continue;

        // iterate over instances
        for (const instance of worker.instances) {
            const upperCaseGameId = instance.gameId.toUpperCase();

            // check for booking counts and dates validity
            if (_bookingCount && _bookingCountDateNow &&
                _bookingCount[upperCaseGameId] && _bookingCountDateNow[upperCaseGameId]) {

                // if it's been 5 seconds since booking count was incremented, set it to the player count
                if (Date.now() - _bookingCountDateNow[upperCaseGameId] > 5000) {
                    _bookingCount[upperCaseGameId] = instance.playerCount;
                }
            }
        }
    }
}

// for updating server params
app.post('/setworkerpoolparams', (req, res) => {
    const { password, spareWorkersAsia, spareWorkersAmerica, spareWorkersEurope, maxWorkersAsia, maxWorkersAmerica, maxWorkersEurope } = req.body;

    // Check if the password is correct
    if (password !== SERVER_PARAM_PASSWORD) {
        return res.status(403).json({ message: 'Forbidden: Incorrect password' });
    }

    // Update the server parameters if provided
    if (typeof spareWorkersAsia !== 'undefined') _spareWorkersAsia = spareWorkersAsia;
    if (typeof spareWorkersAmerica !== 'undefined') _spareWorkersAmerica = spareWorkersAmerica;
    if (typeof spareWorkersEurope !== 'undefined') _spareWorkersEurope = spareWorkersEurope;
    if (typeof maxWorkersAsia !== 'undefined') _maxWorkersAsia = maxWorkersAsia;
    if (typeof maxWorkersAmerica !== 'undefined') _maxWorkersAmerica = maxWorkersAmerica;
    if (typeof maxWorkersEurope !== 'undefined') _maxWorkersEurope = maxWorkersEurope;

    // Respond with the updated values
    res.status(200).json({
        message: 'Server parameters updated successfully',
        spareWorkersAsia: _spareWorkersAsia,
        spareWorkersAmerica: _spareWorkersAmerica,
        spareWorkersEurope: _spareWorkersEurope,
        maxWorkersAsia: _maxWorkersAsia,
        maxWorkersAmerica: _maxWorkersAmerica,
        maxWorkersEurope: _maxWorkersEurope
    });
});

app.get('/getworkerpoolparams', (req, res) => {
    res.status(200).json({
        message: 'Server parameters received',
        spareWorkersAsia: _spareWorkersAsia,
        spareWorkersAmerica: _spareWorkersAmerica,
        spareWorkersEurope: _spareWorkersEurope,
        maxWorkersAsia: _maxWorkersAsia,
        maxWorkersAmerica: _maxWorkersAmerica,
        maxWorkersEurope: _maxWorkersEurope
    });
});

// LOGIC FLOW
// 1. ensure we have up to CERT_POOL_SIZE of dns "worker-" dns records (remove any extra dns records above the CERT_POOL SIZE too)
// 2. point all dns records to the MANAGER_IP
// 3. wait for all dns records to be propogating to MANAGER_IP
// 4. ensure we have letsencrypt certs that match each of the "worker-" dns records (remove, revoke delete any certs above the CERT_POOL_SIZE)
// 5. create pfx certs for each of our letsencrypt certs

async function main() {
    // destroy all old droplets
    await clearAllWorkersAndDroplets();

    // Setup all dns records and certs (we do this in bulk)
    await createDNSRecords();
    await setDNSRecordsToManagerIP();
    await checkDNSPropagation();
    matchCertificatesToDNSRecords();
    createPFXCerts();

    await monitorSpareWorkers();

    // Start monitoring spare workers every 10 seconds
    setInterval(monitorSpareWorkers, 10 * 1000);

    setInterval(trySetBookingCountsToPlayerCounts, 5 * 1000);

    app.listen(PORT, () => {
        console.log(`Manager server running on port ${PORT}`);
    });
}

main();