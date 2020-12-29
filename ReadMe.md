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
5. Run the following command and copy the json-output

		az ad sp create-for-rbac --name HansKindberg-IdentityServer-Demo

### 1.3 GitHub secrets

To create a secret, go to **Settings** > **Secrets** > **New repository secret**.

Create the following secrets, name and value:

- **AZURE_CREDENTIALS**: {the json-output from above}
- **AZURE_LOCATION**: {the location for your resource-group, eg centralus}
- **AZURE_RESOURCEGROUP**: {the name of your resource-group}
- **AZURE_SUBSCRIPTION**: {the id of your Azure subscription}

## 2 Notes

- https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions
- https://azuredevopslabs.com/labs/devopsserver/azureserviceprincipal/#exercise-1-creating-an-azure-service-principal-for-use-as-an-azure-resource-manager-service-connection
- https://github.com/Azure/actions-workflow-samples/tree/master/AzureCLI