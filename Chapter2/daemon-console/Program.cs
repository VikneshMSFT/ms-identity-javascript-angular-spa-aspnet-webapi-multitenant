// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates; //Only import this if you are using certificate
using System.Text;
using System.Threading.Tasks;

namespace daemon_console
{
    /// <summary>
    /// This sample shows how to query the Microsoft Graph from a daemon application
    /// which uses application permissions.
    /// For more information see https://aka.ms/msal-net-client-credentials
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task RunAsync()
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
            
            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenForClient(scopes)
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

            if (result != null)
            {
                var httpClient = new HttpClient();
                var apiCaller = new ProtectedApiCallHelper(httpClient);
                 //await apiCaller.CallWebApiAndProcessResultASync($"https://teamsgraph.teams.microsoft.com/beta/teams('250dfa22-2334-4d15-a7c0-7d3bb9303e36')/channels", result.AccessToken, Display);
                //await apiCaller.CallWebApiAndProcessResultASync($"https://graph.microsoft.com/beta/teams", result.AccessToken, Display);


                //Create Team with migration mode set - Copy the teamId from Response Header
                CreateTeam newTeam = new CreateTeam
                {
                    teamCreationMode = "migration",
                    bind = "https://graph.microsoft.com/beta/teamsTemplates('standard')",
                    displayName = "MigrationTeam TestXYZ",
                    description = "Migrate data into teams",
                    createdDateTime = "2021-03-14T11:22:17.043Z"
                };
              
                var data = new StringContent(JsonConvert.SerializeObject(newTeam), Encoding.UTF8, "application/json");
                var response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams", result.AccessToken, Display, data);
                var location = response.Headers.Location?.ToString();
                var teamId = ((location.Split('/')[1]).Remove(0, 7)).Remove(36, 2);
                var channelId = "";

                CreateChannelRequest newChannel = new CreateChannelRequest
                {
                    channelCreationMode = "migration",
                    displayName = "Migration Channel TestXYZ",
                    description = "New channel",
                    membershipType = "standard",
                    createdDateTime = "2021-03-14T11:22:17.043Z"
                };
                data = new StringContent(JsonConvert.SerializeObject(newChannel), Encoding.UTF8, "application/json");
                response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/channels", result.AccessToken, Display, data);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    channelId = JObject.Parse(json)["id"].ToString();
                    Console.WriteLine("ChannelId - " + channelId);
                }
                else
                {
                    throw new Exception("Channel creation failed");
                }
                if(channelId == "")
                {
                    throw new Exception("Channel creation failed");
                }

                ChatMessageRequest newMessage = new ChatMessageRequest
                {
                    createdDateTime =  "2021-03-12T11:22:17.043Z",
                    from = new From
                    {
                        user = new User
                        {
                            id = "39c07c8d-ff89-4ef6-9855-2ec466148fe2",
                            displayName = "ua0001@sunds17.vtdeploy.com",
                            userIdentityType = "aadUser"
                        }
                    },
                    body = new ItemBody
                    {
                        content = "Automated migrated msg",
                        contentType = "html"
                    }
                };
                var str = JsonConvert.SerializeObject(newMessage);
                data = new StringContent(JsonConvert.SerializeObject(newMessage), Encoding.UTF8, "application/json");
                response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/channels/{channelId}/messages", result.AccessToken, Display, data);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Posted msg");
                }
                else
                {
                    throw new Exception("Posting msg failed");
                }
                    
                
                response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/channels/{channelId}/completeMigration", result.AccessToken, Display, null);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Completed migration for channel");
                }
                else
                {
                    throw new Exception("Completing migration for channel failed");
                }

                //Need to get the 'General' channel Id and complete migration  TODO


                response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/completeMigration", result.AccessToken, Display, null);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Completed migration for team");
                }
                else
                {
                    throw new Exception("Completing migration for team failed");
                }

                //Add owner
                AddMemberToTeam member = new AddMemberToTeam
                {
                    type = "#microsoft.graph.aadUserConversationMember",
                    roles = new string[] {"owner"},
                    bind = "https://graph.microsoft.com/beta/users/39c07c8d-ff89-4ef6-9855-2ec466148fe2"
                };
                data = new StringContent(JsonConvert.SerializeObject(member), Encoding.UTF8, "application/json");
                await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/members", result.AccessToken, Display, data);
            }
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static void Display(JObject result)
        {
            if (result != null)
            {
                foreach (JProperty child in result.Properties().Where(p => ((p.Name != null) && !p.Name.StartsWith("@"))))
                {
                    Console.WriteLine($"{child.Name} = {child.Value}");
                }
            }
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

    }
}
