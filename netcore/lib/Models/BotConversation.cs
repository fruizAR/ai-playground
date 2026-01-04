using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapeAI.Models;

[Table("bot_conversation")]
public class BotConversation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string MessageId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AssistantId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ThreadId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? RunId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(100)]
    public string Role { get; set; } = "user";

    [Required]
    public string Message { get; set; } = string.Empty;
}
