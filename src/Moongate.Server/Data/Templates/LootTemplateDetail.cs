namespace Moongate.Server.Data.Templates;

public sealed record LootTemplateDetail(
    string Id,
    int RootNodeCount,
    IReadOnlyList<LootTemplateNodeSummary> Nodes,
    IReadOnlyList<LootTemplateNodeSummary> PreviewItems
);
