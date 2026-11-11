using FluentAssertions;
using Orchid.Core.Sync.Relay;
using Xunit;

namespace Orchid.UnitTests.Core;

public sealed class RelayEndpointResolverTest
{
    [Theory]
    [InlineData("https://stag.botanicsoft.com/orchid", "https://stag.botanicsoft.com/orchid/hubs/clipboard")]
    [InlineData("https://stag.botanicsoft.com/orchid/", "https://stag.botanicsoft.com/orchid/hubs/clipboard")]
    [InlineData("https://prod.botanicsoft.com/orchid", "https://prod.botanicsoft.com/orchid/hubs/clipboard")]
    public void ResolveHubUriWhenSubpathBaseUrlGivenShouldPreserveSubpath(string baseUrl, string expected)
    {
        // Given & When
        Uri actual = RelayEndpointResolver.ResolveHubUri(baseUrl);

        // Then
        actual.AbsoluteUri.Should().Be(expected);
    }
}