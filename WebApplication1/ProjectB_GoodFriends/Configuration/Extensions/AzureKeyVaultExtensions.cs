using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Configuration;

public static class AzureKeyVaultExtensions
{    
    public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder configuration)
    {
        var kvSecret = Environment.GetEnvironmentVariable("AzureKeyVault_kvAccessParams_kvSecret");
        var kvUri = new Uri(Environment.GetEnvironmentVariable("AzureKeyVault_kvAccessParams_kvUri"));

        var tenantId = Environment.GetEnvironmentVariable("AzureKeyVault_kvAccessParams_readerSecrets_tenant");
        var clientSecret = Environment.GetEnvironmentVariable("AzureKeyVault_kvAccessParams_readerSecrets_password");
        var clientId = Environment.GetEnvironmentVariable("AzureKeyVault_kvAccessParams_readerSecrets_appId");

        //Open the AZKV from creadentials in the environment variables
        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        var client = new SecretClient(kvUri, credential);

        var secret = client.GetSecret(kvSecret);
        var userSecretsAzure = secret.Value.Value;

        if (string.IsNullOrEmpty(userSecretsAzure))
        {
            throw new Exception("The secret value is empty. Please check your Azure Key Vault configuration.");
        }

        //Adding content from Azure Key Vault as a JSON stream
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(userSecretsAzure));
        configuration.AddJsonStream(stream);

        return configuration;
    }

    //For Debug: Read and set them from Vault and set Environment variables
    //For Production: Will be set as Environment variables as part of the deployment process
    public static void PrepareDevelopmentAccess(IConfigurationRoot conf)
    {
        IConfigurationRoot _vaultAccess = null;
        string azureKeyVaultSettings = conf.GetValue<string>("AzureKeyVault:kvAccessParamsFile");
        if (!string.IsNullOrEmpty(azureKeyVaultSettings))
        {
            if (!File.Exists(azureKeyVaultSettings))
            {
                throw new FileNotFoundException($"The specified Azure Key Vault settings file was not found: {azureKeyVaultSettings}");
            }
            
            // Read Azure Key Vault access parameters from json file azureKeyVaultSettings
            Console.WriteLine($"AzureSettings: from {azureKeyVaultSettings}");
            _vaultAccess = new ConfigurationBuilder()
                .AddJsonFile(azureKeyVaultSettings, optional: true, reloadOnChange: true)
                .Build();
        }
        else
        {
            // Read Azure Key Vault access parameters from user secrets
            Console.WriteLine($"AzureSettings: from User Secrets");
            _vaultAccess = conf; 
        }

        // Set Azure Key Vault access parameters as environment variables
        // A deployed WebApp should use environment variables
        Environment.SetEnvironmentVariable("AzureKeyVault_kvAccessParams_kvSecret", _vaultAccess["AzureKeyVault:kvAccessParams:kvSecret"]);
        Environment.SetEnvironmentVariable("AzureKeyVault_kvAccessParams_kvUri", _vaultAccess["AzureKeyVault:kvAccessParams:kvUri"]);

        Environment.SetEnvironmentVariable("AzureKeyVault_kvAccessParams_readerSecrets_tenant", _vaultAccess["AzureKeyVault:kvAccessParams:readerSecrets:tenant"]);
        Environment.SetEnvironmentVariable("AzureKeyVault_kvAccessParams_readerSecrets_password", _vaultAccess["AzureKeyVault:kvAccessParams:readerSecrets:password"]);
        Environment.SetEnvironmentVariable("AzureKeyVault_kvAccessParams_readerSecrets_appId", _vaultAccess["AzureKeyVault:kvAccessParams:readerSecrets:appId"]);
    }
}