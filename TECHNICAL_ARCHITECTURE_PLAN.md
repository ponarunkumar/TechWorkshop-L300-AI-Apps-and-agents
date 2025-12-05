# Technical Architecture Plan: C#14/.NET 10 with Microsoft Agent Framework

## Document Purpose

This document provides a **pure technical architecture and implementation approach** for converting the Python-based TechWorkshop L300 AI Apps and Agents to C#14/.NET 10, focusing on technology stack decisions, architectural patterns, and implementation strategies without budget considerations.

---

## 🎯 Recommended Technology Stack

### Final Architecture Decision

After comprehensive analysis of modern .NET frameworks including .NET Aspire, Blazor, MAUI, Semantic Kernel, AutoGen.NET, and Microsoft Agent Framework, the recommended stack is:

```
┌──────────────────────────────────────────────────┐
│           .NET Aspire Orchestration              │
│     (Service Discovery + Observability)          │
└──────────────────────────────────────────────────┘
                      │
    ┌─────────────────┼─────────────────┐
    │                 │                 │
┌───▼────────┐  ┌────▼────────┐  ┌────▼────────┐
│   Web UI   │  │ Agent Host  │  │ Backend API │
│ (HTML/JS)  │  │  (MAF*)     │  │ (ASP.NET)   │
└────────────┘  └─────────────┘  └─────────────┘
                      │
        ┌─────────────┼─────────────┐
        │             │             │
    ┌───▼──┐      ┌──▼───┐     ┌──▼────┐
    │Agent1│      │Agent2│     │Agent3 │
    └──────┘      └──────┘     └───────┘

* MAF = Microsoft Agent Framework
```

### Core Technology Components

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Runtime** | .NET 10 | Latest LTS with C# 14 features |
| **Orchestration** | .NET Aspire | Cloud-native service orchestration |
| **Agent Framework** | Microsoft Agent Framework | Core agent runtime and workflows |
| **Web Framework** | ASP.NET Core | Backend API services |
| **Real-time Comm** | SignalR | WebSocket-based real-time updates |
| **Frontend** | HTML/JS | Primary UI (with optional Blazor track) |
| **AI Integration** | Azure AI Foundry SDK | LLM and model access |
| **Search** | Azure AI Search SDK | Product search capabilities |
| **Database** | Azure Cosmos DB SDK | Data persistence |
| **Observability** | OpenTelemetry + App Insights | Monitoring and tracing |

---

## 🏗️ Architecture Deep Dive

### 1. Microsoft Agent Framework (Core)

**Why This Framework?**
- ✅ **Official Microsoft Framework** - Production-ready, not experimental
- ✅ **Agent-First Design** - Built specifically for AI agent development
- ✅ **Graph-Based Workflows** - Declarative orchestration patterns
- ✅ **Native Multi-Agent** - Built-in agent coordination
- ✅ **A2A Protocol Support** - Standard agent-to-agent communication
- ✅ **Built-in Observability** - OpenTelemetry integration out-of-the-box
- ✅ **DevUI Included** - Interactive development and debugging interface

**Key Capabilities:**
- Agent runtime with multiple LLM provider support
- Graph-based workflow engine
- Checkpointing and resume functionality
- Human-in-the-loop (HITL) support
- Time-travel debugging
- Middleware pipeline
- Durable workflows for long-running operations

### 2. .NET Aspire (Orchestration)

**Purpose:**
Provides cloud-native orchestration for distributed applications with built-in service discovery, configuration management, and observability.

**Benefits:**
- Automatic service discovery
- Centralized configuration
- Built-in telemetry and distributed tracing
- Simplified Azure resource integration
- Developer dashboard for local development
- Production deployment patterns

**Integration:**
```csharp
// Aspire orchestrates all services
var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");
var aiFoundry = builder.AddAzureAIFoundry("ai-foundry");
var agentHost = builder.AddProject<AgentHost>("agenthost")
    .WithReference(aiFoundry);
var api = builder.AddProject<ApiService>("api")
    .WithReference(redis)
    .WithReference(agentHost);
```

### 3. ASP.NET Core + SignalR (Backend)

**Architecture:**
- Minimal APIs for REST endpoints
- SignalR for real-time bidirectional communication
- Dependency injection for service management
- Middleware pipeline for cross-cutting concerns

**Benefits:**
- High performance and scalability
- Built-in authentication and authorization
- CORS support for web clients
- Automatic model validation
- Built-in health checks

---

## 📦 Project Structure

### Recommended Solution Organization

```
ZavaChat.sln
│
├── ZavaChat.AppHost/                      [.NET Aspire Orchestrator]
│   ├── Program.cs                         → Service configuration
│   └── appsettings.json                   → Environment settings
│
├── ZavaChat.ServiceDefaults/              [Shared Configuration]
│   ├── Extensions.cs                      → Common extensions
│   └── TelemetryExtensions.cs             → Observability setup
│
├── ZavaChat.AgentHost/                    [Agent Runtime Host]
│   ├── Agents/                            → Agent implementations
│   │   ├── InteriorDesignerAgent.cs
│   │   ├── CustomerLoyaltyAgent.cs
│   │   ├── InventoryAgent.cs
│   │   └── CoraAgent.cs
│   ├── Tools/                             → Agent tools/functions
│   │   ├── ProductSearchTool.cs
│   │   ├── ImageAnalysisTool.cs
│   │   └── InventoryCheckTool.cs
│   ├── Workflows/                         → Graph-based workflows
│   │   ├── ShoppingWorkflow.cs
│   │   └── A2AWorkflow.cs
│   ├── Middleware/                        → Processing pipeline
│   │   ├── ContentFilterMiddleware.cs
│   │   └── ObservabilityMiddleware.cs
│   └── Program.cs                         → Agent host startup
│
├── ZavaChat.ApiService/                   [Backend API]
│   ├── Hubs/                              → SignalR hubs
│   │   └── ChatHub.cs
│   ├── Controllers/                       → REST endpoints
│   │   └── AgentsController.cs
│   ├── Services/                          → Business logic
│   │   ├── AgentOrchestrator.cs
│   │   └── SessionManager.cs
│   └── Program.cs                         → API startup
│
├── ZavaChat.Web/                          [Frontend]
│   ├── wwwroot/
│   │   ├── index.html                     → Main UI
│   │   ├── chat.js                        → SignalR client
│   │   └── styles.css                     → Styling
│   └── Program.cs                         → Static file server
│
├── ZavaChat.Core/                         [Shared Library]
│   ├── Models/                            → Data models
│   │   ├── ChatMessage.cs
│   │   ├── Product.cs
│   │   ├── AgentResponse.cs
│   │   └── WorkflowState.cs
│   ├── Interfaces/                        → Contracts
│   │   ├── IAgent.cs
│   │   ├── IAgentTool.cs
│   │   └── IWorkflow.cs
│   └── Constants/                         → Shared constants
│
└── ZavaChat.Tests/                        [Test Project]
    ├── AgentTests/                        → Agent unit tests
    ├── WorkflowTests/                     → Workflow tests
    ├── IntegrationTests/                  → E2E tests
    └── TestFixtures/                      → Test utilities
```

---

## 💻 Technical Implementation Patterns

### Pattern 1: Agent Definition

```csharp
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Abstractions;

public class InteriorDesignerAgent : IAgent
{
    private readonly AzureOpenAIClient _aiClient;
    private readonly ILogger<InteriorDesignerAgent> _logger;
    
    public string Name => "InteriorDesigner";
    public string Description => "Expert in interior design and paint colors";

    public async Task<AgentResponse> ProcessAsync(
        AgentRequest request,
        CancellationToken cancellationToken = default)
    {
        // Configure agent with instructions and tools
        var agentConfig = new AgentConfiguration
        {
            Name = Name,
            Instructions = await LoadInstructionsAsync(),
            Tools = GetTools(),
            Model = "gpt-4o"
        };

        var agent = await _aiClient.CreateAgentAsync(agentConfig);
        var thread = await _aiClient.CreateThreadAsync();
        
        // Add user message
        await _aiClient.AddMessageAsync(
            thread.Id,
            MessageRole.User,
            request.Message);

        // Execute with streaming
        await foreach (var update in _aiClient.StreamRunAsync(
            thread.Id,
            agent.Id,
            cancellationToken))
        {
            if (update.IsComplete)
            {
                return await BuildResponseAsync(thread.Id);
            }
        }
    }

    private AgentTool[] GetTools()
    {
        return new[]
        {
            CreateFunctionTool<ProductSearchTool>("search_products"),
            CreateFunctionTool<ImageAnalysisTool>("analyze_image")
        };
    }
}
```

### Pattern 2: Graph-Based Workflow

```csharp
using Microsoft.Agents.AI.Workflows;

public class ShoppingWorkflow : WorkflowBase
{
    protected override void BuildWorkflow(WorkflowBuilder builder)
    {
        builder
            // Entry point - determine which agent to use
            .AddNode("handoff", async (ctx) => 
                await DetermineAgentAsync(ctx))
            
            // Interior designer path
            .AddNode("interiorDesigner", async (ctx) =>
                await ExecuteDesignerAgentAsync(ctx))
            
            // Check inventory for products
            .AddNode("checkInventory", async (ctx) =>
                await CheckProductInventoryAsync(ctx))
            
            // Apply customer discount
            .AddNode("applyDiscount", async (ctx) =>
                await CalculateDiscountAsync(ctx))
            
            // Final response assembly
            .AddNode("assembleResponse", async (ctx) =>
                await AssembleResponseAsync(ctx))
            
            // Define workflow edges (graph connections)
            .SetStartNode("handoff")
            .AddEdge("handoff", "interiorDesigner", 
                condition: ctx => ctx.SelectedAgent == "interior")
            .AddEdge("interiorDesigner", "checkInventory",
                condition: ctx => ctx.HasProducts)
            .AddEdge("checkInventory", "applyDiscount")
            .AddEdge("applyDiscount", "assembleResponse");
    }
}
```

### Pattern 3: A2A Protocol Implementation

```csharp
using Microsoft.Agents.AI.A2A;

public class ProductManagementA2AAgent : IA2AAgent
{
    public AgentCard GetAgentCard()
    {
        return new AgentCard
        {
            Name = "Product Management Agent",
            Description = "Manages product catalog and inventory",
            Version = "1.0.0",
            Capabilities = new[]
            {
                new AgentCapability
                {
                    Name = "search_products",
                    Description = "Search product catalog",
                    InputSchema = JsonSchema.CreateForType<SearchRequest>()
                },
                new AgentCapability
                {
                    Name = "check_inventory",
                    Description = "Check product availability",
                    InputSchema = JsonSchema.CreateForType<InventoryRequest>()
                }
            }
        };
    }

    public async Task<AgentResponse> InvokeAsync(
        A2ARequest request,
        CancellationToken cancellationToken = default)
    {
        return request.Capability switch
        {
            "search_products" => await SearchProductsAsync(request),
            "check_inventory" => await CheckInventoryAsync(request),
            _ => throw new NotSupportedException(
                $"Capability '{request.Capability}' not supported")
        };
    }
}
```

### Pattern 4: SignalR Integration

```csharp
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    private readonly IWorkflowOrchestrator _orchestrator;
    
    public async Task SendMessage(ChatRequest request)
    {
        // Execute workflow with streaming updates
        await foreach (var update in _orchestrator.ExecuteAsync<ShoppingWorkflow>(
            request,
            Context.CancellationToken))
        {
            // Stream intermediate updates to client
            await Clients.Caller.SendAsync(
                "ReceiveUpdate",
                new StreamUpdate
                {
                    Type = update.Type,
                    Content = update.Content,
                    Agent = update.AgentName
                });
        }

        // Send final result
        var result = await _orchestrator.GetResultAsync<ShoppingWorkflow>(
            request.SessionId);
        
        await Clients.Caller.SendAsync("ReceiveMessage", result);
    }
}
```

### Pattern 5: Aspire Configuration

```csharp
// ZavaChat.AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure resources
var redis = builder.AddRedis("redis")
    .WithDataVolume()
    .WithRedisInsight();

var cosmos = builder.AddAzureCosmosDB("cosmos")
    .AddDatabase("zavadb");

var aiFoundry = builder.AddAzureAIFoundry("ai-foundry");
var aiSearch = builder.AddAzureAISearch("ai-search");

// Application services
var agentHost = builder.AddProject<Projects.ZavaChat_AgentHost>("agenthost")
    .WithReference(aiFoundry)
    .WithReference(aiSearch)
    .WithReference(cosmos)
    .WithEnvironment("AGENT_TELEMETRY_ENABLED", "true")
    .WithEnvironment("DEVUI_ENABLED", "true");

var apiService = builder.AddProject<Projects.ZavaChat_ApiService>("apiservice")
    .WithReference(redis)
    .WithReference(agentHost);

var webFrontend = builder.AddProject<Projects.ZavaChat_Web>("webfrontend")
    .WithReference(apiService)
    .WithExternalHttpEndpoints();

builder.Build().Run();
```

---

## 📚 Technology Comparison

### Microsoft Agent Framework vs Alternatives

| Feature | Microsoft Agent Framework | Semantic Kernel | AutoGen.NET |
|---------|---------------------------|-----------------|-------------|
| **Primary Purpose** | Agent-first development | AI orchestration | Multi-agent research |
| **Maturity Level** | Production-ready | GA (General Availability) | Experimental |
| **Workflow Type** | Graph-based declarative | Imperative/manual | Conversation-based |
| **Multi-Agent Support** | Native orchestration | Manual coordination | Native but limited |
| **A2A Protocol** | Built-in support | None | None |
| **Observability** | Built-in OpenTelemetry | Manual setup required | Limited |
| **DevUI** | Included | None | None |
| **HITL Support** | Built-in | Custom implementation | Limited |
| **Checkpointing** | Built-in | Custom implementation | None |
| **Azure Integration** | Native Azure AI Foundry | Good | Limited |
| **Strategic Direction** | Microsoft's agent future | AI orchestration tool | Research project |

**Recommendation:** Microsoft Agent Framework is the clear choice for this workshop as it's purpose-built for agent development and represents Microsoft's strategic direction.

---

## 🔧 Key Technical Features

### 1. Graph-Based Workflows

**Benefits:**
- Visual representation of agent flow
- Declarative configuration
- Easy to understand and modify
- Conditional branching support
- Parallel execution capabilities

**Example Flow:**
```
User Input → Handoff → [Designer | Inventory | Loyalty] → Combine → Response
```

### 2. Built-in Observability

**Capabilities:**
- Distributed tracing with OpenTelemetry
- Automatic span creation for agent calls
- Performance metrics collection
- Custom event logging
- Application Insights integration

**Implementation:**
```csharp
// Automatic tracing
using var activity = ActivitySource.StartActivity("AgentExecution");
activity?.SetTag("agent.name", agentName);
activity?.SetTag("agent.model", modelName);
```

### 3. A2A Protocol Support

**Features:**
- Standard agent discovery
- Agent capability cards
- Cross-agent invocation
- Message routing
- Protocol versioning

**Use Cases:**
- External agent integration
- Microservices communication
- Agent marketplace support
- Third-party agent usage

### 4. DevUI Development Interface

**Capabilities:**
- Interactive agent testing
- Workflow visualization
- Real-time debugging
- Conversation inspection
- Performance profiling
- Agent registry browser

### 5. Middleware Pipeline

**Supported Middleware:**
- Content filtering
- Rate limiting
- Authentication/authorization
- Logging and monitoring
- Error handling
- Request transformation

---

## 🎓 Implementation Phases

### Phase Overview

| Phase | Focus Area | Key Deliverables |
|-------|------------|------------------|
| **1** | Foundation Setup | Solution structure, Aspire config, Agent Framework integration |
| **2** | Core Agents | 4 agents implemented with tools and middleware |
| **3** | Workflows | Graph-based multi-agent orchestration |
| **4** | A2A Protocol | Agent cards, discovery, cross-agent communication |
| **5** | Web Integration | SignalR hub, frontend, streaming |
| **6** | Observability | OpenTelemetry, Application Insights, dashboards |
| **7** | Testing | Unit, integration, E2E, load testing |
| **8** | Documentation | Exercise conversion, guides, runbooks |
| **9** | Deployment | Container optimization, Azure deployment |
| **10** | Refinement | Performance tuning, feedback integration |

### Phase 1: Foundation (Technical Setup)

**Objectives:**
- Create .NET 10 solution with all projects
- Configure .NET Aspire orchestration
- Install Microsoft Agent Framework packages
- Set up development environment
- Configure CI/CD pipeline

**Technical Tasks:**
```bash
# Create solution
dotnet new sln -n ZavaChat

# Create projects
dotnet new aspire-apphost -n ZavaChat.AppHost
dotnet new classlib -n ZavaChat.AgentHost
dotnet new web -n ZavaChat.ApiService
dotnet new blazorwasm -n ZavaChat.Web
dotnet new classlib -n ZavaChat.Core
dotnet new xunit -n ZavaChat.Tests

# Add packages
dotnet add package Microsoft.Agents.AI
dotnet add package Microsoft.Agents.AI.Workflows
dotnet add package Aspire.Hosting.Azure.AIFoundry
```

### Phase 2: Core Agents (Agent Implementation)

**Objectives:**
- Implement 4 core agents using Agent Framework
- Create agent tools (search, image analysis, inventory)
- Set up agent middleware pipeline
- Add unit tests for agents

**Technical Focus:**
- Agent configuration patterns
- Tool registration and invocation
- Middleware implementation
- Error handling strategies

### Phase 3: Workflows (Orchestration)

**Objectives:**
- Design graph-based shopping workflow
- Implement multi-agent coordination
- Add workflow checkpointing
- Create HITL integration points

**Technical Focus:**
- Workflow graph design
- State management
- Conditional logic
- Error recovery

### Phase 4: A2A Protocol (Inter-Agent Communication)

**Objectives:**
- Implement agent cards
- Set up A2A server endpoints
- Create agent discovery mechanism
- Test cross-agent communication

**Technical Focus:**
- Protocol specification
- Message serialization
- Agent registration
- Discovery patterns

### Phase 5: Web Integration (User Interface)

**Objectives:**
- Implement SignalR hub
- Connect hub to workflow orchestrator
- Create frontend interface
- Add streaming support

**Technical Focus:**
- SignalR protocol
- Streaming patterns
- Session management
- Client-server sync

---

## 🔍 Technology Decision Rationale

### Why Microsoft Agent Framework?

**1. Official Microsoft Framework**
- Not experimental or preview
- Production-ready and supported
- Strategic direction for AI agents
- Integration with Azure ecosystem

**2. Superior Architecture**
- Graph-based workflows vs manual orchestration
- Built-in observability vs custom implementation
- Native A2A protocol vs custom protocol
- Included DevUI vs no debugging tools

**3. Developer Experience**
- Comprehensive samples and documentation
- Migration guides from SK and AutoGen
- Active development and community
- Visual Studio integration

**4. Future-Proof**
- Microsoft's investment and roadmap
- Growing ecosystem
- Integration with future Azure AI features
- Long-term support guarantee

### Why .NET Aspire?

**1. Cloud-Native Design**
- Built specifically for distributed applications
- Service discovery out-of-the-box
- Configuration management
- Simplified deployment

**2. Developer Productivity**
- Local development dashboard
- Automatic telemetry
- Easy Azure resource integration
- Hot reload support

**3. Production Ready**
- Proven deployment patterns
- Scalability built-in
- Monitoring and health checks
- Resource optimization

### Why SignalR?

**1. Better than Raw WebSockets**
- Automatic reconnection
- Fallback transports
- Connection management
- Backplane support for scale-out

**2. .NET Integration**
- Native C# support
- Strongly-typed hubs
- Dependency injection
- Middleware pipeline

**3. Real-time Capabilities**
- Bidirectional communication
- Server-to-client push
- Streaming support
- Group messaging

---

## 🎯 Technical Advantages Summary

### Over Python Implementation

| Aspect | Python | C# with Agent Framework |
|--------|--------|-------------------------|
| **Type Safety** | Dynamic, runtime errors | Static, compile-time safety |
| **Performance** | Interpreted | Compiled, JIT-optimized |
| **Agent Runtime** | Custom implementation | Microsoft Agent Framework |
| **Workflows** | Manual coordination | Graph-based declarative |
| **Observability** | Manual setup | Built-in OpenTelemetry |
| **A2A Protocol** | Custom implementation | Native support |
| **Debugging** | Print statements, logs | DevUI, time-travel debug |
| **IDE Support** | Good | Excellent (IntelliSense, refactoring) |
| **Scalability** | Good | Excellent (compiled, optimized) |
| **Deployment** | Container | Container + Aspire orchestration |

### Key Technical Benefits

1. **Graph-Based Workflows** - Visual, declarative agent orchestration
2. **Built-in Observability** - No manual instrumentation needed
3. **Native A2A Support** - Standard protocol, no custom implementation
4. **DevUI** - Interactive development and debugging
5. **Type Safety** - Compile-time error detection
6. **Performance** - 2-3x faster execution
7. **Tooling** - Superior IDE support and debugging
8. **Enterprise Features** - Checkpointing, HITL, durable workflows

---

## 📋 Optional Enhancements

### Option 1: Blazor Frontend (Advanced Track)

**When to Consider:**
- Want pure C# development experience
- Need component-based UI architecture
- Prefer strongly-typed frontend
- Target C# developers specifically

**Trade-offs:**
- Higher learning curve
- Larger initial bundle size (Blazor WASM)
- Less familiar to web developers

### Option 2: MAUI Mobile App

**When to Consider:**
- Mobile app requirement emerges
- Need offline-first capabilities
- Want native device features

**Current Recommendation:**
- Not recommended for initial implementation
- Web app is mobile-responsive
- Consider PWA for app-like experience

---

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK
- Visual Studio 2024 / VS Code / Rider
- Docker Desktop
- Azure subscription
- Azure CLI

### Quick Start

```bash
# Clone repository
git clone https://github.com/your-repo/zavachat.git
cd zavachat

# Restore packages
dotnet restore

# Run with Aspire
cd src/ZavaChat.AppHost
dotnet run

# Access applications
# - Aspire Dashboard: https://localhost:15000
# - Web UI: https://localhost:5001
# - Agent DevUI: https://localhost:5002/devui
```

### Development Workflow

1. **Local Development**
   - Use Aspire dashboard to monitor services
   - Use Agent DevUI to test agents
   - Use SignalR for real-time testing

2. **Testing**
   - Unit tests for agents and tools
   - Integration tests for workflows
   - E2E tests for complete scenarios

3. **Deployment**
   - Build containers with Docker
   - Deploy to Azure Container Apps
   - Configure Azure resources with Bicep

---

## 📖 Learning Resources

### Microsoft Agent Framework
- Official docs: https://learn.microsoft.com/agent-framework
- GitHub repo: https://github.com/microsoft/agent-framework
- Samples: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples

### .NET Aspire
- Official docs: https://learn.microsoft.com/dotnet/aspire
- Getting started: https://learn.microsoft.com/dotnet/aspire/get-started

### SignalR
- Official docs: https://learn.microsoft.com/aspnet/core/signalr
- Tutorial: https://learn.microsoft.com/aspnet/core/tutorials/signalr

---

## ✅ Final Technical Recommendation

**Adopt the Following Stack:**

✅ **Core Framework**: .NET 10 with C# 14  
✅ **Orchestration**: .NET Aspire for service management  
✅ **Agent Runtime**: Microsoft Agent Framework (official)  
✅ **Backend**: ASP.NET Core + SignalR  
✅ **Frontend**: HTML/JS (primary), optional Blazor track  
✅ **Observability**: OpenTelemetry + Application Insights  

**Key Architectural Patterns:**
- Graph-based agent workflows
- Native A2A protocol
- Streaming real-time responses
- Built-in observability
- DevUI for development

**Technical Advantages:**
- Production-ready official framework
- Superior developer experience
- Better performance and scalability
- Modern cloud-native architecture
- Future-proof technology choices

---

## 📞 Next Steps

**For Technical Teams:**

1. **Review Architecture**
   - Validate technology choices
   - Review project structure
   - Assess technical feasibility

2. **Prototype Phase 1**
   - Set up development environment
   - Create basic solution structure
   - Integrate Agent Framework
   - Validate Aspire configuration

3. **Technical Planning**
   - Detailed sprint planning
   - Technical task breakdown
   - Risk assessment
   - Team skill assessment

4. **Begin Implementation**
   - Follow phased approach
   - Iterative development
   - Continuous testing
   - Regular technical reviews

---

**Status:** ✅ **READY FOR TECHNICAL REVIEW**

**Focus:** Pure technical architecture and implementation approach

**Recommendation:** Proceed with Microsoft Agent Framework + .NET Aspire architecture

---

*Document Version: 1.0 - Technical Focus*  
*Last Updated: 2025-11-21*  
*Focus: Technology Stack & Architecture (Budget-Free)*
