using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Server.Data.Templates;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Data.Data.Hues;
using Moongate.UO.Data.Interfaces.Hues;

namespace Moongate.Tests.Server.Endpoints;

public sealed class AdminHueEndpointExtensionsTests
{
    private sealed class FakeHueStore : IHueStore
    {
        private readonly List<Hue> _hues = [];

        public IReadOnlyList<Hue> Hues => _hues;

        public int Count => _hues.Count;

        public void Add(Hue hue)
            => _hues.Add(hue);

        public Hue? GetHue(int index)
            => index >= 0 && index < _hues.Count ? _hues[index] : null;
    }

    [Fact]
    public void HandleGetHue_Zero_ReturnsNone()
    {
        var result = AdminHueEndpointExtensions.HandleGetHue(new FakeHueStore(), 0);

        var ok = Assert.IsType<Ok<HueSummary>>(result);
        Assert.True(ok.Value!.IsNone);
    }

    [Fact]
    public void HandleGetHue_KnownHue_ReturnsColors()
    {
        var store = new FakeHueStore();
        store.Add(new(Enumerable.Repeat((ushort)0x7FFF, 32).ToArray(), 0, 31, "White"));

        var result = AdminHueEndpointExtensions.HandleGetHue(store, 1);

        var ok = Assert.IsType<Ok<HueSummary>>(result);
        Assert.True(ok.Value!.IsKnown);
        Assert.Equal(32, ok.Value.Colors.Count);
    }

    [Fact]
    public void HandleGetHue_UnknownNonZero_ReturnsNotFound()
    {
        var result = AdminHueEndpointExtensions.HandleGetHue(new FakeHueStore(), 99);

        Assert.IsType<NotFound>(result);
    }
}
