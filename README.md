
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
            var kvResultObject = GetToken("https://vault.azure.net", "2017-09-01").Result;
            var finalString = ParseWebResponse(kvResultObject.Content.ReadAsStreamAsync().Result);
            dynamic parsedResultFromKeyVault = JsonConvert.DeserializeObject(finalString);
            string token = parsedResultFromKeyVault.access_token;
            CreateNewKeyAsync(token, log);
        }
```