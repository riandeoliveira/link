namespace JobScraperBot.Constants;

public static class EnvironmentVariables
{
    private static string Get(string envVarName)
    {
        return Environment.GetEnvironmentVariable(envVarName) ?? "";
    }

    // General
    public static string ChatBotToken => Get("CHAT_BOT_TOKEN");
    public static string HeadlessMode => Get("HEADLESS_MODE");

    // Database
    public static string DatabaseEmail => Get("DATABASE_EMAIL");
    public static string DatabaseHost => Get("DATABASE_HOST");
    public static string DatabaseName => Get("DATABASE_NAME");
    public static string DatabasePassword => Get("DATABASE_PASSWORD");
    public static string DatabasePort => Get("DATABASE_PORT");
    public static string DatabaseUser => Get("DATABASE_USER");

    // Cron
    public static string CronChatId => Get("CRON_CHAT_ID");
    public static string CronIgnoreJobsFound => Get("CRON_IGNORE_JOBS_FOUND");
    public static string CronKeywords => Get("CRON_KEYWORDS");
    public static string CronLimit => Get("CRON_LIMIT");
    public static string CronPostedTime => Get("CRON_POSTED_TIME");
    public static string CronUserId => Get("CRON_USER_ID");
    public static string CronWorkType => Get("CRON_WORK_TYPE");
}
