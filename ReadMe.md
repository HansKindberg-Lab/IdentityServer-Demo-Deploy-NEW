# IdentityServer-Demo-Deploy








Temporary begin

New-SelfsignedCertificate -KeyExportPolicy Exportable -Subject "CN=MyIdsvCertificate" -KeySpec Signature -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -CertStoreLocation "cert:\LocalMachine\My"

[
  {
    "name": "MyName",
    "value": "My value",
    "type": "SQLServer",
    "slotSetting": false
  }
]

Temporary end



















Repository for deploying an IdentityServer demo to Azure. This repository contains configuration and Azure resource-templates for the deployment.

The IdentityServer instance used:

- https://hub.docker.com/r/hanskindberg/identity-server
- https://github.com/HansKindberg/Identity-Server

## 1 Prerequisites

### 1.1 Azure account

- An Azure account with an active subscription. [Create an account for free](https://azure.microsoft.com/en-us/free/).

### 1.2 Azure service principal

You need an Azure service principal to use as deployment credentials from the GitHub repository.

1. Sign in to your Azure account at https://portal.azure.com.
2. Click the **Cloud Shell** button, [>_], up in the black bar to launch the Cloud Shell.
3. Create a storage if you not already have one. You will be asked to do so if you do not have one.
4. Make sure the environment is set to **Bash** (Bash/PowerShell). On the left in the grey **Cloud Shell** bar.
5. Run the following command and copy the json-output

		az ad sp create-for-rbac --name HansKindberg-IdentityServer-Demo --sdk-auth

6. The json-output looks like this:

		{
		  "clientId": "{clientId-value}",
		  "clientSecret": "{clientSecret-value}",
		  "subscriptionId": "{subscriptionId-value}",
		  "tenantId": "{tenantId-value}",
		  "activeDirectoryEndpointUrl": "{activeDirectoryEndpointUrl-value}",
		  "resourceManagerEndpointUrl": "{resourceManagerEndpointUrl-value}",
		  "activeDirectoryGraphResourceId": "{activeDirectoryGraphResourceId-value}",
		  "sqlManagementEndpointUrl": "{sqlManagementEndpointUrl-value}",
		  "galleryEndpointUrl": "{galleryEndpointUrl-value}",
		  "managementEndpointUrl": "{managementEndpointUrl-value}"
		}

7. Remove everything below **tenantId**, it's not needed, like this:

		{
		  "clientId": "{clientId-value}",
		  "clientSecret": "{clientSecret-value}",
		  "subscriptionId": "{subscriptionId-value}",
		  "tenantId": "{tenantId-value}"
		}

8. Save the value to use for the **AZURE_CREDENTIALS** secret, see below.

### 1.3 Create signing-certificate

1. Decide a password.
2. Run the script [Create-Signing-Certificate.ps1](Create-Signing-Certificate.ps1), enter the password.
3. Note the output:
  - **Blob**
  - **Name**

### 1.4 SITHS root-certificate

1. Export **SITHS e-id Root CA v2** from your certificate-store as base 64-encoded X.509 and save it as a *.crt file.
2. Remove *-----BEGIN CERTIFICATE-----*, *-----END CERTIFICATE-----* and all new-lines.
3. Use the value for the **SITHS_ROOTCERTIFICATE** secret, see below.

### 1.5 GitHub secrets

To create a secret, go to **Settings** > **Secrets** > **New repository secret**.

Create the following secrets, name and value:

- **AZURE_APPSERVICE**: {the name of your app-service, eg your-site}
- **AZURE_CREDENTIALS**: {the json-value from above}
- **AZURE_LOCATION**: {the location for your resource-group, eg centralus}
- **AZURE_RESOURCEGROUP**: {the name of your resource-group}
- **AZURE_SUBSCRIPTION**: {the id of your Azure subscription}
- **CONNECTION_STRINGS**: [{"name": "IdentityServer", "slotSetting": false, "type": "SQLServer", "value": "Your connection-string"}]
- **SIGNING_CERTIFICATE**: {the certificate-blob from above}
- **SIGNING_CERTIFICATE_NAME**: {the certificate-name from above}
- **SIGNING_CERTIFICATE_PASSWORD**: {the certificate-password from above}
- **SITHS_ROOTCERTIFICATE**: {the exported certificate-content from above}
- **SQLSERVER_ACCESS_CLIENT_IP**: {an ip-number with access to sql-server}
- **SQLSERVER_ADMINISTRATOR**: {user-name for the sql-server administrator}
- **SQLSERVER_ADMINISTRATOR_PASSWORD**: {password for the sql-server administrator}
- **SQLSERVER_NAME**: {the name of the sql-server}

## 2 Notes

- https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions
- https://azuredevopslabs.com/labs/devopsserver/azureserviceprincipal/#exercise-1-creating-an-azure-service-principal-for-use-as-an-azure-resource-manager-service-connection
- https://github.com/Azure/actions-workflow-samples/tree/master/AzureCLI