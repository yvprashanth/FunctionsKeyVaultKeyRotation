
# Prerequistes:
This assumes that you have an Azure Key Vault set up with a Key created in it.

# Step 1: Create a Timer Azure Function 
This [tutorial](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-scheduled-function) helps you create a timer Azure Function

# Step 2:
Replace the timer function code with the code as shown below and in [KeyRotator.cs](KeyRotator.cs) file. 

The below code gets a token from Azure Active Directory
```
        [FunctionName("NewTimerTrigger")]       
        public static void Run([TimerTrigger("0 */10 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var token = GetToken();
            CreateNewKeyAsync(token, log);
        }
```

# Step 3: 
Once you've specified a schedule in time Function Run command, the below method creates a new key if the name of the key doesn't exist within that vault OR it creates a new version of an existing key
```
        // This method creates a new Key if it doesn't exist or a new version of existing key if it already exists
        // You could replace the vault name with your Vault Name
        private static async void CreateNewKeyAsync(string token, ILogger log)
        {
            try 
            {            
                string myJson = "{'kty': 'RSA','key_size':2048, 'key_ops': [], 'attributes': {}, 'tags': {'tags': { 'purpose': 'unit test', 'test name': 'CreateKeyTest'}}}";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    var response = await client.PostAsync(
                        "https://<YourVaultName>.vault.azure.net/keys/<YourKeyName>/create?api-version=7.0", 
                        new StringContent(myJson, Encoding.UTF8, "application/json"));
                }
            } 
            catch(Exception ex){
                log.LogInformation($"Got the secret out of Key Vault: {ex.StackTrace}");
            }
        }
```