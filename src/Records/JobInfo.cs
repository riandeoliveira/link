namespace LinkJoBot.Records;

public record JobInfo(
    string? Title,
    string? Company,
    string? Region,
    bool HasEasyApply,
    string? PostedTime
);
