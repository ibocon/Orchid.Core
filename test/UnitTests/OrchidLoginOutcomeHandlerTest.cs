using FluentAssertions;
using Garden.Share;
using Orchid.Core.Login;
using Xunit;

namespace Orchid.Core.UnitTests;

public sealed class OrchidLoginOutcomeHandlerTest
{
    [Fact]
    public async Task OnSucceededAsyncRaisesAuthenticatedForEverySubscriber()
    {
        OrchidLoginOutcomeHandler sut = new();
        int first = 0;
        int second = 0;
        sut.Authenticated += () =>
        {
            first++;
            return Task.CompletedTask;
        };
        sut.Authenticated += () =>
        {
            second++;
            return Task.CompletedTask;
        };

        await sut.OnSucceededAsync("google")
            .ConfigureAwait(true);

        first.Should().Be(1);
        second.Should().Be(1);
    }

    [Fact]
    public async Task OnSucceededAsyncWithoutSubscribersCompletes()
    {
        OrchidLoginOutcomeHandler sut = new();

        Func<Task> act = () => sut.OnSucceededAsync("google");

        await act.Should().NotThrowAsync()
            .ConfigureAwait(true);
    }

    [Fact]
    public async Task OnFailedAsyncReturnsShowInline()
    {
        OrchidLoginOutcomeHandler sut = new();

        LoginFailureDisposition actual = await sut.OnFailedAsync(
                "google",
                new UpstreamUnavailableException("bad gateway"))
            .ConfigureAwait(true);

        actual.Should().Be(LoginFailureDisposition.ShowInline);
    }
}
