using FluentAssertions;
using Garden.Share;
using Orchid.Core.Login;
using Xunit;

namespace Orchid.Core.UnitTests;

public sealed class OrchidLoginBrandingTest
{
    [Fact]
    public void CreateWithDefaultsUsesOrchidNameLogoAndCancel()
    {
        LoginBranding actual = OrchidLoginBranding.Create();

        actual.AppName.Should().Be("모두클립");
        actual.LogoSource.Should().Be("logo-login.svg");
        actual.AllowCancel.Should().BeTrue();
    }

    [Fact]
    public void CreateReturnsGoogleKakaoMicrosoftInOrder()
    {
        LoginBranding actual = OrchidLoginBranding.Create();

        actual.Providers.Select(provider => provider.Key)
            .Should()
            .Equal("google", "kakao", "microsoft");
        actual.Providers.Select(provider => provider.Kind)
            .Should()
            .Equal(LoginProviderKind.Google, LoginProviderKind.Kakao, LoginProviderKind.Microsoft);
    }

    [Fact]
    public void CreateWithCustomLogoAndWithoutCancelHonorsArguments()
    {
        LoginBranding actual = OrchidLoginBranding.Create("_content/Orchid.Web/logo-login.svg", allowCancel: false);

        actual.LogoSource.Should().Be("_content/Orchid.Web/logo-login.svg");
        actual.AllowCancel.Should().BeFalse();
    }

    [Fact]
    public void CreateWithBlankLogoThrows()
    {
        Action act = () => OrchidLoginBranding.Create("   ");

        act.Should().Throw<ArgumentException>();
    }
}
