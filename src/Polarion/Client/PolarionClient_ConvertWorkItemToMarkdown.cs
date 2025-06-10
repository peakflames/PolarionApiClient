namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Converts a Polarion work item to Markdown format.
    /// </summary>
    /// <param name="workItemId">The ID of the work item to convert.</param>
    /// <param name="workItem">The work item to convert.</param>
    /// <param name="errorMsgPrefix">An optional prefix to add to the error message.</param>
    /// <param name="includeWorkItemIdentifiers">Whether to include the metadata in the Markdown output.</param>
    /// <returns>The Markdown representation of the work item.</returns>
    [RequiresUnreferencedCode("Uses ReverseMarkdown which requires reflection")]
    public string ConvertWorkItemToMarkdown(string workItemId, WorkItem? workItem, string? errorMsgPrefix = null, bool includeWorkItemIdentifiers = true)
    {
        var sb = new StringBuilder();

        if (!includeWorkItemIdentifiers)
        {
            sb.AppendLine($"## WorkItem (id='{workItemId}')");
        }

        if (workItem is null)
        {
            sb.AppendLine(errorMsgPrefix ?? $"ERROR: WorkItem with ID '{workItemId}' does not exist.");
            return sb.ToString(); ;
        }

        string description = workItem.description?.content?.ToString() ?? "Work Item description was null. Likely does not exist";

        try
        {
            if (workItem.description?.type == "text/html")
            {
                var htmlContent = workItem.description.content?.ToString() ?? "";
                var markdownContent = _markdownConverter.Convert(htmlContent);
                description = markdownContent;
            }
        }
        catch (Exception ex)
        {
            return $"Error extracting data from WorkItem: {ex.Message}";
        }

        if (includeWorkItemIdentifiers)
        {
            sb.AppendLine($"__WorkItem(id='{workItem.id}', type='{workItem.type.id}, lastUpdated='{workItem.updated}'):__");
        }
        sb.AppendLine("");
        sb.AppendLine(description);
        return sb.ToString();
    }
}