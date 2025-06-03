namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Converts a Polarion work item to Markdown format.
    /// </summary>
    /// <param name="item">The work item to convert.</param>
    /// <param name="workItemTypeToShortNameMap">A dictionary mapping work item type IDs to short names.</param>
    /// <param name="builder">The StringBuilder to append the Markdown content to.</param>
    [RequiresUnreferencedCode("Uses ReverseMarkdown which requires reflection")]
    protected void ConvertToMarkdown(WorkItem item, Dictionary<string, string> workItemTypeToShortNameMap, StringBuilder builder)
    {
        if (item.type.id == "heading")
        {
            builder.AppendLine();
            // count the number of . in the outlineNumber
            var dotCount = item.outlineNumber.Count(c => c == '.') + 1;
            builder.AppendLine($"{new string('#', dotCount)} {item.outlineNumber} {item.title}");
        }
        else if (item.type.id == "paragraph")
        {
            builder.AppendLine();
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
            // count the number of . in the outlineNumber
            var dotCount = item.outlineNumber.Count(c => c == '.') + 1;

            var type = workItemTypeToShortNameMap.ContainsKey(item.type.id) ? workItemTypeToShortNameMap[item.type.id] : item.type.id;

            builder.AppendLine($"{new string('#', dotCount)} WorkItem (id='{item.id}', type='{type}')");

            var content = item.description.type == "text/html"
                            ? _markdownConverter.Convert(item.description.content)
                            : item.description.content;
            builder.AppendLine(content);
        }
    }
}