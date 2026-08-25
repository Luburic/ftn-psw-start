using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace Host.Tests;

public abstract class BaseArchitectureTests
{
    protected static readonly string[] Modules = ["Exploration", "Games", "Social", "Payment"];
    protected static readonly string[] Layers = ["Api", "Application", "Contracts", "Domain", "Infrastructure"];
    protected static readonly string[] SharedProjects = ["Shared.Domain", "Shared.Api", "Shared.Infrastructure"];

    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(Modules
            .SelectMany(module => Layers.Select(layer => Assembly.Load($"{module}.{layer}")))
            .Append(Assembly.Load("Identity"))
            .Append(Assembly.Load("Host.Api"))
            .Concat(SharedProjects.Select(Assembly.Load))
            .ToArray())
        .Build();

    public static TheoryData<string> ModuleNames => [.. Modules];
    public static TheoryData<string> SharedProjectNames => [.. SharedProjects];

    protected static void AssertNoDependency(string sourceAssembly, params string[] forbiddenAssemblies)
    {
        if (IsEmpty(sourceAssembly))
        {
            return;
        }

        var forbidden = forbiddenAssemblies.Select(Assembly.Load).ToArray();
        Types().That().ResideInAssembly(Assembly.Load(sourceAssembly))
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(forbidden.First(), forbidden.Skip(1).ToArray()))
            .Check(Architecture);
    }

    protected static void AssertNoNamespaceDependency(string sourceAssembly, params string[] forbiddenNamespaces)
    {
        if (IsEmpty(sourceAssembly))
        {
            return;
        }

        foreach (var forbiddenNamespace in forbiddenNamespaces)
        {
            Types().That().ResideInAssembly(Assembly.Load(sourceAssembly))
                .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching($"^{forbiddenNamespace.Replace(".", "\\.")}(\\..*)?$")
                .Check(Architecture);
        }
    }

    private static bool IsEmpty(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        return Architecture.Types.All(type => type.Assembly.FullName != assembly.FullName);
    }
}
