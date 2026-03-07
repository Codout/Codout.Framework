namespace Codout.Framework.Mcp.Models;

public sealed record GoldReferenceMatch(
    string Name,
    string Module,
    string Pattern,
    string Why,
    double Score);
