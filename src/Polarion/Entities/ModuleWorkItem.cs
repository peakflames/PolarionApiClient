namespace Polarion;

/// <summary>
/// A work item row as it appears in a document (module), in document order.
/// </summary>
/// <remarks>
/// Returned by <see cref="IPolarionClient.GetModuleWorkItemsAsync"/>. Covers items owned by the
/// document as well as live and pinned references to items in other documents or projects.
/// </remarks>
public class ModuleWorkItem
{
    /// <summary>
    /// The work item data with the requested fields. Field values reflect the revision the
    /// document points at (the pinned revision for pinned references). For unresolvable rows
    /// Polarion returns no field values, so this object carries little more than its URI.
    /// </summary>
    public WorkItem WorkItem { get; init; } = null!;

    /// <summary>
    /// The work item URI exactly as returned by Polarion, including any <c>%revision</c> suffix.
    /// </summary>
    public string Uri { get; init; } = string.Empty;

    /// <summary>
    /// The work item ID. Taken from the returned data when present, otherwise parsed from <see cref="Uri"/>.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The revision this row is pinned to, parsed from the <c>%revision</c> suffix of <see cref="Uri"/>.
    /// Empty when the row is not pinned: it follows HEAD, or, when the module URI was given with a
    /// revision, the document at that revision. See <see cref="IsPinned"/> for how pins are detected.
    /// </summary>
    public string Revision { get; init; } = string.Empty;

    /// <summary>
    /// True when the row is a pinned reference (<see cref="Revision"/> is set).
    /// </summary>
    /// <remarks>
    /// <para>
    /// HEAD read (module URI without <c>%revision</c>): Polarion suffixes only pinned rows, so any
    /// <c>%revision</c> suffix on the row URI marks a pin.
    /// </para>
    /// <para>
    /// Revision read (module URI with <c>%revision</c>): Polarion suffixes every row. Unpinned rows carry
    /// the requested revision and pinned rows carry their pin revision, so a row counts as pinned only
    /// when its suffix differs from the requested revision.
    /// </para>
    /// <para>
    /// Limit: in a revision read, a reference pinned to exactly the requested revision has the same
    /// suffix as an unpinned row and is reported as not pinned. Its field values are still correct,
    /// since both describe the item at that revision. <see cref="Uri"/> keeps the suffix as returned.
    /// </para>
    /// </remarks>
    public bool IsPinned => !string.IsNullOrEmpty(Revision);

    /// <summary>
    /// True when Polarion marks the row as unresolvable, e.g. a live reference to a work item
    /// that has since been deleted. Such rows carry no field values.
    /// </summary>
    public bool IsUnresolvable { get; init; }
}
