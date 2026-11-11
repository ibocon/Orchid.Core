namespace Orchid.Core.Sync.Relay;

/// <summary>
/// 서브패스(<c>/orchid</c>)가 포함된 릴레이 기본 URL에서 SignalR 허브 및 API 엔드포인트 URI를 안전하게 계산합니다.
/// </summary>
public static class RelayEndpointResolver
{
    private const string HubRelativePath = "hubs/clipboard";

    /// <summary>
    /// 릴레이 기본 URL의 서브패스를 보존하여 SignalR 클립보드 허브 절대 URI를 반환합니다.
    /// </summary>
    /// <param name="relayBaseUrl">서브패스를 포함한 릴레이 기본 URL</param>
    /// <returns>조합된 허브 절대 <see cref="Uri"/></returns>
    public static Uri ResolveHubUri(string relayBaseUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relayBaseUrl);

        Uri normalizedBase = new(relayBaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
        return new Uri(normalizedBase, HubRelativePath);
    }
}