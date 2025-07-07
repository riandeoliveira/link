namespace JobScraperBot.Records;

public record JobFoundMessageData(
    string Title,
    string Company,
    string Region,
    string EasyApply,
    string PostedTime,
    string JobIndex,
    string TotalJobs,
    string Link
);
