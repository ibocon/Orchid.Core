namespace Orchid.Core.Notification;

/// <summary>
/// 알림 런타임 권한을 요청할지 판정하는 정책입니다.
/// </summary>
public static class NotificationPermissionPolicy
{
    /// <summary>
    /// 권한이 없고 이전에 요청한 적이 없을 때만 요청해야 하는지 판정합니다.
    /// </summary>
    /// <param name="isGranted">현재 권한 허용 여부</param>
    /// <param name="requestedBefore">이전 실행에서 요청한 적이 있는지 여부</param>
    /// <returns>요청해야 하면 true</returns>
    public static bool ShouldRequest(bool isGranted, bool requestedBefore)
    {
        return !isGranted && !requestedBefore;
    }
}
