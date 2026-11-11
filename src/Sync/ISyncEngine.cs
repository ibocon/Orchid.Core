using Orchid.Contract.Payload;
using Orchid.Core.Domain.Common;

namespace Orchid.Core.Sync;

/// <summary>
/// E2EE 클립보드 데이터의 분산 동기화 및 송수신을 담당하는 동기화 엔진 계약입니다.
/// </summary>
public interface ISyncEngine
{
    /// <summary>
    /// 로컬 클립보드 텍스트를 E2EE 암호화하여 저장소에 저장하고 전송용 페이로드를 생성합니다.
    /// </summary>
    /// <param name="plainText">클립보드 평문 텍스트</param>
    /// <param name="deviceId">발신 디바이스 식별자</param>
    /// <param name="key">AES-256-GCM 대칭키</param>
    /// <returns>생성된 <see cref="EncryptedPayload"/>를 담은 <see cref="Result"/></returns>
    Task<Result<EncryptedPayload>> ProcessLocalClipboardAsync(string plainText, Guid deviceId, byte[] key);

    /// <summary>
    /// 원격 디바이스로부터 수신된 암호화 페이로드를 복호화하고 로컬 저장소에 반영합니다.
    /// </summary>
    /// <param name="payload">수신된 E2EE 암호화 페이로드</param>
    /// <param name="key">AES-256-GCM 대칭키</param>
    /// <returns>복호화된 평문 텍스트를 담은 <see cref="Result"/></returns>
    Task<Result<string>> ProcessRemotePayloadAsync(EncryptedPayload payload, byte[] key);
}
