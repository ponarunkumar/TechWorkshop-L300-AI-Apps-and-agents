namespace ZavaChat.Core.Models;

using ZavaChat.Core.Enums;

/// <summary>
/// Represents a message in the chat conversation.
/// </summary>
public sealed record ChatMessage
{
    /// <summary>Unique message identifier</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();
    
    /// <summary>Role of the message sender</summary>
    public required MessageRole Role { get; init; }
    
    /// <summary>Message content</summary>
    public required string Content { get; init; }
    
    /// <summary>Agent type that generated this message</summary>
    public AgentType? AgentType { get; init; }
    
    /// <summary>Timestamp when message was created</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    /// <summary>Indicates if this is an error message</summary>
    public bool IsError { get; init; }
    
    /// <summary>Media URL if message contains media</summary>
    public string? MediaUrl { get; init; }
    
    /// <summary>Type of media if present</summary>
    public MediaType MediaType { get; init; } = MediaType.Text;
    
    /// <summary>Additional metadata</summary>
    public Dictionary<string, object>? Metadata { get; init; }
}
