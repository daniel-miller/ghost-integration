using System.Diagnostics.CodeAnalysis;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Spectre.Console;
using Spectre.Console.Cli;

namespace GhostIntegration;

internal sealed partial class ImportPagesCommand : AsyncCommand<ImportPagesCommandSettings>
{
    private GhostApiHelper _ghost = null!;

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] ImportPagesCommandSettings settings)
    {
        if (!ValidateOptions(settings, out var importFile))
            return -1;

        AnsiConsole.Markup($"[deepskyblue1]Reading import file {importFile} ... [/]");

        var importJson = File.Exists(importFile) ? File.ReadAllText(importFile) : null;

        var importList = string.IsNullOrEmpty(importJson) ? null : JsonConvert.DeserializeObject<ExportedPage[]>(importJson);

        if (importList == null || importList.Length == 0)
        {
            AnsiConsole.Markup("[red] the import file is empty[/]");
            return -1;
        }

        AnsiConsole.Markup($"[lime] found {importList.Length} pages[/]");

        AnsiConsole.WriteLine();

        AnsiConsole.Markup($"[deepskyblue1]Getting existing pages from {settings.Url} ... [/]");

        var existingPages = await GetExistingPages();

        AnsiConsole.Markup($"[lime]found {existingPages.Length} pages[/]");

        AnsiConsole.WriteLine();

        var existingSlugs = existingPages.Select(x => x.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var n = importList.Length;

                var task = ctx.AddTask($"[deepskyblue1]Importing pages[/]", true, n);

                var count = 0;

                for (var i = 0; i < n; i++)
                {
                    var inputItem = importList[i];

                    if (!existingSlugs.Contains(inputItem.Name))
                    {
                        if (!await ImportPage(inputItem))
                        {
                            task.StopTask();
                            break;
                        }
                    }

                    count++;
                    task.Increment(1);
                }
            });

        return 0;
    }

    private async Task<GhostPageItem[]> GetExistingPages()
    {
        var page = 1;

        var result = new List<GhostPageItem>();

        while (true)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_ghost.Url}/ghost/api/admin/pages/?page={page++}"))
            {
                requestMessage.Headers.Add("Authorization", "Ghost " + _ghost.Token);

                requestMessage.Headers.Add("Accept-Version", "v5.0");

                var response = await _ghost.Client.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    await WriteHttpError(response);
                    return [];
                }

                var json = await response.Content.ReadAsStringAsync();

                var jObj = JObject.Parse(json);

                var pages = jObj["pages"]!.ToObject<GhostPageItem[]?>();

                if (pages!.Length == 0)
                    break;

                result.AddRange(pages);
            }
        }

        return result.ToArray();
    }

    private async Task<bool> ImportPage(ExportedPage exportedPage)
    {
        var item = new GhostPageItem
        {
            Slug = exportedPage.Path,
            Title = string.IsNullOrEmpty(exportedPage.Title) ? exportedPage.Name : exportedPage.Title,
            Excerpt = exportedPage.Summary,
            Html = exportedPage.Content,
            Status = "published",
            Visibility = "public",
            CreatedAt = exportedPage.Date.UtcDateTime,
            UpdatedAt = exportedPage.Date.UtcDateTime,
            PublishedAt = exportedPage.Date.UtcDateTime,
        };

        var ghostJson = JsonConvert.SerializeObject(new { pages = new[] { item } }, Formatting.None);

        using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_ghost.Url}/ghost/api/admin/pages/?source=html"))
        {
            requestMessage.Headers.Add("Authorization", "Ghost " + _ghost.Token);

            requestMessage.Headers.Add("Accept-Version", "v5.0");

            requestMessage.Content = new StringContent(ghostJson, Encoding.UTF8, "application/json");

            var response = await _ghost.Client.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                await WriteHttpError(response);
                return false;
            }
        }

        return true;
    }

    private async Task WriteHttpError(HttpResponseMessage response)
    {
        AnsiConsole.WriteLine("An unexpected HTTP error occurred.");

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            var result = JsonConvert.DeserializeObject(content);

            content = JsonConvert.SerializeObject(result, Formatting.Indented);
        }
        catch
        {

        }

        AnsiConsole.WriteLine($"HTTP {response.StatusCode}: {content}");
    }

    private bool ValidateOptions(ImportPagesCommandSettings args, out string? input)
    {
        var url = args.Url;

        var key = args.Key;

        input = args.Input;

        if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out var _))
        {
            AnsiConsole.WriteLine("'url' is a required parameter. It holds the address for your Ghost server (https://example.ghost.io/)");
            return false;
        }

        if (string.IsNullOrEmpty(key))
        {
            AnsiConsole.WriteLine("'key' is required parameter. It holds the Staff Access Token for your Ghost profile.");
            return false;
        }

        if (string.IsNullOrEmpty(input))
        {
            AnsiConsole.WriteLine("'input' is required parameter. It holds the physical path to the JSON file that contains the pages you want to import.");
            return false;
        }

        if (url!.EndsWith("/"))
            url = url.Substring(0, url.Length - 1);

        _ghost = new GhostApiHelper(new HttpClient(), url, GhostJwtHelper.GetToken(key!)!);

        return _ghost.Token != null;
    }
}