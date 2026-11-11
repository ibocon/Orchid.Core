using System.Text;
using Orchid.Contract.Model;
using Orchid.Contract.Payload;
using Orchid.Core.Cryptography;
using Orchid.Core.Domain.Common;
using Orchid.Core.Domain.Entity;
using Orchid.Core.Domain.ValueObject;
using Orchid.Core.Storage;

namespace Orchid.Core.Sync;

/// <summary>
/// 로컬과 원격 간 E2EE 클립보드 동기화 흐름을 제어하는 오케스트레이션 엔진 구현체입니다.
/// </summary>
public sealed class SyncEngine : ISyncEngine
{
    /// <summary>
    /// 이미 처리한 내용이라 건너뛰었음을 나타내는 오류입니다.
    /// </summary>
    public static readonly Error DuplicateContent = new("Sync.Duplicate", "Duplicate clipboard content already processed.");

    private readonly IAesGcmCryptoService _cryptoService;
    private readonly IClipboardRepository _repository;

    /// <summary>
    /// <see cref="SyncEngine"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public SyncEngine(IAesGcmCryptoService cryptoService, IClipboardRepository repository)
    {
        _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc />
    public async Task<Result<EncryptedPayload>> ProcessLocalClipboardAsync(string plainText, Guid deviceId, byte[] key)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return Result<EncryptedPayload>.Failure("Clipboard text cannot be empty.");
        }

        string hash = PayloadHash.FromString(plainText).Value;
        if (await _repository.ExistsByHashAsync(hash).ConfigureAwait(false))
        {
            return Result<EncryptedPayload>.Failure(DuplicateContent);
        }

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        Result<(byte[] CipherText, byte[] Nonce, byte[] Tag)> encryptResult = _cryptoService.Encrypt(plainBytes, key);
        if (encryptResult.IsFailure)
        {
            return Result<EncryptedPayload>.Failure(encryptResult.Error);
        }

        (byte[] cipherText, byte[] nonce, byte[] tag) = encryptResult.Value;
        Guid id = Guid.NewGuid();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        ClipboardItem item = new(
            id,
            deviceId,
            DevicePlatform.Windows,
            ContentType.Text,
            cipherText,
            nonce,
            tag,
            hash,
            now);

        await _repository.UpsertAsync(item).ConfigureAwait(false);

        EncryptedPayload payload = new(
            id,
            cipherText,
            nonce,
            tag,
            "AES-256-GCM",
            now);

        return Result<EncryptedPayload>.Success(payload);
    }

    /// <inheritdoc />
    public async Task<Result<string>> ProcessRemotePayloadAsync(EncryptedPayload payload, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(payload);

        Result<byte[]> decryptResult = _cryptoService.Decrypt(payload.CipherText, payload.Nonce, payload.Tag, key);
        if (decryptResult.IsFailure)
        {
            return Result<string>.Failure(decryptResult.Error);
        }

        string plainText = Encoding.UTF8.GetString(decryptResult.Value);
        string hash = PayloadHash.FromString(plainText).Value;

        ClipboardItem item = new(
            payload.Id,
            Guid.Empty,
            DevicePlatform.Windows,
            ContentType.Text,
            payload.CipherText,
            payload.Nonce,
            payload.Tag,
            hash,
            payload.CreatedAtUtc);

        await _repository.UpsertAsync(item).ConfigureAwait(false);

        return Result<string>.Success(plainText);
    }
}
