using System.Reflection;
using FluentAssertions;
using Polarion;
using Xunit;

namespace Polarion.Tests.Unit;

/// <summary>
/// Guards the deprecation of owned-items-only module queries.
/// </summary>
public class DeprecationTests
{
    [Theory]
    [InlineData(typeof(PolarionClient))]
    [InlineData(typeof(IPolarionClient))]
    public void GetWorkItemsByModuleAsync_IsObsolete_AndPointsToReplacements(Type type)
    {
        var method = type.GetMethod("GetWorkItemsByModuleAsync");

        var attribute = method!.GetCustomAttribute<ObsoleteAttribute>();

        attribute.Should().NotBeNull();
        attribute!.IsError.Should().BeFalse("existing callers must keep compiling");
        attribute.Message.Should().Contain("GetModuleWorkItemsAsync").And.Contain("QueryWorkItemsInModuleAsync");
    }
}
