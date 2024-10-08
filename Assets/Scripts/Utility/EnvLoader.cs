using System;
using System.Collections.Generic;
using System.IO;

public static class EnvLoader
{
    private static Dictionary<string, string> envVars;

    static EnvLoader()
    {
        // Load .env file from the same directory as the executable
        string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
        envVars = LoadEnvFile(envPath);
    }

    private static Dictionary<string, string> LoadEnvFile(string filePath)
    {
        var envVars = new Dictionary<string, string>();

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Warning: File {filePath} not found.");
            return envVars;
        }

        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var keyValuePair = line.Split(new[] { '=' }, 2);
            if (keyValuePair.Length == 2)
            {
                string key = keyValuePair[0].Trim();
                string value = keyValuePair[1].Trim();
                envVars[key] = value;
            }
        }

        return envVars;
    }

    public static string GetEnv(string envParam)
    {
        if (envVars.ContainsKey(envParam))
        {
            return envVars[envParam];
        }

        return null;
    }
}
