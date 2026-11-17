// keep: FluentAssertions extension methods
using FluentAssertions;
// keep: NetArchTest fluent rules
using NetArchTest.Rules;
// keep: BindingFlags for entity member inspection
using System.Reflection;
// keep: xUnit Fact attribute
using Xunit;

namespace Orchid.Core.ArchTests;

/// <summary>
/// Core 라이브러리의 플랫폼 무의존성 및 도메인 순수성 불변식을 검증하는 아키텍처 테스트입니다.
/// </summary>
public sealed class CoreArchitectureRulesTest
{
    [Fact]
    public void CoreAssemblyShouldNotDependOnUiFrameworks()
    {
        TestResult result = Types.InAssembly(CoreArchitecture.CoreAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Garden.Razor",
                "Microsoft.AspNetCore.Components",
                "MudBlazor",
                "Microsoft.Maui")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Core는 Garden.Share 계약까지만 허용하며 UI 프레임워크에 의존해서는 안 됩니다.");
    }

    /// <summary>
    /// Core 도메인이 플랫폼 UI 및 프레임워크에 오염되지 않는지 검증합니다.
    /// </summary>
    [Fact]
    public void DomainShouldNotDependOnPlatformOrUiFrameworks()
    {
        // Given & When
        // ✅ Core 도메인이 플랫폼 UI 및 프레임워크에 오염되지 않음을 강제
        TestResult result = Types.InAssembly(CoreArchitecture.CoreAssembly)
            .That()
            .ResideInNamespace("Orchid.Core.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.AspNetCore",
                "Microsoft.Maui",
                "Orchid.Desktop",
                "Orchid.Mobile",
                "Orchid.Web",
                "Orchid.Service")
            .GetResult();

        // Then
        result.IsSuccessful.Should().BeTrue("Core 도메인은 플랫폼이나 상위 계층에 의존해서는 안 됩니다.");
    }

    /// <summary>
    /// 도메인 불변성 유지를 위해 비추상 엔티티와 값 객체가 sealed로 선언되어 있는지 검증합니다.
    /// </summary>
    [Fact]
    public void DomainEntitiesAndValueObjectsShouldBeSealed()
    {
        // Given & When
        // ✅ 도메인 불변성 유지를 위해 비추상 엔티티와 값 객체의 sealed 강제
        TestResult result = Types.InAssembly(CoreArchitecture.CoreAssembly)
            .That()
            .ResideInNamespace("Orchid.Core.Domain")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveName("Result")
            .Should()
            .BeSealed()
            .GetResult();

        // Then
        result.IsSuccessful.Should().BeTrue("Core 도메인의 구체 클래스는 상속 오용 방지를 위해 sealed여야 합니다.");
    }

    /// <summary>
    /// 도메인이 저장 기술(SQLite)을 알지 못하는지 검증합니다.
    /// </summary>
    [Fact]
    public void DomainShouldNotDependOnSqlite()
    {
        // Given & When
        // ✅ 매핑 특성이 도메인으로 돌아오지 않도록 SQLite 의존을 막는다
        TestResult result = Types.InAssembly(CoreArchitecture.CoreAssembly)
            .That()
            .ResideInNamespace("Orchid.Core.Domain")
            .ShouldNot()
            .HaveDependencyOnAny("SQLite", "SQLitePCL")
            .GetResult();

        // Then
        result.IsSuccessful.Should().BeTrue(
            "도메인은 저장 기술을 몰라야 합니다. 실패한 타입: {0}",
            string.Join(", ", result.FailingTypeNames ?? []));
    }

    /// <summary>
    /// 도메인 엔티티가 공개 생성자와 공개 setter를 노출하지 않는지 검증합니다.
    /// </summary>
    [Fact]
    public void DomainEntitiesShouldNotExposePublicConstructorsOrSetters()
    {
        // Given
        Type[] entities =
        [
            .. CoreArchitecture.CoreAssembly
                .GetTypes()
                .Where(t => t.Namespace == "Orchid.Core.Domain.Entity" && t.IsClass && !t.IsAbstract)
        ];

        // When
        string[] violations =
        [
            .. entities.SelectMany(t => t
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Select(_ => $"{t.Name}: public constructor")),
            .. entities.SelectMany(t => t
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.SetMethod is { IsPublic: true })
                .Select(p => $"{t.Name}.{p.Name}: public setter")),
        ];

        // Then
        // ✅ 위반한 타입과 멤버가 실패 메시지에 그대로 나타난다
        violations.Should().BeEmpty("도메인 엔티티는 팩토리와 행위 메서드로만 만들고 바꿔야 합니다.");
    }
}
