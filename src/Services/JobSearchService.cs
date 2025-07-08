using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using LinkJoBot.Constants;
using LinkJoBot.Entities;
using LinkJoBot.Interfaces;
using LinkJoBot.Records;
using Microsoft.Playwright;

namespace LinkJoBot.Services;

public class JobSearchService(
    IChatBotNotifierService chatBot,
    IIgnoredJobRepository ignoredJobRepository,
    IUnitOfWork unitOfWork
) : IJobSearchService
{
    private const int PageTimeout = 10000;

    private readonly IChatBotNotifierService _chatBot = chatBot;
    private readonly IIgnoredJobRepository _ignoredJobRepository = ignoredJobRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private static readonly ConcurrentDictionary<Guid, IList<IgnoredJob>> _jobCache = new();

    private IPage? _page;

    public async Task RunJobSearchAsync(User user, CancellationToken cancellationToken)
    {
        if (_page is null)
        {
            throw new InvalidOperationException("Page not initialized");
        }

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            """
            🚨 BUSCA DE VAGAS INICIADA 🚨

            Por favor, aguarde...
            """
        );

        await AccessJobsPage(user);

        var availableLimit = await EnsureJobsAvailableLimit(user);

        await _chatBot.SendTextMessageAsync(
            user.ChatId,
            $"""
            <b>ℹ️ {availableLimit} Vagas identificadas! ℹ️</b>

            Iniciando análise...
            """
        );

        var sessionJobs = _jobCache.GetOrAdd(user.Id, _ => []);

        await ForEachJobAsync(
            availableLimit,
            async (job, index) =>
            {
                var jobId = GetJobId();
                var steps = $"{index + 1}/{availableLimit}";

                var ignoredJob = new IgnoredJob { UserId = user.Id, JobId = jobId };

                if (user.IgnoreJobsFound)
                {
                    var alreadyIgnoredJob = await _ignoredJobRepository.FindOneAsync(
                        x => x.UserId == user.Id && x.JobId == jobId,
                        cancellationToken
                    );

                    if (alreadyIgnoredJob is not null)
                    {
                        await _chatBot.SendTextMessageAsync(
                            user.ChatId,
                            """
                            <b>⚠️ Vaga já encontrada – {steps} ⚠️</b>

                            Esta vaga já foi localizada anteriormente. Pulando para a próxima...
                            """
                        );

                        return;
                    }

                    await _ignoredJobRepository.CreateAsync(ignoredJob, cancellationToken);
                    await _unitOfWork.CommitAsync(cancellationToken);
                }

                sessionJobs.Add(ignoredJob);

                var jobLink = $"https://www.linkedin.com/jobs/view/{jobId}";

                await _chatBot.SendJobFoundMessageAsync(
                    user.ChatId,
                    new JobFoundMessageData(
                        Title: job.Title ?? "Indefinido",
                        Company: job.Company ?? "Indefinido",
                        Region: job.Region ?? "Indefinido",
                        HasEasyApply: job.HasEasyApply ? "Sim" : "Não",
                        PostedTime: job.PostedTime ?? "Indefinido",
                        JobIndex: (index + 1).ToString(),
                        TotalJobs: availableLimit.ToString(),
                        Link: jobLink
                    )
                );
            }
        );

        if (sessionJobs.Count == 0)
        {
            await _chatBot.SendTextMessageAsync(
                user.ChatId,
                """
                <b>⚠️ NENHUMA VAGA ENCONTRADA ⚠️</b>

                Nenhuma nova vaga encontrada com os parâmetros definidos. Por favor, tente novamente mais tarde :(
                """
            );
        }
        else
        {
            await _chatBot.SendTextMessageAsync(
                user.ChatId,
                """
                <b>✅ BUSCA DE VAGAS FINALIZADA ✅</b>

                Até a próxima :)
                """
            );
        }

        sessionJobs.Clear();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var playwright = await Playwright.CreateAsync();
        var options = new BrowserTypeLaunchOptions { Headless = EnvironmentVariables.HeadlessMode };
        var browser = await playwright.Chromium.LaunchAsync(options);

        _page = await browser.NewPageAsync();
    }

    private async Task AccessJobsPage(User user)
    {
        var urlParams = new JobsPageUrlParams(
            PostedTime: user.PostedTime.HasValue ? $"r{user.PostedTime}" : null,
            WorkType: user.WorkType == 0 ? null : ((int)user.WorkType).ToString(),
            Keywords: user.Keywords
        );

        var jobsPageUrl = GetJobsPageUrl(urlParams);

        await LoadCookiesAsync();
        await _page!.GotoAsync(jobsPageUrl);
    }

    private async Task<int> EnsureJobsAvailableLimit(User user)
    {
        await _page!.WaitForSelectorAsync(
            PageSelectors.JobContainer,
            new PageWaitForSelectorOptions { Timeout = PageTimeout }
        );

        await _page.EvalOnSelectorAsync(
            PageSelectors.JobContainer,
            @"el => { el.style.zoom = '10%'; }"
        );

        var jobCardElements = await _page.QuerySelectorAllAsync(PageSelectors.JobCard);
        var totalJobsFound = jobCardElements.Count;

        if (totalJobsFound < user.Limit)
        {
            await _chatBot.SendErrorMessageAsync(
                user.ChatId,
                $"""
                Limite de vagas excedido!

                Total esperado: {user.Limit}
                Total encontrado: {totalJobsFound}

                Buscando somente o que foi encontrado. Aguarde...
                """
            );
        }

        await Task.Delay(PageTimeout);

        var availableLimit = Math.Min(totalJobsFound, user.Limit);

        return availableLimit;
    }

    private async Task ForEachJobAsync(int limit, Func<JobInfo, int, Task> processJob)
    {
        if (_page is null)
        {
            throw new InvalidOperationException("Page not initialized");
        }

        var jobCardFooterHandles = await _page.QuerySelectorAllAsync(PageSelectors.JobCardFooter);
        var jobLinkHandles = await _page.QuerySelectorAllAsync(PageSelectors.JobLink);
        var jobCompanyHandle = await _page.QuerySelectorAsync(PageSelectors.JobCompany);
        var jobInfoHandle = await _page.QuerySelectorAsync(PageSelectors.JobInfo);
        var jobTitleHandle = await _page.QuerySelectorAsync(PageSelectors.JobTitle);

        for (int index = 0; index < limit; index++)
        {
            await jobLinkHandles[index].ClickAsync();

            var jobCardFooterText = await jobCardFooterHandles[index]
                .EvaluateAsync<string>("el => el.innerText.trim()");

            var hasEasyApply = jobCardFooterText.Contains("Candidatura simplificada");

            var title = jobTitleHandle is not null
                ? await jobTitleHandle.EvaluateAsync<string>("el => el.innerText.trim()")
                : null;

            var company = jobCompanyHandle is not null
                ? await jobCompanyHandle.EvaluateAsync<string>("el => el.innerText.trim()")
                : null;

            var jobInfo = jobInfoHandle is not null
                ? await jobInfoHandle.EvaluateAsync<string>("el => el.innerText.trim()")
                : null;

            string? region = null;
            string? postedTime = null;

            if (!string.IsNullOrWhiteSpace(jobInfo))
            {
                var infoParts = jobInfo.Split('·', StringSplitOptions.TrimEntries);

                region = infoParts.ElementAtOrDefault(0);
                postedTime = infoParts.ElementAtOrDefault(1);
            }

            var job = new JobInfo(title, company, region, hasEasyApply, postedTime);

            await processJob(job, index);
        }
    }

    private string GetJobId()
    {
        if (_page is null)
        {
            throw new InvalidOperationException("Page not initialized");
        }

        var uri = new Uri(_page.Url);
        var jobId = HttpUtility.ParseQueryString(uri.Query).Get("currentJobId");

        if (jobId is null)
        {
            throw new InvalidOperationException("Job ID not found");
        }

        return jobId;
    }

    public static string GetJobsPageUrl(JobsPageUrlParams urlParams)
    {
        var baseUrl = "https://www.linkedin.com/jobs/search";
        var searchParams = HttpUtility.ParseQueryString(string.Empty);

        if (!string.IsNullOrWhiteSpace(urlParams.PostedTime))
        {
            searchParams["f_TPR"] = urlParams.PostedTime;
        }

        if (!string.IsNullOrWhiteSpace(urlParams.WorkType))
        {
            searchParams["f_WT"] = urlParams.WorkType;
        }

        if (!string.IsNullOrWhiteSpace(urlParams.Keywords))
        {
            searchParams["keywords"] = urlParams.Keywords.Trim();
        }

        return $"{baseUrl}?{searchParams}";
    }

    public async Task LoadCookiesAsync()
    {
        if (_page is null)
        {
            throw new InvalidOperationException("Page not initialized");
        }

        var cookiesJson = await File.ReadAllTextAsync("../temp/cookies.json");
        var cookies = JsonSerializer.Deserialize<IEnumerable<Cookie>>(cookiesJson, JsonOptions);

        if (cookies is not null)
        {
            await _page.Context.AddCookiesAsync(cookies);
        }
    }
}
