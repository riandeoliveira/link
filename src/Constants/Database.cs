namespace LinkJoBot.Constants;

public static class Database
{
    public static string ConnectionString =>
        $@"
            Server={EnvironmentVariables.DatabaseHost};
            Port={EnvironmentVariables.DatabasePort};
            Database={EnvironmentVariables.DatabaseName};
            Username={EnvironmentVariables.DatabaseUser};
            Password={EnvironmentVariables.DatabasePassword}
        ";
}
