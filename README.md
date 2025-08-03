# AI Services Demo
This application contains several simple examples that demonstrate how to connect to different Azure AI services APIs.

## Pre-requisites.

- An [Azure](https://portal.azure.com/) account.
- An [Azure AI resource](https://learn.microsoft.com/en-us/azure/ai-services/multi-service-resource?pivots=azportal).

## Configuration requirements.

The application requires the following configuration values:

- **aiServicesEndpoint**: The URL for the Azure AI resource.
- **aiServicesKey**: The key for the Azure AI resource.

The application uses a combination of Azure Key Vault and Azure App Configuration to store the values needed to connect to the APIs (endpoints and keys), if you prefer to use appsettings.json or user secrets comment lines 8 to 16 in Program.cs.

## Scenario notes.

### Voice navigation.

The application uses a combination of the speech to text and conversational language understanding services to navigate the website via voice. In order to use those services you will have to create a language project

