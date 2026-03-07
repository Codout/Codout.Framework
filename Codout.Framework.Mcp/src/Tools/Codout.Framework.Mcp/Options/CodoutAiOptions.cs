using System.ComponentModel.DataAnnotations;

namespace Codout.Framework.Mcp.Options;

public sealed class CodoutAiOptions
{
    public const string SectionName = "CodoutAi";

    public string? DocsRoot { get; set; }
}
