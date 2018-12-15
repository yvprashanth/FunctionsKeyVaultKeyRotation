using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Prashanth.Function
{
    public static class NewTimerTrigger
    {
        private static readonly HttpClient client = new HttpClient();

        // Azure function which runs every 10 minutes to create a new key
        [FunctionName("NewTimerTrigger")]
        public static void Run([TimerTrigger("0 */10 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var kvResultObject = GetToken("https://vault.azure.net", "2017-09-01").Result;
            var finalString = ParseWebResponse(kvResultObject.Content.ReadAsStreamAsync().Result);
            dynamic parsedResultFromKeyVault = JsonConvert.DeserializeObject(finalString);
            string token = parsedResultFromKeyVault.access_token;
            CreateNewKeyAsync(token, log);
        }

        private static async void CreateNewKeyAsync(string token, ILogger log)
        {
            try {
            string myJson = "{'kty': 'RSA','key_size':2048, 'key_ops': [], 'attributes': {}, 'tags': {'tags': { 'purpose': 'unit test', 'test name': 'CreateKeyTest'}}}";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response = await client.PostAsync(
                    "https://prashanthyofficialvault.vault.azure.net/keys/myFirstKey/create?api-version=7.0", 
                    new StringContent(myJson, Encoding.UTF8, "application/json"));
            }
            } catch(Exception ex){
                log.LogInformation($"There was an issue creating a key: {ex.StackTrace}");
            }
        }

        // Helper method to parse JSON response
        private static string ParseWebResponse(Stream stream)
        {
            StreamReader readStream = new StreamReader (stream, Encoding.UTF8);
            var finalString = readStream.ReadToEnd();
            return finalString;
        }

        // This token is used to authenticate to Key Vault
        public static async Task<HttpResponseMessage> GetToken(string resource, string apiversion)  {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Secret", Environment.GetEnvironmentVariable("MSI_SECRET"));
            return await client.GetAsync(String.Format("{0}/?resource={1}&api-version={2}", Environment.GetEnvironmentVariable("MSI_ENDPOINT"), resource, apiversion));
        }
    }
}
