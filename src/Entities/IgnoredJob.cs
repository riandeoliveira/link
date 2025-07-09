using System.ComponentModel.DataAnnotations.Schema;

namespace LinkJobber.Entities;

[Table("ignored_jobs")]
public class IgnoredJob : BaseEntity
{
    [Column("user_id")]
    public required Guid UserId { get; set; }

    [Column("job_id")]
    public required string JobId { get; set; }

    public User User { get; set; } = null!;
}
