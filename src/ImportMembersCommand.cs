using System.Diagnostics.CodeAnalysis;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Spectre.Console;
using Spectre.Console.Cli;

namespace GhostIntegration;

internal sealed partial class ImportMembersCommand : AsyncCommand<ImportMembersCommandSettings>
{
    private GhostApiHelper _ghost = null!;

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] ImportMembersCommandSettings settings)
    {
        if (!ValidateOptions(settings, out var importFile))
            return -1;

        AnsiConsole.Markup($"[deepskyblue1]Reading import file {importFile} ... [/]");

        var importJson = File.Exists(importFile) ? File.ReadAllText(importFile) : null;

        var importList = string.IsNullOrEmpty(importJson) ? null : JsonConvert.DeserializeObject<ExportedMember[]>(importJson);

        if (importList == null || importList.Length == 0)
        {
            AnsiConsole.Markup("[red] the import file is empty[/]");
            return -1;
        }

        AnsiConsole.Markup($"[lime] found {importList.Length} members[/]");

        AnsiConsole.WriteLine();

        AnsiConsole.Markup($"[deepskyblue1]Getting existing members from {settings.Url} ... [/]");

        var existingMembers = await GetExistingMembers();

        AnsiConsole.Markup($"[lime]found {existingMembers.Length} members[/]");

        AnsiConsole.WriteLine();

        var existingEmails = existingMembers.Select(x => x.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var n = importList.Length;

                var task = ctx.AddTask($"[deepskyblue1]Importing members[/]", true, n);

                var count = 0;

                for (var i = 0; i < n; i++)
                {
                    var inputItem = importList[i];

                    if (!existingEmails.Contains(inputItem.Name))
                    {
                        if (!await ImportMember(inputItem))
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

    private async Task<GhostMemberItem[]> GetExistingMembers()
    {
        var page = 1;

        var result = new List<GhostMemberItem>();

        while (true)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_ghost.Url}/ghost/api/admin/members/?page={page++}"))
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

                var members = jObj["members"]!.ToObject<GhostMemberItem[]?>();

                if (members!.Length == 0)
                    break;

                result.AddRange(members);
            }
        }

        return result.ToArray();
    }

    private async Task<bool> ImportMember(ExportedMember exportedMember)
    {
        var item = new GhostMemberItem
        {
            Name = exportedMember.Name,
            Email = exportedMember.Email,
            CreatedAt = exportedMember.CreatedAt
        };

        var ghostJson = JsonConvert.SerializeObject(new { members = new[] { item } }, Formatting.None);

        using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_ghost.Url}/ghost/api/admin/members/"))
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

    private bool ValidateOptions(ImportMembersCommandSettings args, out string? input)
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
            AnsiConsole.WriteLine("'input' is required parameter. It holds the physical path to the JSON file that contains the members you want to import.");
            return false;
        }

        if (url!.EndsWith("/"))
            url = url.Substring(0, url.Length - 1);

        _ghost = new GhostApiHelper(new HttpClient(), url, GhostJwtHelper.GetToken(key!)!);

        return _ghost.Token != null;
    }
}