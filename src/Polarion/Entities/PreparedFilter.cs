namespace Polarion;

/// <summary>
/// Represents a filter configuration for querying Polarion work items.
/// </summary>
/// <param name="WorkItemFilter">The filter expression for work items.</param>
/// <param name="Order">The ordering criteria for the results.</param>
/// <param name="Fields">The list of fields to include in the results.</param>
public record PolarionFilter(string WorkItemFilter, string Order, List<string> Fields)
{

    /// <summary>
    /// Creates a PolarionFilter object with the specified criteria.
    /// </summary>
    /// <param name="workItemFilter">if null, then all work item types are included (e.g., type:testCase OR type:testStep)</param>
    /// <param name="headings">Whether to include heading items.</param>
    /// <param name="paragraphs">Whether to include paragraph items.</param>
    /// <param name="customFields">Whether to include custom fields in the results.</param>
    /// <param name="links">Whether to include linked work items in the results.</param>
    /// <returns></returns>
    public static PolarionFilter Create(string? workItemFilter, bool headings, bool paragraphs, List<string> customFields, bool links)
    {
        var fields = new List<string>()
        {
            "id",
            "outlineNumber",
            "type",
            "description",
            "status",
            "title",
        };

        if (customFields.Count > 0)
        {
            // Add custom fields to the list of fields to include in the results
            fields.AddRange(customFields);
        }

        if (links)
        {
            fields.Add("linkedWorkItems");
        }

        if (!string.IsNullOrWhiteSpace(workItemFilter))
        {
            if (headings)
            {
                workItemFilter += " OR type:heading";
            }

            if (paragraphs)
            {
                workItemFilter += " OR type:paragraph";
            }
        }

        return new PolarionFilter(
            string.IsNullOrWhiteSpace(workItemFilter) ? "" : $"({workItemFilter})",
            "outlineNumber",
            fields
        );
    }
}