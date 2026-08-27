using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Host.Tests.ConventionTests;

public class ApplicationConventionTests : BaseArchitectureTests
{
    [Fact]
    public void Query_classes_do_not_depend_on_the_unit_of_work()
    {
        Classes().That().HaveNameEndingWith("Queries")
            .Should().NotDependOnAnyTypesThat().HaveName("IUnitOfWork")
            .Check(Architecture);
    }
}
