# IdentityServer-Demo-Deploy

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
5. Run the following command and copy the json-output (change to your preferred name)

		az ad sp create-for-rbac --name Your-IdentityServer-Demo --sdk-auth

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
2. Run the script [Create-Signing-Certificate.ps1](Create-Signing-Certificate.ps1), enter index and the password.
	  - The index is 1 for the first generation, 2 for the second generation etc. 
3. Note the output:
  - **Blob**
  - **Name**
  - **Thumbprint**

### 1.4 SITHS root-certificate

1. Export **SITHS e-id Root CA v2** from your certificate-store as base 64-encoded X.509 and save it as a *.crt file.
2. Remove *-----BEGIN CERTIFICATE-----*, *-----END CERTIFICATE-----* and all new-lines.
3. Use the value for the **SITHS_ROOTCERTIFICATE** secret, see below.

### 1.5 GitHub secrets

**IMPORTANT!** In the workflow we create a SQL Server login by using the powershell command **Invoke-Sqlcmd** and passing a query built up as an environment-variable. In that environment-variable we pass in ${{ secrets.DATABASE_USER_PASSWORD }}. Some special characters in the password may break the command. If you can not login to the database with the secret password after deployment you can try a another password. Make sure it does NOT contain a semicolon, ";", because that will break the connection-string.

To create a secret, go to **Settings** > **Secrets** > **New repository secret**.

Create the following secrets, name and value:

- **AZURE_APPSERVICE**: {the name of your app-service, eg your-site}
- **AZURE_CREDENTIALS**: {the json-value from above}
- **AZURE_LOCATION**: {the location for your resource-group, eg centralus}
- **AZURE_RESOURCEGROUP**: {the name of your resource-group}
- **AZURE_SUBSCRIPTION**: {the id of your Azure subscription}
- **CONNECTION_STRINGS**: [{"name": "IdentityServer", "slotSetting": false, "type": "SQLServer", "value": "Your connection-string"}]
- **DATABASE_NAME**: {name of the identity-server-database}
- **DATABASE_USER**: {user-name for the identity-server-database}
- **DATABASE_USER_PASSWORD**: {password for the identity-server-database user}
- **SIGNING_CERTIFICATE**: {the certificate-blob from above}
- **SIGNING_CERTIFICATE_NAME**: {the certificate-name from above}
- **SIGNING_CERTIFICATE_PASSWORD**: {the certificate-password from above}
- **SIGNING_CERTIFICATE_THUMBPRINT**: {the certificate-password from above}
- **SITHS_ROOTCERTIFICATE**: {the exported certificate-content from above}
- **SQLSERVER_ACCESS_CLIENT_IP**: {an ip-number with access to sql-server}
- **SQLSERVER_ADMINISTRATOR**: {user-name for the sql-server administrator}
- **SQLSERVER_ADMINISTRATOR_PASSWORD**: {password for the sql-server administrator}
- **SQLSERVER_NAME**: {the name of the sql-server}
- **VALIDATION_CERTIFICATE_THUMBPRINTS**: {empty, string or comma-separated string of values}
  - All validation-certificate-thumbprints. That is all previous signing-certificate-thumbprints, If you want previous encryptions to still work after renewing the signing-certificate.

### 1.6 Authentication providers

You need to allow your-identityserver-demo to use the registered authentication-providers.

https://your-identityserver-demo.azurewebsites.net

https://your-identityserver-demo-mtls.azurewebsites.net

#### 1.6.1 Google

- [Google registration](/Source/AppSettings.json#L86)
- [Google external login setup in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins/)
- https://console.developers.google.com/

#### 1.6.2 Microsoft

- [Microsoft registration](/Source/AppSettings.json#L120)
- [Microsoft Account external login setup with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins/)
- https://portal.azure.com/?l=en.en-001#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps

## 2 Notes

- https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions
- https://azuredevopslabs.com/labs/devopsserver/azureserviceprincipal/#exercise-1-creating-an-azure-service-principal-for-use-as-an-azure-resource-manager-service-connection
- https://github.com/Azure/actions-workflow-samples/tree/master/AzureCLI
- https://docs.microsoft.com/en-us/azure/app-service/configure-ssl-certificate-in-code
- https://docs.microsoft.com/en-us/azure/app-service/configure-ssl-certificate-in-code#load-certificate-in-linuxwindows-containers
- https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-manage-powershell-core/