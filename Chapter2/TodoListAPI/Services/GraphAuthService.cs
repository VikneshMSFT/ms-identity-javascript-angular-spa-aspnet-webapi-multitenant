﻿using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TodoListAPI.BusinessModels.TeamsModels;

namespace TodoListAPI.Services
{
    public class GraphAuthService : IGraphAuthService
    {
        private AuthenticationResult _authResult;

        public async Task<AuthenticationResult> GetGraphAuthResult()
        {
            if (this._authResult != null)
            {
                return this._authResult;
            }
            Task<AuthenticationResult> result = null;
            lock (this)
            {
                AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

                // You can run this sample using ClientSecret or Certificate. The code will differ only when instantiating the IConfidentialClientApplication
                bool isUsingClientSecret = AppUsesClientSecret(config);

                // Even if this is a console application here, a daemon application is a confidential client application
                IConfidentialClientApplication app;

                if (isUsingClientSecret)
                {
                    app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                        .WithClientSecret(config.ClientSecret)
                        .WithAuthority(new Uri(config.Authority))
                        .Build();
                }

                else
                {
                    X509Certificate2 certificate = ReadCertificate(config.CertificateName);
                    app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                        .WithCertificate(certificate)
                        .WithAuthority(new Uri(config.Authority))
                        .Build();
                }

                // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
                // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
                // a tenant administrator. 
                string[] scopes = new string[] { $"{config.ApiUrl}.default" };

                try
                {
                    result = app.AcquireTokenForClient(scopes)
                        .ExecuteAsync();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Token acquired");
                    Console.ResetColor();
                }
                catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
                {
                    // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                    // Mitigation: change the scope to be as expected
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Scope provided is not supported");
                    Console.ResetColor();
                }
            }
            return await result;
        }

        /// <summary>
        /// Checks if the sample is configured for using ClientSecret or Certificate. This method is just for the sake of this sample.
        /// You won't need this verification in your production application since you will be authenticating in AAD using one mechanism only.
        /// </summary>
        /// <param name="config">Configuration from appsettings.json</param>
        /// <returns></returns>
        private static bool AppUsesClientSecret(AuthenticationConfig config)
        {
            string clientSecretPlaceholderValue = "[Enter here a client secret for your application]";
            string certificatePlaceholderValue = "[Or instead of client secret: Enter here the name of a certificate (from the user cert store) as registered with your application]";

            if (!String.IsNullOrWhiteSpace(config.ClientSecret) && config.ClientSecret != clientSecretPlaceholderValue)
            {
                return true;
            }

            else if (!String.IsNullOrWhiteSpace(config.CertificateName) && config.CertificateName != certificatePlaceholderValue)
            {
                return false;
            }

            else
                throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
        }

        private static X509Certificate2 ReadCertificate(string certificateName)
        {
            if (string.IsNullOrWhiteSpace(certificateName))
            {
                throw new ArgumentException("certificateName should not be empty. Please set the CertificateName setting in the appsettings.json", "certificateName");
            }
            X509Certificate2 cert = null;

            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates;

                // Find unexpired certificates.
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                // From the collection of unexpired certificates, find the ones with the correct name.
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certificateName, false);

                // Return the first certificate in the collection, has the right name and is current.
                cert = signingCert.OfType<X509Certificate2>().OrderByDescending(c => c.NotBefore).FirstOrDefault();
            }
            return cert;
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private void Display(JObject result)
        {
            if (result != null)
            {
                foreach (JProperty child in result.Properties().Where(p => ((p.Name != null) && !p.Name.StartsWith("@"))))
                {
                    Console.WriteLine($"{child.Name} = {child.Value}");
                }
            }
        }
    }

}
