# Technical Specification: C#14/.NET 10 Implementation

## Document Overview

This document provides detailed technical specifications for converting the Python-based TechWorkshop L300 AI Apps and Agents to C#14/.NET 10.

## Table of Contents
1. [Project Structure](#project-structure)
2. [Code Conversion Examples](#code-conversion-examples)
3. [NuGet Package Dependencies](#nuget-package-dependencies)
4. [Configuration Schema](#configuration-schema)
5. [API Endpoints](#api-endpoints)
6. [SignalR Protocol](#signalr-protocol)
7. [Data Models](#data-models)
8. [Performance Targets](#performance-targets)

## Project Structure

### Solution File (ZavaChat.sln)
```xml
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ZavaChat.Web", "src\ZavaChat.Web\ZavaChat.Web.csproj", "{GUID}"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ZavaChat.Agents", "src\ZavaChat.Agents\ZavaChat.Agents.csproj", "{GUID}"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ZavaChat.Tools", "src\ZavaChat.Tools\ZavaChat.Tools.csproj", "{GUID}"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ZavaChat.Services", "src\ZavaChat.Services\ZavaChat.Services.csproj", "{GUID}"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ZavaChat.Core", "src\ZavaChat.Core\ZavaChat.Core.csproj", "{GUID}"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ZavaChat.A2A", "src\ZavaChat.A2A\ZavaChat.A2A.csproj", "{GUID}"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ZavaChat.Tests", "tests\ZavaChat.Tests\ZavaChat.Tests.csproj", "{GUID}"
```

### Directory.Build.props
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
```

### global.json
```json
{
  "sdk": {
    "version": "10.0.0",
    "rollForward": "latestMinor"
  }
}
```

## Code Conversion Examples

### Example 1: Main Application Entry Point

#### Python (chat_app.py)
```python
from fastapi import FastAPI, WebSocket
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential

app = FastAPI()
project_client = AIProjectClient(
    endpoint=project_endpoint,
    credential=DefaultAzureCredential(),
)

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    # Handle messages

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("chat_app:app", host="0.0.0.0", port=8000)
```

#### C# (Program.cs)
```csharp
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.AspNetCore.SignalR;
using ZavaChat.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Register Azure AI Project Client as singleton
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["AzureAI:ProjectEndpoint"]
        ?? throw new InvalidOperationException("Azure AI Project endpoint not configured");
    
    return new AIProjectClient(
        new Uri(endpoint),
        new DefaultAzureCredential()
    );
});

// Add other services
builder.Services.AddSingleton<IAgentService, AgentService>();
builder.Services.AddSingleton<IHandoffService, HandoffService>();
builder.Services.AddScoped<IFallbackService, FallbackService>();

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("ZavaChat.*")
        .AddAzureMonitorTraceExporter(options =>
        {
            options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
        }));

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Map SignalR hub
app.MapHub<ChatHub>("/ws");

// Map health check
app.MapGet("/health", (IConfiguration config) => Results.Ok(new
{
    Status = "healthy",
    Timestamp = DateTime.UtcNow,
    Environment = new
    {
        ProjectEndpointConfigured = !string.IsNullOrEmpty(config["AzureAI:ProjectEndpoint"]),
        OpenAIEndpointConfigured = !string.IsNullOrEmpty(config["AzureAI:OpenAI:Endpoint"]),
        ApplicationInsightsConfigured = !string.IsNullOrEmpty(config["ApplicationInsights:ConnectionString"])
    }
}));

// Map default route
app.MapGet("/", () => Results.File("wwwroot/index.html", "text/html"));

app.Run();
```

### Example 2: SignalR Hub Implementation

#### C# (ChatHub.cs)
```csharp
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;
using ZavaChat.Core.Models;
using ZavaChat.Services;

namespace ZavaChat.Web.Hubs;

/// <summary>
/// SignalR hub for handling real-time chat communication
/// </summary>
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IAgentService _agentService;
    private readonly IHandoffService _handoffService;
    private readonly IFallbackService _fallbackService;
    private readonly ActivitySource _activitySource;
    
    // Session state management (in-memory for now, could be Redis for scale-out)
    private static readonly ConcurrentDictionary<string, ChatSession> _sessions = new();

    public ChatHub(
        ILogger<ChatHub> logger,
        IAgentService agentService,
        IHandoffService handoffService,
        IFallbackService fallbackService,
        ActivitySource activitySource)
    {
        _logger = logger;
        _agentService = agentService;
        _handoffService = handoffService;
        _fallbackService = fallbackService;
        _activitySource = activitySource;
    }

    /// <summary>
    /// Handles incoming chat messages from clients
    /// </summary>
    /// <param name="request">The chat request containing message and metadata</param>
    public async Task SendMessage(ChatRequest request)
    {
        using var activity = _activitySource.StartActivity("ProcessChatMessage");
        activity?.SetTag("message.length", request.Message?.Length ?? 0);
        activity?.SetTag("has.image", request.HasImage);
        activity?.SetTag("has.video", request.HasVideo);

        var sessionId = Context.ConnectionId;
        var session = _sessions.GetOrAdd(sessionId, _ => new ChatSession
        {
            SessionId = sessionId,
            StartTime = DateTime.UtcNow,
            ChatHistory = new Queue<(string Role, string Message)>(capacity: 10)
        });

        try
        {
            _logger.LogInformation(
                "Processing message from session {SessionId}: {MessagePreview}",
                sessionId,
                request.Message?[..Math.Min(50, request.Message.Length)]);

            // Add user message to history
            session.ChatHistory.Enqueue(("user", request.Message ?? string.Empty));

            // Determine which agent should handle this request
            var handoffResult = await _handoffService.DetermineAgentAsync(
                session.ChatHistory,
                request,
                Context.CancellationToken);

            if (handoffResult.IsContentFilterError)
            {
                await Clients.Caller.SendAsync(
                    "ReceiveMessage",
                    new ChatResponse
                    {
                        Answer = "Your message triggered a content filter. Please modify your prompt and try again.",
                        Agent = null,
                        Cart = session.Cart
                    },
                    Context.CancellationToken);
                return;
            }

            if (handoffResult.SelectedAgent == null)
            {
                await Clients.Caller.SendAsync(
                    "ReceiveMessage",
                    new ChatResponse
                    {
                        Answer = "Sorry, I could not determine the right agent to help you.",
                        Agent = null,
                        Cart = session.Cart
                    },
                    Context.CancellationToken);
                return;
            }

            // Process with selected agent
            var response = await _agentService.ProcessWithAgentAsync(
                handoffResult.SelectedAgent,
                handoffResult.AgentType,
                request,
                session,
                Context.CancellationToken);

            // Add bot response to history
            session.ChatHistory.Enqueue(("bot", response.Answer ?? string.Empty));

            // Keep only last 10 messages
            while (session.ChatHistory.Count > 10)
            {
                session.ChatHistory.Dequeue();
            }

            // Send response to client
            await Clients.Caller.SendAsync(
                "ReceiveMessage",
                response,
                Context.CancellationToken);

            _logger.LogInformation(
                "Message processed successfully for session {SessionId} using agent {AgentType}",
                sessionId,
                handoffResult.AgentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing message for session {SessionId}",
                sessionId);

            await Clients.Caller.SendAsync(
                "ReceiveMessage",
                new ChatResponse
                {
                    Answer = "An error occurred while processing your message. Please try again.",
                    Error = ex.Message,
                    Agent = null,
                    Cart = session.Cart
                },
                Context.CancellationToken);
        }
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var sessionId = Context.ConnectionId;
        _logger.LogInformation("Client connected: {SessionId}", sessionId);
        
        await Clients.Caller.SendAsync(
            "Connected",
            new { SessionId = sessionId, Timestamp = DateTime.UtcNow });
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var sessionId = Context.ConnectionId;
        
        if (_sessions.TryRemove(sessionId, out var session))
        {
            var duration = DateTime.UtcNow - session.StartTime;
            _logger.LogInformation(
                "Client disconnected: {SessionId}, Duration: {Duration}s",
                sessionId,
                duration.TotalSeconds);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
```

### Example 3: Agent Processor

#### C# (AgentProcessor.cs)
```csharp
using Azure.AI.Projects;
using Azure.AI.Agents;
using Azure.AI.Agents.Models;
using System.Diagnostics;
using ZavaChat.Core.Models;
using ZavaChat.Tools;

namespace ZavaChat.Agents;

/// <summary>
/// Processes agent interactions with Azure AI Foundry
/// </summary>
public class AgentProcessor : IAgentProcessor
{
    private readonly AIProjectClient _projectClient;
    private readonly string _agentId;
    private readonly string _agentType;
    private readonly string? _threadId;
    private readonly ILogger<AgentProcessor> _logger;
    private readonly ActivitySource _activitySource;
    private ToolSet? _toolset;

    public AgentProcessor(
        AIProjectClient projectClient,
        string agentId,
        string agentType,
        string? threadId,
        ILogger<AgentProcessor> logger,
        ActivitySource activitySource)
    {
        _projectClient = projectClient ?? throw new ArgumentNullException(nameof(projectClient));
        _agentId = agentId ?? throw new ArgumentNullException(nameof(agentId));
        _agentType = agentType ?? throw new ArgumentNullException(nameof(agentType));
        _threadId = threadId;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
        
        InitializeToolset();
    }

    private void InitializeToolset()
    {
        using var activity = _activitySource.StartActivity("InitializeToolset");
        activity?.SetTag("agent.type", _agentType);

        var functions = _agentType switch
        {
            "interior_designer" => new FunctionTool(new[]
            {
                nameof(ImageCreationTool.CreateImage),
                nameof(AISearchTools.GetProductRecommendations)
            }),
            "customer_loyalty" => new FunctionTool(new[]
            {
                nameof(DiscountCalculator.CalculateDiscount)
            }),
            "inventory_agent" => new FunctionTool(new[]
            {
                nameof(InventoryCheckTool.CheckInventory)
            }),
            _ => new FunctionTool(Array.Empty<string>())
        };

        _toolset = new ToolSet();
        _toolset.Add(functions);
        
        _projectClient.Agents.EnableAutoFunctionCalls(_toolset);
        
        _logger.LogDebug(
            "Toolset initialized for agent type {AgentType}",
            _agentType);
    }

    /// <summary>
    /// Runs a conversation with streaming text responses
    /// </summary>
    public async IAsyncEnumerable<string> RunConversationWithTextStreamAsync(
        string inputMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("RunConversationStream");
        activity?.SetTag("agent.id", _agentId);
        activity?.SetTag("agent.type", _agentType);
        activity?.SetTag("message.length", inputMessage.Length);

        var threadId = _threadId ?? (await _projectClient.Agents.CreateThreadAsync(cancellationToken)).Id;

        // Create message
        await _projectClient.Agents.CreateMessageAsync(
            threadId,
            new MessageInput
            {
                Role = MessageRole.User,
                Content = new MessageInputTextBlock { Text = inputMessage }
            },
            cancellationToken);

        // Stream responses
        await foreach (var streamEvent in _projectClient.Agents.CreateRunStreamingAsync(
            threadId,
            _agentId,
            cancellationToken: cancellationToken))
        {
            if (streamEvent is MessageStreamEvent messageEvent)
            {
                if (messageEvent.Text is { } text)
                {
                    yield return text;
                }
            }
        }
    }

    /// <summary>
    /// Runs a conversation with image input
    /// </summary>
    public async Task<string> RunConversationWithImageAsync(
        string inputMessage,
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("RunConversationWithImage");
        activity?.SetTag("agent.id", _agentId);
        activity?.SetTag("agent.type", _agentType);
        activity?.SetTag("has.image", true);

        var threadId = _threadId ?? (await _projectClient.Agents.CreateThreadAsync(cancellationToken)).Id;

        var contentBlocks = new List<MessageInputContentBlock>
        {
            new MessageInputTextBlock { Text = inputMessage },
            new MessageInputImageUrlBlock
            {
                ImageUrl = new MessageImageUrlParam
                {
                    Url = imageUrl,
                    Detail = "high"
                }
            }
        };

        await _projectClient.Agents.CreateMessageAsync(
            threadId,
            new MessageInput
            {
                Role = MessageRole.User,
                Content = contentBlocks
            },
            cancellationToken);

        var run = await _projectClient.Agents.CreateRunAsync(
            threadId,
            _agentId,
            cancellationToken: cancellationToken);

        // Wait for completion
        while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress)
        {
            await Task.Delay(500, cancellationToken);
            run = await _projectClient.Agents.GetRunAsync(threadId, run.Id, cancellationToken);
        }

        // Get messages
        var messages = await _projectClient.Agents.GetMessagesAsync(
            threadId,
            cancellationToken: cancellationToken);

        var assistantMessages = messages.Data
            .Where(m => m.Role == MessageRole.Assistant)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();

        if (assistantMessages.Count == 0)
        {
            return string.Empty;
        }

        var latestMessage = assistantMessages[0];
        var textContent = latestMessage.Content
            .OfType<MessageTextContent>()
            .FirstOrDefault();

        return textContent?.Text?.Value ?? string.Empty;
    }
}
```

### Example 4: AI Search Tools

#### C# (AISearchTools.cs)
```csharp
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using ZavaChat.Core.Models;

namespace ZavaChat.Tools;

/// <summary>
/// Tools for searching products using Azure AI Search
/// </summary>
public class AISearchTools : IAISearchTools
{
    private readonly SearchClient _searchClient;
    private readonly ILogger<AISearchTools> _logger;
    private readonly SearchOptions _searchOptions;

    public AISearchTools(
        SearchClient searchClient,
        IOptions<SearchConfiguration> searchConfig,
        ILogger<AISearchTools> logger)
    {
        _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var config = searchConfig?.Value ?? throw new ArgumentNullException(nameof(searchConfig));
        
        _searchOptions = new SearchOptions
        {
            QueryType = SearchQueryType.Semantic,
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = $"{config.IndexName}-semantic-configuration"
            },
            Size = 8,
            Select = { "ProductID", "ProductName", "ProductCategory", "ProductDescription", 
                      "ImageURL", "ProductPunchLine", "Price" }
        };
    }

    /// <summary>
    /// Get product recommendations based on a natural language query
    /// </summary>
    /// <param name="question">Natural language user query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recommended products</returns>
    public async Task<List<Product>> GetProductRecommendationsAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            _logger.LogWarning("Empty search query received");
            return new List<Product>();
        }

        try
        {
            _logger.LogDebug(
                "Searching for products with query: {Query}",
                question[..Math.Min(50, question.Length)]);

            var searchResults = await _searchClient.SearchAsync<SearchDocument>(
                question,
                _searchOptions,
                cancellationToken);

            var products = new List<Product>();

            await foreach (var result in searchResults.Value.GetResultsAsync())
            {
                products.Add(new Product
                {
                    Id = GetStringValue(result.Document, "ProductID"),
                    Name = GetStringValue(result.Document, "ProductName"),
                    Type = GetStringValue(result.Document, "ProductCategory"),
                    Description = GetStringValue(result.Document, "ProductDescription"),
                    ImageUrl = GetStringValue(result.Document, "ImageURL"),
                    PunchLine = GetStringValue(result.Document, "ProductPunchLine"),
                    Price = GetDecimalValue(result.Document, "Price")
                });
            }

            _logger.LogInformation(
                "Found {Count} products for query: {Query}",
                products.Count,
                question[..Math.Min(50, question.Length)]);

            return products;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(
                ex,
                "Azure Search request failed for query: {Query}",
                question[..Math.Min(50, question.Length)]);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error during product search: {Query}",
                question[..Math.Min(50, question.Length)]);
            throw;
        }
    }

    private static string GetStringValue(SearchDocument document, string key)
    {
        return document.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
    }

    private static decimal GetDecimalValue(SearchDocument document, string key)
    {
        if (document.TryGetValue(key, out var value) && value != null)
        {
            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                return jsonElement.GetDecimal();
            }
            if (decimal.TryParse(value.ToString(), out var result))
            {
                return result;
            }
        }
        return 0m;
    }
}
```

## NuGet Package Dependencies

### ZavaChat.Web.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- ASP.NET Core -->
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="10.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
    
    <!-- Azure SDKs -->
    <PackageReference Include="Azure.AI.Projects" Version="1.1.0" />
    <PackageReference Include="Azure.AI.Agents" Version="1.2.0" />
    <PackageReference Include="Azure.AI.OpenAI" Version="2.3.0" />
    <PackageReference Include="Azure.AI.Inference" Version="1.0.0" />
    <PackageReference Include="Azure.Identity" Version="1.25.1" />
    <PackageReference Include="Azure.Monitor.OpenTelemetry" Version="1.8.1" />
    
    <!-- OpenTelemetry -->
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.10.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.10.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.10.0" />
    
    <!-- Logging -->
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
    <PackageReference Include="Serilog.Sinks.ApplicationInsights" Version="4.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ZavaChat.Agents\ZavaChat.Agents.csproj" />
    <ProjectReference Include="..\ZavaChat.Services\ZavaChat.Services.csproj" />
    <ProjectReference Include="..\ZavaChat.Core\ZavaChat.Core.csproj" />
  </ItemGroup>
</Project>
```

### ZavaChat.Tools.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Azure.Search.Documents" Version="11.6.0" />
    <PackageReference Include="Azure.Cosmos" Version="4.9.0" />
    <PackageReference Include="Azure.Storage.Blobs" Version="12.21.0" />
    <PackageReference Include="SixLabors.ImageSharp" Version="3.1.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="10.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ZavaChat.Core\ZavaChat.Core.csproj" />
  </ItemGroup>
</Project>
```

## Configuration Schema

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ZavaChat": "Debug"
    }
  },
  "AllowedHosts": "*",
  
  "AzureAI": {
    "ProjectEndpoint": "https://your-project.cognitiveservices.azure.com",
    "ApiVersion": "2024-12-01-preview",
    
    "OpenAI": {
      "Endpoint": "https://your-openai.openai.azure.com",
      "ApiKey": "", 
      "ApiVersion": "2024-12-01-preview",
      "GptDeployment": "gpt-4.1",
      "Phi4Deployment": "Phi-4"
    },
    
    "Agents": {
      "InteriorDesigner": "",
      "CustomerLoyalty": "",
      "InventoryAgent": "",
      "Cora": ""
    }
  },
  
  "AzureSearch": {
    "Endpoint": "https://your-search.search.windows.net",
    "ApiKey": "",
    "IndexName": "zava-product-catalog"
  },
  
  "CosmosDB": {
    "Endpoint": "https://your-cosmos.documents.azure.com:443/",
    "Key": "",
    "DatabaseName": "zava",
    "ContainerName": "product_catalog"
  },
  
  "BlobStorage": {
    "ConnectionString": "",
    "ContainerName": "zava"
  },
  
  "ApplicationInsights": {
    "ConnectionString": ""
  },
  
  "SignalR": {
    "HandshakeTimeout": "00:00:15",
    "KeepAliveInterval": "00:00:10",
    "ClientTimeoutInterval": "00:00:30"
  },
  
  "ChatSettings": {
    "MaxHistoryLength": 10,
    "MaxMessageLength": 10000,
    "EnableContentFilter": true
  }
}
```

### User Secrets (for development)
```bash
dotnet user-secrets set "AzureAI:OpenAI:ApiKey" "your-api-key"
dotnet user-secrets set "AzureSearch:ApiKey" "your-search-key"
dotnet user-secrets set "CosmosDB:Key" "your-cosmos-key"
dotnet user-secrets set "ApplicationInsights:ConnectionString" "your-app-insights-connection"
```

## API Endpoints

### REST API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/` | GET | Serves the main HTML page |
| `/health` | GET | Health check endpoint |
| `/api/agents` | GET | List available agents |
| `/api/products/search` | POST | Search products (alternative to SignalR) |

### SignalR Hub Methods

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `SendMessage` | `ChatRequest` | `void` | Sends a chat message |
| `ReceiveMessage` | - | `ChatResponse` | Receives bot response (client subscribes) |
| `Connected` | - | `ConnectionInfo` | Notification on connection |

## SignalR Protocol

### Client to Server: SendMessage
```json
{
  "message": "I need help painting my living room",
  "hasImage": false,
  "imageUrl": null,
  "hasVideo": false,
  "videoUrl": null,
  "cart": [],
  "conversationHistory": "user: Hello\nbot: Hi there!"
}
```

### Server to Client: ReceiveMessage
```json
{
  "answer": "I'd be happy to help you with your living room painting project!",
  "agent": "interior_designer",
  "products": [
    {
      "id": "PROD001",
      "name": "Premium Paint Roller",
      "type": "Tool",
      "price": 19.99,
      "imageUrl": "https://...",
      "description": "...",
      "punchLine": "..."
    }
  ],
  "discountPercentage": "10",
  "imageUrl": null,
  "videoUrl": null,
  "cart": [],
  "additionalData": null,
  "error": null
}
```

## Data Models

### ChatRequest.cs
```csharp
namespace ZavaChat.Core.Models;

public record ChatRequest
{
    public string? Message { get; init; }
    public bool HasImage { get; init; }
    public string? ImageUrl { get; init; }
    public bool HasVideo { get; init; }
    public string? VideoUrl { get; init; }
    public List<CartItem> Cart { get; init; } = new();
    public string? ConversationHistory { get; init; }
}
```

### ChatResponse.cs
```csharp
namespace ZavaChat.Core.Models;

public record ChatResponse
{
    public string? Answer { get; init; }
    public string? Agent { get; init; }
    public List<Product>? Products { get; init; }
    public string? DiscountPercentage { get; init; }
    public string? ImageUrl { get; init; }
    public string? VideoUrl { get; init; }
    public List<CartItem> Cart { get; init; } = new();
    public string? AdditionalData { get; init; }
    public string? Error { get; init; }
}
```

### Product.cs
```csharp
namespace ZavaChat.Core.Models;

public record Product
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string PunchLine { get; init; } = string.Empty;
    public decimal Price { get; init; }
}
```

## Performance Targets

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Response Time (P50) | < 1s | SignalR hub method execution |
| Response Time (P95) | < 2s | SignalR hub method execution |
| Response Time (P99) | < 5s | SignalR hub method execution |
| Concurrent Users | 1000+ | Load testing with NBomber |
| Memory Usage | < 500MB | Production monitoring |
| CPU Usage | < 70% | Production monitoring |
| Error Rate | < 0.1% | Application Insights |
| Availability | > 99.9% | Uptime monitoring |

## Testing Strategy

### Unit Tests Example
```csharp
using Xunit;
using Moq;
using ZavaChat.Tools;

public class AISearchToolsTests
{
    [Fact]
    public async Task GetProductRecommendations_ValidQuery_ReturnsProducts()
    {
        // Arrange
        var mockSearchClient = new Mock<SearchClient>();
        var tools = new AISearchTools(mockSearchClient.Object, ...);
        
        // Act
        var products = await tools.GetProductRecommendationsAsync("paint roller");
        
        // Assert
        Assert.NotEmpty(products);
        Assert.All(products, p => Assert.False(string.IsNullOrEmpty(p.Name)));
    }
}
```

### Integration Tests Example
```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;

public class ChatHubIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task SendMessage_ValidRequest_ReceivesResponse()
    {
        // Arrange
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/ws")
            .Build();
            
        await connection.StartAsync();
        
        var receivedResponse = false;
        connection.On<ChatResponse>("ReceiveMessage", response =>
        {
            receivedResponse = true;
            Assert.NotNull(response.Answer);
        });
        
        // Act
        await connection.InvokeAsync("SendMessage", new ChatRequest
        {
            Message = "Hello"
        });
        
        await Task.Delay(5000); // Wait for response
        
        // Assert
        Assert.True(receivedResponse);
    }
}
```

## Deployment

### Dockerfile
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ZavaChat.Web/ZavaChat.Web.csproj", "src/ZavaChat.Web/"]
COPY ["src/ZavaChat.Agents/ZavaChat.Agents.csproj", "src/ZavaChat.Agents/"]
COPY ["src/ZavaChat.Tools/ZavaChat.Tools.csproj", "src/ZavaChat.Tools/"]
COPY ["src/ZavaChat.Services/ZavaChat.Services.csproj", "src/ZavaChat.Services/"]
COPY ["src/ZavaChat.Core/ZavaChat.Core.csproj", "src/ZavaChat.Core/"]

RUN dotnet restore "src/ZavaChat.Web/ZavaChat.Web.csproj"

COPY . .
WORKDIR "/src/src/ZavaChat.Web"
RUN dotnet build "ZavaChat.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ZavaChat.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "ZavaChat.Web.dll"]
```

---

**Document Version**: 1.0  
**Last Updated**: 2025-11-21  
**Status**: Ready for Implementation
