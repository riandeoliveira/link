namespace LinkJoBot.Records;

public record JobFoundMessageData(
    string Title,
    string Company,
    string Region,
    string HasEasyApply,
    string PostedTime,
    string JobIndex,
    string TotalJobs,
    string Link
);
