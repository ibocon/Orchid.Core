// keep: Assembly reflection type
using System.Reflection;

namespace Orchid.Core.ArchTests;

/// <summary>
/// Core 라이브러리의 아키텍처 검증을 위한 어셈블리 및 루트 네임스페이스 정의 헬퍼입니다.
/// </summary>
internal static class CoreArchitecture
{
    // ✅ Core 대상 어셈블리 지정
    public static readonly Assembly CoreAssembly = typeof(Domain.Entity.ClipboardItem).Assembly;
    public const string RootNamespace = "Orchid.Core";
}
