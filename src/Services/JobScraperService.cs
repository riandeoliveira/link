using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

using JobScraperBot.Constants;
using JobScraperBot.Interfaces;
using JobScraperBot.Models;
using JobScraperBot.Records;

using Microsoft.Playwright;

namespace JobScraperBot.Services;

public class JobScraperService(
    IChatBotNotifierService chatBot,
    IIgnoredJobRepository ignoredJobRepository,
    IUnitOfWork unitOfWork
) : IJobScraperService
{
    private readonly ConcurrentDictionary<Guid, IList<IgnoredJob>> _jobCache = new();
    private readonly IChatBotNotifierService _chatBot = chatBot;
    private readonly IIgnoredJobRepository _ignoredJobRepository = ignoredJobRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private IPage? _page;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IPlaywright playwright = await Playwright.CreateAsync();

        BrowserTypeLaunchOptions options = new()
        {
            Headless = bool.Parse(EnvironmentVariables.HeadlessMode)
        };

        IBrowser browser = await playwright.Chromium.LaunchAsync(options);

        _page = await browser.NewPageAsync();
    }

    public async Task ForEachJobAsync(int limit, Func<Job, int, Task> processJob)
    {
        if (_page is null) throw new InvalidOperationException("Page not found");

        IReadOnlyList<IElementHandle> jobCardFooterHandles = await _page.QuerySelectorAllAsync(Selectors.JobCardFooter);
        IReadOnlyList<IElementHandle> jobLinkHandles = await _page.QuerySelectorAllAsync(Selectors.JobLink);
        IElementHandle? jobCompanyHandle = await _page.QuerySelectorAsync(Selectors.JobCompany);
        IElementHandle? jobInfoHandle = await _page.QuerySelectorAsync(Selectors.JobInfo);
        IElementHandle? jobTitleHandle = await _page.QuerySelectorAsync(Selectors.JobTitle);

        for (int index = 0; index < limit; index++)
        {
            await jobLinkHandles[index].ClickAsync();

            string jobCardFooterText = await jobCardFooterHandles[index].EvaluateAsync<string>("el => el.innerText.trim()");

            bool easyApply = jobCardFooterText.Contains("Candidatura simplificada");

            string? title = jobTitleHandle is not null
                ? await jobTitleHandle.EvaluateAsync<string>("el => el.innerText.trim()")
                : null;

            string? company = jobCompanyHandle is not null
                ? await jobCompanyHandle.EvaluateAsync<string>("el => el.innerText.trim()")
                : null;

            string? jobInfo = jobInfoHandle is not null
                ? await jobInfoHandle.EvaluateAsync<string>("el => el.innerText.trim()")
                : null;

            string? region = null;
            string? postedTime = null;

            if (!string.IsNullOrWhiteSpace(jobInfo))
            {
                string[] infoParts = jobInfo.Split('·', StringSplitOptions.TrimEntries);

                region = infoParts.ElementAtOrDefault(0);
                postedTime = infoParts.ElementAtOrDefault(1);
            }

            Job job = new(
                title,
                company,
                region,
                easyApply,
                postedTime
            );

            await processJob(job, index);
        }
    }

    public string GetJobId()
    {
        if (_page is null) throw new InvalidOperationException("Page not initialized");

        Uri uri = new(_page.Url);

        string? jobId = HttpUtility.ParseQueryString(uri.Query).Get("currentJobId");

        return jobId is null ? throw new InvalidOperationException("Job ID not found") : jobId;
    }

    public static string GetJobsPageUrl(JobsPageUrlParams urlParams)
    {
        string baseUrl = "https://www.linkedin.com/jobs/search";

        NameValueCollection searchParams = HttpUtility.ParseQueryString(string.Empty);

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
        if (_page is null) throw new InvalidOperationException("Page not found");

        string cookiesJson = await File.ReadAllTextAsync("../temp/cookies.json");

        IEnumerable<Cookie>? cookies = JsonSerializer.Deserialize<IEnumerable<Cookie>>(cookiesJson, JsonOptions);

        if (cookies is not null)
        {
            await _page.Context.AddCookiesAsync(cookies);
        }
    }

    public async Task RunJobSearchAsync(User user, CancellationToken cancellationToken)
    {
        if (_page is null) throw new InvalidOperationException("Page not found");

        await _chatBot.SendMultilineMessageAsync(
            user.ChatId,
            "🚨 BUSCA DE VAGAS INICIADA 🚨",
            "",
            "Por favor, aguarde..."
        );

        JobsPageUrlParams urlParams = new(
            PostedTime: user.PostedTime.HasValue ? $@"r{user.PostedTime}" : null,
            WorkType: user.WorkType == 0 ? null : ((int) user.WorkType).ToString(),
            Keywords: user.Keywords
        );

        string jobsPageUrl = GetJobsPageUrl(urlParams);

        await LoadCookiesAsync();
        await _page.GotoAsync(jobsPageUrl);

        await _page.WaitForSelectorAsync(".scaffold-layout__list-header + div", new PageWaitForSelectorOptions
        {
            Timeout = 10000
        });

        await _page.EvalOnSelectorAsync(
            ".scaffold-layout__list-header + div",
            @"el => { el.style.zoom = '10%'; }"
        );

        IReadOnlyList<IElementHandle> jobCardElements = await _page.QuerySelectorAllAsync(".scaffold-layout__list-header+div>ul>li");

        int totalJobsFound = jobCardElements.Count;

        if (totalJobsFound < user.Limit)
        {
            await _chatBot.SendErrorMessageAsync(
                user.ChatId,
                "Limite de vagas excedido!",
                "",
                $@"Total esperado: {user.Limit}",
                $@"Total encontrado: {totalJobsFound}",
                "",
                "Buscando somente o que foi encontrado. Aguarde..."
            );
        }

        await Task.Delay(5000, cancellationToken);

        int availableLimit = Math.Min(totalJobsFound, user.Limit);

        await _chatBot.SendMultilineMessageAsync(
            user.ChatId,
            $@"<b>ℹ️ {availableLimit} Vagas identificadas! ℹ️</b>",
            "",
            "Iniciando análise..."
        );

        IList<IgnoredJob> sessionJobs = _jobCache.GetOrAdd(user.Id, _ => []);

        await ForEachJobAsync(availableLimit, async (job, index) =>
        {
            string jobId = GetJobId();
            string steps = $@"{index + 1}/{availableLimit}";

            IgnoredJob ignoredJob = new()
            {
                UserId = user.Id,
                JobId = jobId,
            };

            if (user.IgnoreJobsFound)
            {
                IgnoredJob? alreadyIgnoredJob = await _ignoredJobRepository.FindOneAsync(
                    x => x.UserId == user.Id &&
                    x.JobId == jobId,
                    cancellationToken
                );

                if (alreadyIgnoredJob is not null)
                {
                    await _chatBot.SendMultilineMessageAsync(
                        user.ChatId,
                        $@"<b>⚠️ Vaga já encontrada – {steps} ⚠️</b>",
                        "",
                        "Esta vaga já foi localizada anteriormente. Pulando para a próxima..."
                    );

                    return;
                }

                await _ignoredJobRepository.CreateAsync(ignoredJob, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
            }

            sessionJobs.Add(ignoredJob);

            string jobLink = $@"https://www.linkedin.com/jobs/view/{jobId}";

            await _chatBot.SendJobFoundMessageAsync(
                user.ChatId,
                new JobFoundMessageData(
                    Title: job.Title ?? "Indefinido",
                    Company: job.Company ?? "Indefinido",
                    Region: job.Region ?? "Indefinido",
                    EasyApply: job.EasyApply ? "Sim" : "Não",
                    PostedTime: job.PostedTime ?? "Indefinido",
                    JobIndex: (index + 1).ToString(),
                    TotalJobs: availableLimit.ToString(),
                    Link: jobLink
                )
            );
        });

        if (sessionJobs.Count == 0)
        {
            await _chatBot.SendMultilineMessageAsync(
                user.ChatId,
                "<b>⚠️ NENHUMA VAGA ENCONTRADA ⚠️</b>",
                "",
                "Nenhuma nova vaga encontrada com os parâmetros definidos. Por favor, tente novamente mais tarde :("
            );
        }
        else
        {
            await _chatBot.SendMultilineMessageAsync(
                user.ChatId,
                "<b>✅ BUSCA DE VAGAS FINALIZADA ✅</b>",
                "",
                "Até a próxima :)"
            );
        }

        sessionJobs.Clear();
    }
}
