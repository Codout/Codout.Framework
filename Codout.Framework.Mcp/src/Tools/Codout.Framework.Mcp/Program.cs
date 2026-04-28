using System.Reflection;
using Codout.Framework.Mcp.Options;
using Codout.Framework.Mcp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

if (await TryHandleCliCommandAsync(args))
{
    return;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Configuration.AddEnvironmentVariables(prefix: "CODOUT_MCP_");

builder.Services
    .AddOptions<CodoutAiOptions>()
    .Bind(builder.Configuration.GetSection(CodoutAiOptions.SectionName));

builder.Services.AddSingleton<IDocsRootResolver, DocsRootResolver>();

builder.Services.AddSingleton<FileSystemKnowledgeSource>(sp =>
{
    var resolver = sp.GetRequiredService<IDocsRootResolver>();
    return new FileSystemKnowledgeSource(resolver.Resolve() ?? string.Empty);
});

builder.Services.AddSingleton<EmbeddedKnowledgeSource>(_ =>
    new EmbeddedKnowledgeSource(typeof(Program).Assembly));

builder.Services.AddSingleton<IKnowledgeSource>(sp => new CompositeKnowledgeSource(new IKnowledgeSource[]
{
    sp.GetRequiredService<FileSystemKnowledgeSource>(),
    sp.GetRequiredService<EmbeddedKnowledgeSource>(),
}));

builder.Services.AddSingleton<IAiKnowledgeRepository, AiKnowledgeRepository>();
builder.Services.AddHostedService<StartupValidationService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

static async Task<bool> TryHandleCliCommandAsync(string[] args)
{
    if (args.Length == 0)
    {
        return false;
    }

    var command = args[0].Trim().ToLowerInvariant();

    switch (command)
    {
        case "--help":
        case "-h":
        case "help":
            PrintHelp();
            return true;

        case "--version":
        case "-v":
            Console.Out.WriteLine(GetVersion());
            return true;

        case "--list-tools":
            PrintTools();
            return true;

        case "--validate":
            await PrintValidationAsync();
            return true;

        default:
            return false;
    }
}

static void PrintHelp()
{
    Console.Out.WriteLine("codout-mcp - Codout Framework MCP server");
    Console.Out.WriteLine();
    Console.Out.WriteLine("Usage:");
    Console.Out.WriteLine("  codout-mcp                Run as MCP stdio server (default).");
    Console.Out.WriteLine("  codout-mcp --validate     Print knowledge pack status and exit.");
    Console.Out.WriteLine("  codout-mcp --list-tools   List MCP tools exposed by the server and exit.");
    Console.Out.WriteLine("  codout-mcp --version      Print version and exit.");
    Console.Out.WriteLine("  codout-mcp --help         Show this help.");
    Console.Out.WriteLine();
    Console.Out.WriteLine("Environment:");
    Console.Out.WriteLine("  CODOUT_MCP_CodoutAi__DocsRoot   Override path to docs/ai (filesystem source).");
}

static string GetVersion()
{
    var asm = typeof(Program).Assembly;
    var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    return string.IsNullOrWhiteSpace(info) ? asm.GetName().Version?.ToString() ?? "0.0.0" : info!;
}

static async Task PrintValidationAsync()
{
    var options = new CodoutAiOptions
    {
        DocsRoot = Environment.GetEnvironmentVariable("CODOUT_MCP_CodoutAi__DocsRoot")
    };
    var resolver = new DocsRootResolver(Microsoft.Extensions.Options.Options.Create(options));
    var fs = new FileSystemKnowledgeSource(resolver.Resolve() ?? string.Empty);
    var embedded = new EmbeddedKnowledgeSource(typeof(Program).Assembly);
    var composite = new CompositeKnowledgeSource(new IKnowledgeSource[] { fs, embedded });
    var repo = new AiKnowledgeRepository(composite);
    var status = await repo.GetStatusAsync();

    Console.Out.WriteLine($"Source       : {composite.Description}");
    Console.Out.WriteLine($"Resolved     : {status.DocsRootResolved}");
    Console.Out.WriteLine($"StaticDocs   : {status.StaticDocumentCount}");
    Console.Out.WriteLine($"GoldRefs     : {status.GoldReferenceCount}");
    Console.Out.WriteLine($"Filesystem   : {fs.Description} (resolved={fs.IsResolved})");
    Console.Out.WriteLine($"Embedded     : {embedded.Description} (resolved={embedded.IsResolved})");
}

static void PrintTools()
{
    var assembly = typeof(Program).Assembly;
    var toolType = assembly.GetType("Codout.Framework.Mcp.Tools.CodoutAiTools");
    if (toolType is null)
    {
        Console.Error.WriteLine("Tool type not found.");
        return;
    }

    foreach (var method in toolType.GetMethods(BindingFlags.Public | BindingFlags.Static))
    {
        var toolAttr = method.GetCustomAttributes()
            .FirstOrDefault(a => a.GetType().Name == "McpServerToolAttribute");
        if (toolAttr is null)
        {
            continue;
        }

        var nameProperty = toolAttr.GetType().GetProperty("Name");
        var name = nameProperty?.GetValue(toolAttr) as string;
        var resolvedName = string.IsNullOrWhiteSpace(name)
            ? ToSnakeCase(method.Name)
            : name!;

        var description = method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description ?? string.Empty;
        Console.Out.WriteLine($"{resolvedName}\t{description}");
    }
}

static string ToSnakeCase(string value)
{
    if (string.IsNullOrEmpty(value))
    {
        return value;
    }

    var sb = new System.Text.StringBuilder(value.Length + 4);
    for (var i = 0; i < value.Length; i++)
    {
        var c = value[i];
        if (i > 0 && char.IsUpper(c))
        {
            sb.Append('_');
        }
        sb.Append(char.ToLowerInvariant(c));
    }
    return sb.ToString();
}
