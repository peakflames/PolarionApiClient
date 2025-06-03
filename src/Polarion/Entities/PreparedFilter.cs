namespace Polarion;

/// <summary>
/// Represents a filter configuration for querying Polarion work items.
/// </summary>
/// <param name="WorkItemFilter">The filter expression for work items.</param>
/// <param name="Order">The ordering criteria for the results.</param>
/// <param name="Fields">The list of fields to include in the results.</param>
public record PolarionFilter(string WorkItemFilter, string Order, List<string> Fields);