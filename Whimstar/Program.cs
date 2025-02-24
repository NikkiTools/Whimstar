// See https://aka.ms/new-console-template for more information

using Spectre.Console.Cli;
using Whimstar.Commands;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<ExtractCommand>("extract").WithDescription("Extracts data for a given version.");
    config.AddCommand<ListVersionsCommand>("list-versions").WithDescription("List all versions available in a given folder.");
});

await app.RunAsync(args);