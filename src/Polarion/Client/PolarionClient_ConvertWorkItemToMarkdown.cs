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
                var markdownContent = ConvertPolarionWorkItemHtmlToMarkdown(htmlContent);
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


    public string ConvertPolarionWorkItemHtmlToMarkdown(string htmlContent)
    {
        // Process Polarion's math formula spans by replacing them with pre tags containing the data-source content
        // Pattern matches: <span data-source="..." data-inline="..." class="polarion-rte-formula">...</span>
        var processedHtml = System.Text.RegularExpressions.Regex.Replace(
            htmlContent,
            @"<span\s+(?:[^>]*?\s+)?data-source=""([^""]*?)""(?:\s+[^>]*?)?\s+class=""polarion-rte-formula""[^>]*>.*?</span>",
            match => {
                // Extract the data-source attribute value
                var dataSource = match.Groups[1].Value;
                // Replace with pre tag containing the data-source content
                return $"$${dataSource}$$";
            },
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
        );

        // Process Polarion's cross-reference spans by replacing them with markdown links to the referenced work items
        // Pattern matches: <span class="polarion-rte-link" data-type="crossReference" ... data-item-id="MD-145888" ...>...</span>
        processedHtml = System.Text.RegularExpressions.Regex.Replace(
            processedHtml,
            @"<span\s+(?:[^>]*?\s+)?class=""polarion-rte-link""(?:\s+[^>]*?)?\s+data-type=""crossReference""(?:\s+[^>]*?)?\s+data-item-id=""([^""]*?)""[^>]*>.*?</span>",
            match => {
                // Extract the data-item-id attribute value
                var itemId = match.Groups[1].Value;
                // Replace with a markdown link to the referenced work item
                return $"[{itemId}](#{itemId})";
            },
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
        );

        var markdownContent = _markdownConverter.Convert(processedHtml);
        return markdownContent;
    }
}
