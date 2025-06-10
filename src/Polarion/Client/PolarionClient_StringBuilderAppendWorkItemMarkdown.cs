namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Appends a document-style formatted Markdown representation of a Polarion work item to a StringBuilder.
    /// </summary>
    /// <param name="item">The work item to convert.</param>
    /// <param name="workItemTypeToShortNameMap">A dictionary mapping work item type IDs to short names.</param>
    /// <param name="includeWorkItemIdentifiers">Whether to include the work item identifiers in the Markdown output.</param>
    /// <param name="builder">The StringBuilder to append the Markdown content to.</param>
    [RequiresUnreferencedCode("Uses ReverseMarkdown which requires reflection")]
    protected void StringBuilderAppendWorkItemMarkdown(WorkItem item, Dictionary<string, string> workItemTypeToShortNameMap, bool includeWorkItemIdentifiers, StringBuilder builder)
    {
        var type = workItemTypeToShortNameMap.TryGetValue(item.type.id, out string? value) ? value : item.type.id;

        if (type == "heading")
        {
            builder.AppendLine();
            // count the number of . in the outlineNumber
            var dotCount = item.outlineNumber.Count(c => c == '.') + 1;
            builder.AppendLine($"{new string('#', dotCount)} {item.outlineNumber} {item.title}");
        }
        else if (type == "paragraph")
        {
            builder.AppendLine();

            if (includeWorkItemIdentifiers)
            {
                builder.AppendLine($"__WorkItem(id='{item.id}', type='{type}'):__");
            }
            
            if (item.description is not null)
            {
                var content = item.description.type is not null && item.description.type == "text/html"
                                ? _markdownConverter.Convert(item.description.content)
                                : item.description.content;
                builder.AppendLine(content);
            }
        }
        else
        {
            builder.AppendLine();

            if (includeWorkItemIdentifiers)
            {
                builder.AppendLine($"__WorkItem(id='{item.id}', type='{type}'):__");
            }
            
            var content = item.description.type == "text/html"
                            ? _markdownConverter.Convert(item.description.content)
                            : item.description.content;
            builder.AppendLine(content);
        }
    }
}