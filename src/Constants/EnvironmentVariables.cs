using System.Globalization;

namespace LinkJoBot.Constants;

public static class EnvironmentVariables
{
    private static T Get<T>(string key, T fallback)
    {
        string? value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        try
        {
            Type targetType = typeof(T);

            if (targetType.IsEnum)
            {
                var parsed = Enum.TryParse(
                    targetType,
                    value,
                    ignoreCase: true,
                    out object? enumResult
                );

                return parsed ? (T)enumResult! : fallback;
            }

            if (targetType == typeof(Guid))
            {
                var parsed = Guid.TryParse(value, out Guid guidResult);

                return parsed ? (T)(object)guidResult : fallback;
            }

            if (targetType == typeof(bool))
            {
                var parsed = bool.TryParse(value, out bool boolResult);

                return parsed ? (T)(object)boolResult : fallback;
            }

            if (targetType == typeof(int))
            {
                var parsed = int.TryParse(
                    value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out int intResult
                );

                return parsed ? (T)(object)intResult : fallback;
            }

            return (T)(object)value;
        }
        catch
        {
            return fallback;
        }
    }

    // General
    public static string ChatBotToken => Get("CHAT_BOT_TOKEN", "");
    public static bool HeadlessMode => Get("HEADLESS_MODE", false);

    // Database
    public static string DatabaseEmail => Get("DATABASE_EMAIL", "");
    public static string DatabaseHost => Get("DATABASE_HOST", "");
    public static string DatabaseName => Get("DATABASE_NAME", "");
    public static string DatabasePassword => Get("DATABASE_PASSWORD", "");
    public static int DatabasePort => Get("DATABASE_PORT", 0);
    public static string DatabaseUser => Get("DATABASE_USER", "");

    // Cron
    public static string CronChatId => Get("CRON_CHAT_ID", "");
    public static bool CronIgnoreJobsFound => Get("CRON_IGNORE_JOBS_FOUND", false);
    public static string CronKeywords => Get("CRON_KEYWORDS", "");
    public static int CronLimit => Get("CRON_LIMIT", 5);
    public static int? CronPostedTime => Get("CRON_POSTED_TIME", (int?)null);
    public static Guid CronUserId => Get("CRON_USER_ID", Guid.Empty);
    public static Enums.WorkType CronWorkType => Get("CRON_WORK_TYPE", Enums.WorkType.All);
}
