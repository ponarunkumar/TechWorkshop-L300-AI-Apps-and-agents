namespace ZavaChat.Core.Configuration;

/// <summary>
/// Configuration for Azure AI services.
/// </summary>
public sealed record AzureAIConfig
{
    /// <summary>Configuration section name</summary>
    public const string SectionName = "AzureAI";
    
    /// <summary>Azure AI endpoint</summary>
    public required string Endpoint { get; init; }
    
    /// <summary>Azure AI API key</summary>
    public required string ApiKey { get; init; }
    
    /// <summary>Deployment name</summary>
    public required string DeploymentName { get; init; }
    
    /// <summary>Model name</summary>
    public string ModelName { get; init; } = "gpt-4";
    
    /// <summary>Maximum tokens for responses</summary>
    public int MaxTokens { get; init; } = 4096;
    
    /// <summary>Temperature setting</summary>
    public double Temperature { get; init; } = 0.7;
}
