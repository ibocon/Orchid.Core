// keep: FluentAssertions extension methods
using FluentAssertions;
// keep: NetArchTest fluent rules
using NetArchTest.Rules;
// keep: xUnit Fact attribute
using Xunit;

namespace Orchid.Core.ArchTests;

/// <summary>
/// Core 라이브러리의 플랫폼 무의존성 및 도메인 순수성 불변식을 검증하는 아키텍처 테스트입니다.
/// </summary>
public sealed class CoreArchitectureRulesTest
{
    /// <summary>
    /// Core 도메인이 플랫폼 UI 및 프레임워크에 오염되지 않는지 검증합니다.
    /// </summary>
    [Fact]
    public void Domain_ShouldNotDependOn_PlatformOrUiFrameworks()
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
    public void DomainEntitiesAndValueObjects_ShouldBeSealed()
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
}
