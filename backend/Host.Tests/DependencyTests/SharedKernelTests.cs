using Xunit;

namespace Host.Tests.DependencyTests;

// No Shared project may know about any module and Shared.Domain stays free of frameworks.
public class SharedKernelTests : BaseArchitectureTests
{
    [Theory]
    [MemberData(nameof(SharedProjectNames))]
    public void Shared_project_does_not_depend_on_any_module(string sharedProject)
    {
        var moduleAssemblies = Modules
            .SelectMany(module => Layers.Select(layer => $"{module}.{layer}"))
            .Append("Identity")
            .ToArray();
        AssertNoDependency(sharedProject, moduleAssemblies);
    }

    [Fact]
    public void Shared_domain_is_framework_free()
    {
        AssertNoNamespaceDependency("Shared.Domain", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore");
    }
}
