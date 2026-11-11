namespace Orchid.Core.Notification;

/// <summary>
/// 네이티브 OS 토스트 알림 또는 시스템 배너 표시 인터페이스입니다.
/// </summary>
public interface INativeNotification
{
    /// <summary>
    /// 사용자에게 동기화 완료 또는 이벤트 알림을 표시합니다.
    /// </summary>
    /// <param name="title">알림 제목</param>
    /// <param name="message">알림 내용</param>
    Task ShowNotificationAsync(string title, string message);
}
