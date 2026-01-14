namespace Polarion;

/// <summary>
/// Represents a work item along with its revision context from a module query.
/// </summary>
public class WorkItemWithRevisionInfo
{
    /// <summary>
    /// The work item data.
    /// </summary>
    public WorkItem WorkItem { get; set; } = null!;

    /// <summary>
    /// The revision at which this work item was retrieved.
    /// </summary>
    public string Revision { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a historical revision (not HEAD).
    /// </summary>
    public bool IsHistorical { get; set; }

    /// <summary>
    /// The original URI used to fetch this work item.
    /// </summary>
    public string SourceUri { get; set; } = string.Empty;

    /// <summary>
    /// The HEAD revision of this work item (for comparison).
    /// </summary>
    public string HeadRevision { get; set; } = string.Empty;
}
