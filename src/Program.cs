using GhostIntegration;

using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<ImportPagesCommand>("import-pages")
        .WithDescription("Imports pages from a JSON file");

    config.AddCommand<ImportMembersCommand>("import-members")
        .WithDescription("Imports members from a JSON file");
});

return app.Run(args);