using Garden.Share;

namespace Orchid.Core.Login;

/// <summary>
/// Orchid 앱이 공유하는 로그인 화면 브랜딩을 만든다.
/// </summary>
/// <remarks>
/// UI 형식에 의존하지 않는 <see cref="LoginBranding"/>만 반환하므로 Core의 플랫폼 무의존성을 지킨다.
/// 화면 호스팅은 각 UI head가 담당한다.
/// </remarks>
public static class OrchidLoginBranding
{
    /// <summary>
    /// 로그인 화면에 표시하는 앱 이름이다.
    /// </summary>
    public const string AppName = "모두클립";

    /// <summary>
    /// UI head가 공통으로 쓰는 기본 로그인 로고 경로다.
    /// </summary>
    public const string DefaultLogoSource = "logo-login.svg";

    private static readonly LoginProviderOption[] Providers =
    [
        new(IdentityProvider.Google, LoginProviderKind.Google),
        new(IdentityProvider.Kakao, LoginProviderKind.Kakao),
        new(IdentityProvider.Microsoft, LoginProviderKind.Microsoft),
    ];

    /// <summary>
    /// Orchid 로그인 화면 브랜딩을 만든다. 공급자는 문화권과 무관하게 Google, Kakao, Microsoft 순서다.
    /// </summary>
    /// <param name="logoSource">로고 이미지 주소. 기본값은 <see cref="DefaultLogoSource"/>다.</param>
    /// <param name="allowCancel">로그인 진행 중 취소 버튼을 표시할지 여부다.</param>
    /// <returns>검증된 로그인 브랜딩이다.</returns>
    public static LoginBranding Create(
        string logoSource = DefaultLogoSource,
        bool allowCancel = true)
    {
        return new LoginBranding(AppName, logoSource, Providers, allowCancel);
    }
}
