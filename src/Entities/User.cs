using System.ComponentModel.DataAnnotations.Schema;
using LinkJoBot.Enums;

namespace LinkJoBot.Entities;

[Table("users")]
public sealed class User : BaseEntity
{
    [Column("chat_id")]
    public required string ChatId { get; set; }

    [Column("work_type")]
    public WorkType WorkType { get; set; } = WorkType.All;

    [Column("limit")]
    public int Limit { get; set; } = 5;

    [Column("posted_time")]
    public int? PostedTime { get; set; }

    [Column("keywords")]
    public string Keywords { get; set; } = "";

    [Column("is_awaiting_for_keywords")]
    public bool IsAwaitingForKeywords { get; set; } = false;

    [Column("ignore_jobs_found")]
    public bool IgnoreJobsFound { get; set; } = false;

    [InverseProperty("User")]
    public ICollection<IgnoredJob> IgnoredJobs { get; set; } = [];
}
