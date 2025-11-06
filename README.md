# Project

This lab teaches you how to design and build AI applications and agents using Azure AI Foundry. You will learn how to create AI-powered applications that can interact with users, process natural language, and perform tasks based on user guidance. You will also learn how to monitor, troubleshoot, and perform red teaming activities against agents.

## Deployment

### Azure Container Registry Deployment

The project includes a GitHub Actions workflow to automatically deploy the application to Azure Container Registry when changes are pushed to the `main` branch.

#### Required GitHub Secrets

To use the deployment workflow, configure the following secrets in your GitHub repository settings:

- `ACR_LOGIN_SERVER`: Your Azure Container Registry login server (e.g., `myregistry.azurecr.io`)
- `ACR_USERNAME`: Azure Container Registry username
- `ACR_PASSWORD`: Azure Container Registry password
- `ENV`: Complete contents of your `.env` file (see `src/env_sample.txt` for the expected format)

#### How It Works

1. The workflow triggers on push to the `main` branch
2. Builds a Docker image from the `src/` folder using the Dockerfile
3. Creates a `.env` file inside the container from the `ENV` secret during build time
4. Pushes the image to your Azure Container Registry with appropriate tags

**Note**: The `.env` file is never committed to the repository - it's created during the Docker build process from the GitHub secret.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
