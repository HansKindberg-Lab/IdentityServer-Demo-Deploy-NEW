# IdentityServer-Demo-Deploy











name: "Azure-Deploy"

on:
  #push:
  #  branches: [ master ]
  workflow_dispatch:

env:
  SQLSERVER_INSTANCE: "${{ secrets.SQLSERVER_NAME }}.database.windows.net"

jobs:
  Azure-Deploy:
    name: "Azure-Deploy"
    runs-on: "ubuntu-latest"
    steps:
      - name: "Checkout code"
        uses: actions/checkout@v2
      - name: "Set up dotnet"
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: "5.0"
      - name: "Build"
        run: dotnet build --configuration Release
        working-directory: "Source"
      - name: "Get AppSettings.json path"
        id: appSettingsJsonPath
        run: |
          $appSettingsJsonPath = (Get-ChildItem -Filter "AppSettings.json").FullName;
          echo "::set-output name=value::$($appSettingsJsonPath)";
        shell: pwsh
        working-directory: "Source"
      - name: "Convert AppSettings.json to Azure App Service settings"
        id: azureAppServiceSettings
        run: |
          Add-Type -AssemblyName "Project.dll";
          $appServiceName = "${{ secrets.AZURE_APPSERVICE }}";
          $appSettingsJsonPath = "${{ steps.appSettingsJsonPath.outputs.value }}";
          $signingCertificateThumbprint = "${{ secrets.SIGNING_CERTIFICATE_THUMBPRINT }}";
          $validationCertificateThumbprints = "${{ secrets.VALIDATION_CERTIFICATE_THUMBPRINTS }}";
          $azureAppServiceSettings = [Project.ConfigurationHelper]::ConvertToAzureAppServiceSettings($appServiceName, $appSettingsJsonPath, $false, $signingCertificateThumbprint, $validationCertificateThumbprints);
          echo "::set-output name=value::$($azureAppServiceSettings)"
        shell: pwsh
        working-directory: "Source/Project/bin/Release/net5.0"
      - name: "Azure sign-in"
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      - name: "Ensure resource-group"
        uses: azure/cli@v1
        with:
          inlineScript: |
            az group create --location ${{ secrets.AZURE_LOCATION }} --name ${{ secrets.AZURE_RESOURCEGROUP }}
      - name: "ARM deploy"
        uses: azure/arm-deploy@v1
        with:
          parameters: appServiceName=${{ secrets.AZURE_APPSERVICE }} databaseName=${{ secrets.DATABASE_NAME }} signingCertificate=${{ secrets.SIGNING_CERTIFICATE }} signingCertificateName=${{ secrets.SIGNING_CERTIFICATE_NAME }} signingCertificatePassword=${{ secrets.SIGNING_CERTIFICATE_PASSWORD }} sithsRootCertificate=${{ secrets.SITHS_ROOTCERTIFICATE }} sqlServerAccessClientIp=${{ secrets.SQLSERVER_ACCESS_CLIENT_IP }} sqlServerAdministrator=${{ secrets.SQLSERVER_ADMINISTRATOR }} sqlServerAdministratorPassword=${{ secrets.SQLSERVER_ADMINISTRATOR_PASSWORD }} sqlServerName=${{ secrets.SQLSERVER_NAME }}
          resourceGroupName: ${{ secrets.AZURE_RESOURCEGROUP }}
          subscriptionId: ${{ secrets.AZURE_SUBSCRIPTION }}
          template: "Source/Template.json"
      - name: "Ensure user & owner for database"
        env:
          ENSURE_USER_SQL: |
            IF NOT EXISTS(SELECT [name] FROM [sys].[sql_logins] WHERE [name] = '${{ secrets.DATABASE_USER }}')
            BEGIN
              SELECT 'Creating login ''${{ secrets.DATABASE_USER }}''...' AS 'Output';
              EXECUTE ('CREATE LOGIN [${{ secrets.DATABASE_USER }}] WITH PASSWORD = ''${{ secrets.DATABASE_USER_PASSWORD }}'';');
            END
            ELSE
            BEGIN
              SELECT 'Setting password for login ''${{ secrets.DATABASE_USER }}''...' AS 'Output';
              EXECUTE ('ALTER LOGIN [${{ secrets.DATABASE_USER }}] WITH PASSWORD = ''${{ secrets.DATABASE_USER_PASSWORD }}'';');
            END
          ENSURE_OWNER_SQL: |
            IF NOT EXISTS (SELECT [name] FROM [sys].[database_principals] WHERE [name] = '${{ secrets.DATABASE_USER }}' AND [type] = 'S')
            BEGIN
              SELECT 'Setting ''${{ secrets.DATABASE_USER }}'' as owner for database ''${{ secrets.DATABASE_NAME }}''...' AS 'Output';
              EXECUTE ('CREATE USER [${{ secrets.DATABASE_USER }}] FOR LOGIN [${{ secrets.DATABASE_USER }}];');
              EXECUTE ('ALTER ROLE [db_owner] ADD MEMBER [${{ secrets.DATABASE_USER }}];');
            END
            ELSE
            BEGIN
              SELECT '''${{ secrets.DATABASE_USER }}'' is already owner for database ''${{ secrets.DATABASE_NAME }}''.' AS 'Output';
            END
        run: |
          Install-Module -Force -Name SqlServer -Repository PSGallery;
          Import-Module SqlServer;
          $note = "Instead of using -Credential we could also use -Password & -Username, but if doing so and the login fails the password gets exposed in the console when the error is written."
          $securePassword = ConvertTo-SecureString '${{ secrets.SQLSERVER_ADMINISTRATOR_PASSWORD }}' -AsPlainText -Force;
          $credential = [PSCredential]::New("${{ secrets.SQLSERVER_ADMINISTRATOR }}", $securePassword);
          Invoke-Sqlcmd -Credential $credential -Database "master" -Query "${{ env.ENSURE_USER_SQL }}" -ServerInstance "${{ env.SQLSERVER_INSTANCE }}";
          Invoke-Sqlcmd -Credential $credential -Database "${{ secrets.DATABASE_NAME }}" -Query "${{ env.ENSURE_OWNER_SQL }}" -ServerInstance "${{ env.SQLSERVER_INSTANCE }}";
        shell: pwsh
      - name: "Set App Service settings"
        uses: azure/appservice-settings@v1
        with:
          app-name: "${{ secrets.AZURE_APPSERVICE }}"
          app-settings-json: "${{ steps.azureAppServiceSettings.outputs.value }}"
          connection-strings-json: "${{ secrets.CONNECTION_STRINGS }}"
          mask-inputs: true
      - name: "Set MTLS App Service settings"
        uses: azure/appservice-settings@v1
        with:
          app-name: "${{ secrets.AZURE_APPSERVICE }}-mtls"
          app-settings-json: "${{ steps.azureAppServiceSettings.outputs.value }}"
          connection-strings-json: "${{ secrets.CONNECTION_STRINGS }}"
          mask-inputs: true

















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

## 2 Notes

- https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions
- https://azuredevopslabs.com/labs/devopsserver/azureserviceprincipal/#exercise-1-creating-an-azure-service-principal-for-use-as-an-azure-resource-manager-service-connection
- https://github.com/Azure/actions-workflow-samples/tree/master/AzureCLI
- https://docs.microsoft.com/en-us/azure/app-service/configure-ssl-certificate-in-code
- https://docs.microsoft.com/en-us/azure/app-service/configure-ssl-certificate-in-code#load-certificate-in-linuxwindows-containers
- https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-manage-powershell-core/