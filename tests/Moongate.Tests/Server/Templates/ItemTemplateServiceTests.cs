using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.Tests.Server.Templates;

public sealed class ItemTemplateServiceTests
{
    [Fact]
    public void Clear_RemovesAllTemplates()
    {
        var service = new ItemTemplateService();
        service.UpsertRange([NewTemplate("a"), NewTemplate("b")]);

        service.Clear();

        Assert.Equal(0, service.Count);
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void GetAll_ReturnsAllRegisteredTemplates()
    {
        var service = new ItemTemplateService();
        service.UpsertRange([NewTemplate("a"), NewTemplate("b")]);

        Assert.Equal(2, service.GetAll().Count);
    }

    [Fact]
    public void TryGet_IsCaseInsensitive()
    {
        var service = new ItemTemplateService();
        service.UpsertRange([NewTemplate("Plain_Shirt")]);

        Assert.True(service.TryGet("plain_shirt", out var definition));
        Assert.Equal("Plain_Shirt", definition!.Id);
    }

    [Fact]
    public void TryGet_UnknownId_ReturnsFalse()
    {
        var service = new ItemTemplateService();

        Assert.False(service.TryGet("missing", out _));
    }

    [Fact]
    public void UpsertRange_SameId_ReplacesExisting()
    {
        var service = new ItemTemplateService();
        service.UpsertRange([NewTemplate("shirt")]);
        var replacement = new ItemTemplateDefinition { Id = "shirt", Name = "Replaced" };

        service.UpsertRange([replacement]);

        Assert.Equal(1, service.Count);
        Assert.True(service.TryGet("shirt", out var definition));
        Assert.Equal("Replaced", definition!.Name);
    }

    private static ItemTemplateDefinition NewTemplate(string id)
        => new() { Id = id, Name = id };
}
