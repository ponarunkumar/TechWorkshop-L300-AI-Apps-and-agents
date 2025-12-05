# ZavaChat - C#/.NET 10 Implementation

## 🎯 Overview

This is the C#14/.NET 10 implementation of the Zava Shopping Assistant, converted from the original Python-based workshop. This implementation showcases modern .NET development with Microsoft Agent Framework, Blazor components, and .NET Aspire orchestration.

## 🏗️ Solution Architecture

```
ZavaChat.sln
├── ZavaChat.Core          # Shared models, enums, and utilities
├── ZavaChat.Agents        # AI agents (Interior Designer, Loyalty, Inventory, Cora)
├── ZavaChat.Tools         # AI tools (Search, Image/Video analysis, etc.)
├── ZavaChat.Services      # Business logic and orchestration
├── ZavaChat.A2A           # Agent-to-Agent protocol
└── ZavaChat.Web           # ASP.NET Core Web API with SignalR
```

## 🚀 Quick Start

### Prerequisites

- .NET 10 SDK (10.0.100 or later)
- Visual Studio 2024 / VS Code / Rider
- Azure subscription (for Azure AI services)

### Build and Run

```bash
# Clone the repository
git clone https://github.com/ponarunkumar/TechWorkshop-L300-AI-Apps-and-agents.git
cd TechWorkshop-L300-AI-Apps-and-agents/csharp-implementation

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run web application
cd ZavaChat.Web
dotnet run
```

The application will be available at `https://localhost:5001` (or the URL shown in console).

## 📋 Implementation Status

### Phase 1: Foundation ✅ COMPLETE
- [x] Solution and project structure
- [x] Core models (ChatMessage, Product, CartItem)
- [x] Enumerations (MessageRole, AgentType, MediaType)
- [x] Configuration models (AzureAIConfig)
- [x] C# 14 features enabled
- [x] Build verification

### Phase 2: Tools & Services 🚧 IN PROGRESS
- [ ] Base tool implementation
- [ ] Product search tool
- [ ] Image/video analysis tools
- [ ] Agent orchestration service
- [ ] State management service

### Phase 3: AI Agents 📋 PLANNED
- [ ] Agent base class
- [ ] Interior Designer agent
- [ ] Customer Loyalty agent
- [ ] Inventory agent
- [ ] Cora (general assistant) agent

### Phase 4: Web Application 📋 PLANNED
- [ ] SignalR hub implementation
- [ ] Chat API endpoints
- [ ] Middleware setup
- [ ] Configuration

### Phase 5: Blazor UI 📋 PLANNED
- [ ] Component structure
- [ ] Chat window component
- [ ] Product display components
- [ ] Shopping cart component

### Phase 6: Microsoft Agent Framework 📋 PLANNED
- [ ] Agent Framework integration
- [ ] Graph-based workflows
- [ ] DevUI setup
- [ ] Observability

### Phase 7: .NET Aspire 📋 PLANNED
- [ ] Aspire app host
- [ ] Service discovery
- [ ] Distributed tracing
- [ ] Dashboard

## 🔧 Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| **Language** | C# | 14 |
| **Runtime** | .NET | 10.0 |
| **Web Framework** | ASP.NET Core | 10.0 |
| **Real-time** | SignalR | 10.0 |
| **AI Agent Framework** | Microsoft Agent Framework | Latest |
| **Orchestration** | .NET Aspire | Latest |
| **UI Framework** | Blazor Server | 10.0 |
| **Logging** | Serilog | Latest |
| **Telemetry** | OpenTelemetry | Latest |

## 📚 Documentation

- **[PHASE1_IMPLEMENTATION_GUIDE.md](PHASE1_IMPLEMENTATION_GUIDE.md)** - Detailed Phase 1 implementation guide with code examples and flowcharts
- **[CSHARP_CONVERSION_PLAN.md](../CSHARP_CONVERSION_PLAN.md)** - Overall conversion strategy
- **[TECHNICAL_SPECIFICATION.md](../TECHNICAL_SPECIFICATION.md)** - Technical specifications
- **[BLAZOR_ARCHITECTURE_PLAN.md](../BLAZOR_ARCHITECTURE_PLAN.md)** - Blazor component architecture

## 🏃 Development

### Build Commands

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build ZavaChat.Core

# Clean build output
dotnet clean

# Restore NuGet packages
dotnet restore
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Code Quality

```bash
# Format code
dotnet format

# Analyze code
dotnet build -v q --no-incremental
```

## 🎨 C# 14 Features Used

- ✅ **File-scoped namespaces** - Cleaner code structure
- ✅ **Required members** - Compile-time enforcement of required properties
- ✅ **Record types** - Immutable data models
- ✅ **Nullable reference types** - Better null safety
- ✅ **Init-only setters** - Immutable after construction
- ✅ **Pattern matching** - Enhanced switch expressions
- ✅ **Global usings** - Reduced boilerplate

## 🔍 Project Details

### ZavaChat.Core

Contains shared models, enums, configuration, and utilities used across all projects.

**Key Types:**
- `ChatMessage` - Chat message model
- `Product` - Product catalog model
- `CartItem` - Shopping cart item
- `MessageRole` - User/Assistant/System enum
- `AgentType` - Agent type enumeration

### ZavaChat.Agents

AI agent implementations using Microsoft Agent Framework.

**Agents:**
- `InteriorDesignerAgent` - Room design recommendations
- `CustomerLoyaltyAgent` - Rewards and loyalty programs
- `InventoryAgent` - Product availability
- `CoraAgent` - General assistant

### ZavaChat.Tools

AI tools for agents to use.

**Tools:**
- `ProductSearchTool` - Search product catalog
- `ImageAnalysisTool` - Analyze uploaded images
- `VideoAnalysisTool` - Process video content
- `WebSearchTool` - Web search integration

### ZavaChat.Services

Business logic and orchestration services.

**Services:**
- `AgentOrchestrationService` - Multi-agent coordination
- `HandoffService` - Agent handoff logic
- `StateManagementService` - Session state management

### ZavaChat.Web

ASP.NET Core web application with SignalR for real-time communication.

**Features:**
- SignalR hub for chat
- REST API endpoints
- Middleware pipeline
- Configuration management

## 🤝 Contributing

1. Follow Phase implementation guides
2. Use C# 14 features
3. Maintain null safety
4. Add XML documentation
5. Write unit tests
6. Follow naming conventions

## 📝 Naming Conventions

- **Classes/Records**: PascalCase (e.g., `ChatMessage`)
- **Methods**: PascalCase (e.g., `GetProductById`)
- **Properties**: PascalCase (e.g., `ProductName`)
- **Fields**: _camelCase (e.g., `_httpClient`)
- **Parameters**: camelCase (e.g., `productId`)
- **Constants**: PascalCase (e.g., `MaxRetryCount`)

## 🐛 Troubleshooting

### Build Errors

```bash
# Clear all build artifacts
dotnet clean
rm -rf */bin */obj

# Restore and rebuild
dotnet restore
dotnet build
```

### NuGet Issues

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore --force
```

## 📞 Support

For issues, questions, or contributions, please refer to the main repository documentation.

## 📄 License

This project follows the license of the parent repository.

---

**Status**: Phase 1 Complete ✅  
**Last Updated**: 2025-11-21  
**Version**: 0.1.0-alpha
