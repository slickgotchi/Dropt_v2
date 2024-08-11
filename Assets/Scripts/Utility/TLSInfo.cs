using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Net.Security;
using UnityEngine;

public class TLSInfo : MonoBehaviour
{
    async void Start()
    {
        // Define the URL for the request
        string url = "https://alphaserver.playdropt.io";

        // Create an HttpClientHandler to configure the SSL/TLS settings
        HttpClientHandler handler = new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12,
            ServerCertificateCustomValidationCallback = ServerCertificateValidationCallback
        };

        // Create the HttpClient with the handler
        using (HttpClient client = new HttpClient(handler))
        {
            try
            {
                // Make the request
                HttpResponseMessage response = await client.GetAsync(url);

                // Log the response status code
                Debug.Log($"Response status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                // Log any errors
                Debug.LogError($"Error during request: {ex.Message}");
            }
        }
    }

    // Callback to inspect the SSL/TLS details
    private bool ServerCertificateValidationCallback(HttpRequestMessage requestMessage, System.Security.Cryptography.X509Certificates.X509Certificate2 certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslErrors)
    {
        // The current SslStream instance to access SSL details
        SslStream sslStream = new SslStream(null);

        // Log the protocol used
        Debug.Log($"TLS Protocol: {sslStream.SslProtocol}");

        // Note: Accessing the specific key exchange algorithm directly isn't straightforward in this context,
        // but the SslStream instance gives you access to the protocol and certificate details.
        // You can inspect the certificate for more information if needed.

        return true;
    }
}
