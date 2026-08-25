using Xunit;

namespace Host.Tests.DependencyTests;

// The layering inside a single module: which of its own projects and which Shared projects each layer may depend on.
public class ModuleLayerTests : BaseArchitectureTests
{
    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Domain_depends_only_on_shared_domain(string module)
    {
        AssertNoDependency($"{module}.Domain",
            $"{module}.Application", $"{module}.Infrastructure", $"{module}.Api", $"{module}.Contracts",
            "Shared.Api", "Shared.Infrastructure");
        AssertNoNamespaceDependency($"{module}.Domain", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore");
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Application_depends_only_on_domain_contracts_and_shared_domain(string module)
    {
        AssertNoDependency($"{module}.Application",
            $"{module}.Infrastructure", $"{module}.Api",
            "Shared.Api", "Shared.Infrastructure");
        AssertNoNamespaceDependency($"{module}.Application", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore");
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Contracts_depends_on_nothing(string module)
    {
        AssertNoDependency($"{module}.Contracts",
            $"{module}.Domain", $"{module}.Application", $"{module}.Infrastructure", $"{module}.Api",
            "Shared.Domain", "Shared.Api", "Shared.Infrastructure");
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Api_does_not_depend_on_infrastructure_shared_infrastructure_or_ef(string module)
    {
        AssertNoDependency($"{module}.Api", $"{module}.Infrastructure", "Shared.Infrastructure");
        AssertNoNamespaceDependency($"{module}.Api", "Microsoft.EntityFrameworkCore");
    }

    // Domain is visible to Api through the project graph (Api -> Application -> Domain),
    // so this is the rule that stops an endpoint from returning a domain entity to the
    // outside world instead of a DTO.
    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Api_does_not_depend_on_domain(string module)
    {
        AssertNoDependency($"{module}.Api", $"{module}.Domain");
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Infrastructure_does_not_depend_on_api_or_shared_api(string module)
    {
        AssertNoDependency($"{module}.Infrastructure", $"{module}.Api", "Shared.Api");
    }
}
