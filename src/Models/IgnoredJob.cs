using System.ComponentModel.DataAnnotations.Schema;

namespace JobScraperBot.Models;

[Table("ignored_jobs")]
public sealed class IgnoredJob : BaseModel
{
    [Column("user_id")]
    public required Guid UserId { get; set; }

    [Column("job_id")]
    public required string JobId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
