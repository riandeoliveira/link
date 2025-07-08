using LinkJoBot.Enums;

namespace LinkJoBot.Utils;

public static class JobUtils
{
    public static string GetPostedTimeLabel(int? postedTime)
    {
        return postedTime switch
        {
            3600 => "1 hora",
            14400 => "4 horas",
            28800 => "8 horas",
            43200 => "12 horas",
            86400 => "24 horas",
            null => "24h+",
            _ => "24h+",
        };
    }

    public static string GetWorkTypeLabel(WorkType workType)
    {
        return workType switch
        {
            WorkType.All => "Todos",
            WorkType.OnSite => "Presencial",
            WorkType.Remote => "Remoto",
            WorkType.Hybrid => "Híbrido",
            _ => "Todos",
        };
    }
}
