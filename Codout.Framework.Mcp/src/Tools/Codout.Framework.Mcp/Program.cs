using Codout.Framework.Mcp.Options;
using Codout.Framework.Mcp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
builder.Services.AddSingleton<IAiKnowledgeRepository, FileSystemAiKnowledgeRepository>();
builder.Services.AddHostedService<StartupValidationService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
