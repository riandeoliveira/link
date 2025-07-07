namespace JobScraperBot.Records;

public record JobsPageUrlParams(
    string? WorkType,
    string? PostedTime,
    string Keywords
);
