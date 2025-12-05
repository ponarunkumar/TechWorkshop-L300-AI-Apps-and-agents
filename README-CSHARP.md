# TechWorkshop L300: AI Apps and Agents - C#14/.NET 10 Edition

> **🎯 Purpose**: This repository provides a C# implementation of the AI Apps and Agents workshop, designed specifically for C# developers to learn about building AI-powered applications using Microsoft Azure AI Foundry.

[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![C# Version](https://img.shields.io/badge/C%23-14.0-239120)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()

## 📋 Table of Contents

- [Overview](#overview)
- [What's Different from Python Version](#whats-different-from-python-version)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [Exercise Modules](#exercise-modules)
- [Development Guide](#development-guide)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [Resources](#resources)

## 🌟 Overview

This lab teaches you how to design and build AI applications and agents using **C#14**, **.NET 10**, and **Azure AI Foundry**. You will learn how to:

- 🤖 Build intelligent AI agents that can interact with users
- 🎨 Create multimodal applications (text, image, video)
- 🔗 Implement agent-to-agent (A2A) communication
- 📊 Monitor and observe AI application behavior
- 🛡️ Implement red teaming and safety measures
- 🚀 Deploy production-ready AI applications to Azure

### Key Features

✨ **Multimodal AI Shopping Assistant** - Chat interface supporting text, images, and videos  
🤖 **Multiple Specialized Agents** - Interior Designer, Customer Loyalty, Inventory Management  
⚡ **Real-time Communication** - SignalR-based WebSocket connections  
🔍 **Azure AI Search Integration** - Semantic search for product recommendations  
📈 **Full Observability** - OpenTelemetry integration with Application Insights  
🔐 **Enterprise-ready** - Strong typing, dependency injection, comprehensive error handling  

## 🔄 What's Different from Python Version

This C# implementation provides **feature parity** with the Python version while leveraging C#/.NET strengths:

### Technology Stack Comparison

| Component | Python | C# |
|-----------|--------|-----|
| **Web Framework** | FastAPI | ASP.NET Core |
| **Real-time Comm** | WebSocket (raw) | SignalR |
| **DI Container** | Manual setup | Built-in DI |
| **Configuration** | .env files | appsettings.json + User Secrets |
| **Type Safety** | Dynamic typing | Strong static typing |
| **Performance** | Interpreted | Compiled (JIT optimized) |
| **Testing** | pytest | xUnit/NUnit |
| **Logging** | print/logging | Serilog/ILogger |

### Key Benefits of C# Version

1. **🎯 Type Safety** - Catch errors at compile time
2. **⚡ Performance** - 2-3x faster in many scenarios
3. **🛠️ Better Tooling** - Visual Studio, Rider, VS Code with excellent IntelliSense
4. **📚 Rich Ecosystem** - Mature libraries and frameworks
5. **🏢 Enterprise Support** - Microsoft's backing and support
6. **🔒 Security** - Built-in security features and best practices

## 📦 Prerequisites

### Required Software

- **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** or later
- **[Visual Studio 2024](https://visualstudio.microsoft.com/)**, **[VS Code](https://code.visualstudio.com/)**, or **[JetBrains Rider](https://www.jetbrains.com/rider/)**
- **[Git](https://git-scm.com/)**
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** (for containerization)
- **[Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)** (for deployment)

### Azure Resources

- **Azure Subscription** with appropriate permissions
- **Azure AI Foundry** (formerly Azure OpenAI Service)
- **Azure AI Search**
- **Azure Cosmos DB**
- **Azure Storage Account**
- **Application Insights**

### Knowledge Prerequisites

- Basic C# programming knowledge
- Understanding of async/await patterns
- Familiarity with REST APIs and WebSockets
- Basic understanding of Azure services

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/ponarunkumar/TechWorkshop-L300-AI-Apps-and-agents.git
cd TechWorkshop-L300-AI-Apps-and-agents
```

### 2. Deploy Azure Resources

Follow the instructions in [Exercise 01: Deploy and Configure Resources](docs/csharp/01_deploy_configure_resources/01_deploy_configure_resources.md) to set up your Azure infrastructure.

**Quick Deploy:**
```bash
cd infra
az deployment group create \
  --resource-group <your-resource-group> \
  --template-file DeployAzureResources.bicep \
  --parameters location=eastus
```

### 3. Configure Application

#### Option A: Using User Secrets (Recommended for Development)

```bash
cd src/ZavaChat.Web

# Set your configuration values
dotnet user-secrets set "AzureAI:ProjectEndpoint" "https://your-project.cognitiveservices.azure.com"
dotnet user-secrets set "AzureAI:OpenAI:Endpoint" "https://your-openai.openai.azure.com"
dotnet user-secrets set "AzureAI:OpenAI:ApiKey" "your-api-key"
dotnet user-secrets set "AzureSearch:Endpoint" "https://your-search.search.windows.net"
dotnet user-secrets set "AzureSearch:ApiKey" "your-search-key"
dotnet user-secrets set "CosmosDB:Endpoint" "https://your-cosmos.documents.azure.com:443/"
dotnet user-secrets set "CosmosDB:Key" "your-cosmos-key"
dotnet user-secrets set "ApplicationInsights:ConnectionString" "your-app-insights-connection"
```

#### Option B: Using appsettings.json (Not Recommended for Production)

```bash
cp src/ZavaChat.Web/appsettings.sample.json src/ZavaChat.Web/appsettings.Development.json
# Edit appsettings.Development.json with your values
```

### 4. Restore Dependencies

```bash
dotnet restore
```

### 5. Build the Solution

```bash
dotnet build
```

### 6. Run Tests

```bash
dotnet test
```

### 7. Run the Application

```bash
cd src/ZavaChat.Web
dotnet run
```

Open your browser to `https://localhost:5001` 🎉

## 📁 Project Structure

```
TechWorkshop-L300-AI-Apps-and-agents/
├── src/
│   ├── ZavaChat.Web/                      # ASP.NET Core Web Application
│   │   ├── Program.cs                     # Application entry point
│   │   ├── Hubs/
│   │   │   └── ChatHub.cs                 # SignalR hub for real-time chat
│   │   ├── wwwroot/                       # Static files (HTML, CSS, JS)
│   │   └── appsettings.json               # Configuration
│   │
│   ├── ZavaChat.Agents/                   # AI Agents Library
│   │   ├── AgentProcessor.cs              # Base agent processor
│   │   ├── InteriorDesignerAgent.cs       # Interior design agent
│   │   ├── CustomerLoyaltyAgent.cs        # Customer loyalty agent
│   │   ├── InventoryAgent.cs              # Inventory management agent
│   │   └── CoraAgent.cs                   # Conversational agent
│   │
│   ├── ZavaChat.Tools/                    # AI Tools Library
│   │   ├── AISearchTools.cs               # Azure AI Search integration
│   │   ├── ImageCreationTool.cs           # Image generation
│   │   ├── ImageUnderstandingTool.cs      # Image analysis
│   │   ├── InventoryCheckTool.cs          # Inventory checking
│   │   └── DiscountCalculator.cs          # Discount calculation
│   │
│   ├── ZavaChat.Services/                 # Business Services Library
│   │   ├── AgentService.cs                # Agent orchestration
│   │   ├── HandoffService.cs              # Agent handoff logic
│   │   └── FallbackService.cs             # Fallback responses
│   │
│   ├── ZavaChat.A2A/                      # Agent-to-Agent Protocol
│   │   ├── Server/                        # A2A server implementation
│   │   ├── Client/                        # A2A client implementation
│   │   └── Models/                        # A2A message models
│   │
│   └── ZavaChat.Core/                     # Core Models & Utilities
│       ├── Models/                        # Data models
│       │   ├── ChatRequest.cs
│       │   ├── ChatResponse.cs
│       │   ├── Product.cs
│       │   └── ...
│       └── Utilities/                     # Utility classes
│           ├── HistoryUtils.cs
│           ├── MessageUtils.cs
│           └── ResponseUtils.cs
│
├── tests/
│   └── ZavaChat.Tests/                    # Unit and Integration Tests
│       ├── AgentTests/
│       ├── ToolTests/
│       ├── ServiceTests/
│       └── IntegrationTests/
│
├── docs/
│   └── csharp/                            # C# specific documentation
│       ├── 01_deploy_configure_resources/
│       ├── 02_implement_multimodal_ai_shopping_assistant/
│       ├── 03_extend_shopping_assistant_with_a2a/
│       ├── 04_observability_ai_foundry/
│       ├── 05_agentic_devops/
│       ├── 06_red_teaming_troubleshooting/
│       └── 07_resource_cleanup/
│
├── infra/                                 # Infrastructure as Code
│   └── DeployAzureResources.bicep         # Azure Bicep template
│
├── prompts/                               # AI Agent Prompts
│   ├── CoraPrompt.txt
│   ├── InteriorDesignAgentPrompt.txt
│   └── ...
│
├── .github/
│   └── workflows/                         # CI/CD Pipelines
│       ├── dotnet-build.yml
│       └── azure-deploy.yml
│
├── ZavaChat.sln                           # Visual Studio Solution
├── global.json                            # .NET SDK version
├── Directory.Build.props                  # Common project properties
├── README-CSHARP.md                       # This file
├── CSHARP_CONVERSION_PLAN.md             # Conversion plan document
├── TECHNICAL_SPECIFICATION.md             # Technical specifications
└── PROJECT_ROADMAP.md                     # Implementation roadmap
```

## 🏗️ Architecture

### High-Level Architecture

```
┌─────────────────┐
│   Web Browser   │
└────────┬────────┘
         │ SignalR (WebSocket)
         │
┌────────▼────────────────────────────────────────┐
│          ASP.NET Core Web Application           │
│  ┌──────────────────────────────────────────┐  │
│  │            ChatHub (SignalR)             │  │
│  └───────┬──────────────────────────────────┘  │
│          │                                      │
│  ┌───────▼──────────┐  ┌──────────────────┐   │
│  │  HandoffService  │  │  FallbackService │   │
│  └───────┬──────────┘  └──────────────────┘   │
│          │                                      │
│  ┌───────▼──────────────────────────────────┐  │
│  │          AgentService                    │  │
│  └───────┬──────────────────────────────────┘  │
└──────────┼──────────────────────────────────────┘
           │
    ┌──────▼──────┐
    │   Agents    │
    ├─────────────┤
    │ Interior    │
    │ Designer    │
    ├─────────────┤
    │ Customer    │
    │ Loyalty     │
    ├─────────────┤
    │ Inventory   │
    ├─────────────┤
    │ Cora        │
    └──────┬──────┘
           │
    ┌──────▼──────────────────────────────┐
    │          Azure AI Foundry           │
    │  ┌────────────┐  ┌──────────────┐  │
    │  │ OpenAI GPT │  │  Phi-4 Model │  │
    │  └────────────┘  └──────────────┘  │
    └─────────────────────────────────────┘
           │
    ┌──────▼───────────────────────────────┐
    │        Azure Services                │
    │  ┌─────────────┐  ┌──────────────┐  │
    │  │  AI Search  │  │  Cosmos DB   │  │
    │  └─────────────┘  └──────────────┘  │
    │  ┌─────────────┐  ┌──────────────┐  │
    │  │   Storage   │  │ App Insights │  │
    │  └─────────────┘  └──────────────┘  │
    └──────────────────────────────────────┘
```

### Key Design Patterns

- **🏭 Factory Pattern** - Agent creation and configuration
- **💉 Dependency Injection** - Service registration and resolution
- **🔌 Repository Pattern** - Data access abstraction
- **🎯 Strategy Pattern** - Agent selection and routing
- **🔄 Observer Pattern** - SignalR real-time updates
- **⚡ Circuit Breaker** - Fault tolerance with Polly
- **📦 Options Pattern** - Configuration management

## 📚 Exercise Modules

### Exercise 01: Deploy and Configure Resources
**Duration**: 30 minutes  
Learn how to deploy Azure AI Foundry and related services using Bicep.

📖 [Go to Exercise 01](docs/csharp/01_deploy_configure_resources/01_deploy_configure_resources.md)

### Exercise 02: Implement a Multimodal AI Shopping Assistant
**Duration**: 90 minutes  
Build agents that can process text, images, and videos for a retail scenario.

📖 [Go to Exercise 02](docs/csharp/02_implement_multimodal_ai_shopping_assistant/02_implement_multimodal_ai_shopping_assistant.md)

### Exercise 03: Extend with Agent-to-Agent (A2A) Protocol
**Duration**: 60 minutes  
Enable your agents to communicate with each other and with external agents.

📖 [Go to Exercise 03](docs/csharp/03_extend_shopping_assistant_with_a2a/03_extend_shopping_assistant_with_a2a.md)

### Exercise 04: Observability in AI Foundry
**Duration**: 45 minutes  
Implement monitoring, logging, and tracing for your AI application.

📖 [Go to Exercise 04](docs/csharp/04_observability_ai_foundry/04_observability_ai_foundry.md)

### Exercise 05: Agentic DevOps
**Duration**: 60 minutes  
Set up CI/CD pipelines for automated testing and deployment.

📖 [Go to Exercise 05](docs/csharp/05_agentic_devops/05_agentic_devops.md)

### Exercise 06: Red Teaming and Troubleshooting
**Duration**: 60 minutes  
Test your AI application for vulnerabilities and learn troubleshooting techniques.

📖 [Go to Exercise 06](docs/csharp/06_red_teaming_troubleshooting/06_red_teaming_troubleshooting.md)

### Exercise 07: Resource Cleanup
**Duration**: 15 minutes  
Clean up Azure resources to avoid unnecessary charges.

📖 [Go to Exercise 07](docs/csharp/07_resource_cleanup/07_resource_cleanup.md)

## 🛠️ Development Guide

### Running Locally

#### Using Visual Studio
1. Open `ZavaChat.sln`
2. Set `ZavaChat.Web` as startup project
3. Press F5 to run with debugging

#### Using VS Code
1. Open the repository folder
2. Install recommended extensions
3. Press F5 or use `dotnet run`

#### Using Command Line
```bash
cd src/ZavaChat.Web
dotnet watch run  # Hot reload enabled
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsFormat=opencover

# Run specific test category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
```

### Debugging

#### Attach to Process (VS Code)
```json
{
  "type": "coreclr",
  "request": "attach",
  "processId": "${command:pickProcess}"
}
```

#### Enable Detailed Logging
```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "ZavaChat": "Trace"
  }
}
```

### Code Quality Tools

```bash
# Format code
dotnet format

# Analyze code
dotnet tool install -g dotnet-security-guard
dotnet security-guard analyze

# Check for outdated packages
dotnet list package --outdated
```

## 🚀 Deployment

### Docker Deployment

#### Build Image
```bash
docker build -t zavachat:latest -f src/ZavaChat.Web/Dockerfile .
```

#### Run Container
```bash
docker run -d -p 8080:8080 \
  -e AzureAI__ProjectEndpoint="https://..." \
  -e AzureAI__OpenAI__ApiKey="..." \
  zavachat:latest
```

### Azure Container Apps Deployment

```bash
# Login to Azure
az login

# Create Azure Container Apps
az containerapp up \
  --name zavachat \
  --resource-group <your-rg> \
  --location eastus \
  --source .

# Configure environment variables
az containerapp update \
  --name zavachat \
  --resource-group <your-rg> \
  --set-env-vars \
    "AzureAI__ProjectEndpoint=https://..." \
    "AzureAI__OpenAI__ApiKey=secretref:openai-key"
```

### Azure App Service Deployment

```bash
# Publish app
dotnet publish src/ZavaChat.Web/ZavaChat.Web.csproj \
  -c Release \
  -o ./publish

# Deploy to App Service
az webapp up \
  --name zavachat-app \
  --resource-group <your-rg> \
  --runtime "DOTNET|10.0"
```

### CI/CD with GitHub Actions

The repository includes GitHub Actions workflows for automated build and deployment. See [`.github/workflows/`](.github/workflows/) for details.

## 🔧 Troubleshooting

### Common Issues

#### Issue: "Could not find Azure AI Project endpoint"
**Solution**: Ensure `AzureAI:ProjectEndpoint` is configured in user secrets or appsettings.json

```bash
dotnet user-secrets set "AzureAI:ProjectEndpoint" "https://your-project.cognitiveservices.azure.com"
```

#### Issue: SignalR connection fails
**Solution**: Check CORS settings and ensure WebSocket is enabled

```csharp
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
```

#### Issue: High memory usage
**Solution**: Enable garbage collection and adjust heap settings

```json
"System.GC.Server": true,
"System.GC.Concurrent": true
```

#### Issue: Slow agent responses
**Solution**: Enable response caching and optimize tool calls

```csharp
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();
```

### Getting Help

- 📖 Check the [docs/csharp/](docs/csharp/) folder for detailed guides
- 🐛 [Open an issue](https://github.com/ponarunkumar/TechWorkshop-L300-AI-Apps-and-agents/issues) on GitHub
- 💬 Ask questions in [Discussions](https://github.com/ponarunkumar/TechWorkshop-L300-AI-Apps-and-agents/discussions)
- 📧 Contact the maintainers

## 🤝 Contributing

We welcome contributions! Here's how you can help:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

### Contribution Guidelines

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting
- Keep PR scope focused and manageable

## 📚 Resources

### Official Documentation
- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-services/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr/)
- [C# Language Reference](https://docs.microsoft.com/dotnet/csharp/)

### Learning Resources
- [C# Learning Path](https://docs.microsoft.com/learn/dotnet/)
- [Azure AI Learning Path](https://docs.microsoft.com/learn/azure/ai/)
- [Building AI Apps with .NET](https://dotnet.microsoft.com/apps/ai)

### Community
- [.NET Community](https://dotnet.microsoft.com/platform/community)
- [Azure AI Community](https://techcommunity.microsoft.com/azure/ai/)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Original Python implementation contributors
- Microsoft Azure AI team
- .NET community
- All workshop participants and feedback providers

---

**⭐ If you find this helpful, please give it a star!**

**📢 Share this with other C# developers interested in AI!**

**💡 Have questions or suggestions? Open an issue!**
