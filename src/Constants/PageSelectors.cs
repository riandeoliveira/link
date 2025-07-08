namespace LinkJoBot.Constants;

public static class PageSelectors
{
    public const string JobCard = ".scaffold-layout__list-header+div>ul>li";

    public const string JobCardFooter = ".job-card-list__footer-wrapper";

    public const string JobCompany = ".job-details-jobs-unified-top-card__company-name";

    public const string JobContainer = ".scaffold-layout__list-header+div";

    public const string JobInfo =
        ".job-details-jobs-unified-top-card__tertiary-description-container > span";

    public const string JobLink =
        ".scaffold-layout__list-header+div>ul>li>div>div>div>div>div+div>div>a";

    public const string JobTitle = ".job-details-jobs-unified-top-card__job-title";
}
