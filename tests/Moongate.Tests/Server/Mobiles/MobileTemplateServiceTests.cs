using Moongate.Server.Services.Mobiles;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Tests.Server.Mobiles;

public sealed class MobileTemplateServiceTests
{
    private static MobileTemplateDefinition New(string id)
        => new() { Id = id, Name = id };

    [Fact]
    public void TryGet_IsCaseInsensitive()
    {
        var service = new MobileTemplateService();
        service.UpsertRange([New("Town_Guard")]);

        Assert.True(service.TryGet("town_guard", out var definition));
        Assert.Equal("Town_Guard", definition!.Id);
    }

    [Fact]
    public void TryGet_Unknown_ReturnsFalse()
    {
        var service = new MobileTemplateService();

        Assert.False(service.TryGet("missing", out _));
    }

    [Fact]
    public void UpsertRange_SameId_Replaces()
    {
        var service = new MobileTemplateService();
        service.UpsertRange([New("guard")]);
        service.UpsertRange([new MobileTemplateDefinition { Id = "guard", Name = "Replaced" }]);

        Assert.Equal(1, service.Count);
        Assert.True(service.TryGet("guard", out var definition));
        Assert.Equal("Replaced", definition!.Name);
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var service = new MobileTemplateService();
        service.UpsertRange([New("a"), New("b")]);

        service.Clear();

        Assert.Equal(0, service.Count);
        Assert.Empty(service.GetAll());
    }
}
