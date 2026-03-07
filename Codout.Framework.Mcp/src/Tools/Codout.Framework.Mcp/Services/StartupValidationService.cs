using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Codout.Framework.Mcp.Services;

public sealed class StartupValidationService : IHostedService
{
    private readonly IAiKnowledgeRepository _repository;
    private readonly ILogger<StartupValidationService> _logger;

    public StartupValidationService(
        IAiKnowledgeRepository repository,
        ILogger<StartupValidationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var status = await _repository.GetStatusAsync(cancellationToken);

        if (!status.DocsRootResolved)
        {
            _logger.LogWarning("Codout AI docs root was not resolved. Expected path: {DocsRoot}", status.DocsRoot);
            return;
        }

        _logger.LogInformation(
            "Codout MCP ready. DocsRoot={DocsRoot}; StaticDocs={StaticDocumentCount}; GoldReferences={GoldReferenceCount}",
            status.DocsRoot,
            status.StaticDocumentCount,
            status.GoldReferenceCount);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
