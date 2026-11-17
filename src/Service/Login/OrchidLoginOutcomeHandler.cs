using Garden.Share;

namespace Orchid.Core.Login;

/// <summary>
/// 로그인 성공을 구독자에게 알리고 실패는 화면의 인라인 경고로 처리하게 한다.
/// </summary>
public sealed class OrchidLoginOutcomeHandler : ILoginOutcomeHandler
{
    /// <summary>
    /// 로그인이 성공해 부모 화면이 인증 상태를 갱신해야 할 때 발생한다.
    /// </summary>
    public event Func<Task>? Authenticated;

    /// <inheritdoc />
    public async Task OnSucceededAsync(string identityProvider)
    {
        Func<Task>? subscribers = Authenticated;
        if (subscribers is null)
        {
            return;
        }

        foreach (Func<Task> subscriber in subscribers.GetInvocationList().Cast<Func<Task>>())
        {
            await subscriber()
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public Task<LoginFailureDisposition> OnFailedAsync(
        string identityProvider,
        UpstreamUnavailableException failure)
    {
        return Task.FromResult(LoginFailureDisposition.ShowInline);
    }
}
