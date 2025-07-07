namespace JobScraperBot.Records;

public record Job(
    string? Title,
    string? Company,
    string? Region,
    bool EasyApply,
    string? PostedTime
);
