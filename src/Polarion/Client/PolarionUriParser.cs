namespace Polarion;

/// <summary>
/// Utilities for parsing Polarion URIs to extract work item IDs and revisions.
/// </summary>
public static class PolarionUriParser
{
    /// <summary>
    /// Extracts the work item ID from a Polarion work item URI.
    /// </summary>
    /// <remarks>
    /// URI Format: subterra:data-service:objects:/default/{Project}${ModuleFolder}#{WorkItemId}%{Revision}
    /// Algorithm: Split by '/', take last segment, split by '}', take last segment, split by '%', take first segment, trim.
    /// </remarks>
    /// <param name="uri">The Polarion work item URI</param>
    /// <returns>The extracted work item ID, or empty string if URI is null/empty</returns>
    public static string ExtractIdFromUri(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return string.Empty;
        }

        var lastSegment = uri.Split('/').Last();
        var afterBrace = lastSegment.Split('}').Last();
        var beforePercent = afterBrace.Split('%').First();
        return beforePercent.Trim();
    }

    /// <summary>
    /// Extracts the revision number from a Polarion work item URI.
    /// </summary>
    /// <param name="uri">The Polarion work item URI</param>
    /// <returns>The extracted revision number, or empty string if URI is null/empty</returns>
    public static string ExtractRevisionFromUri(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return string.Empty;
        }

        return uri.Split('%').Last().Trim();
    }

    /// <summary>
    /// Builds a module URI with revision for querying work items in a branched document.
    /// </summary>
    /// <remarks>
    /// URI Format: subterra:data-service:objects:/default/{ProjectName}${Module}{moduleFolder}{Folder}#{DocumentId}%{Revision}
    /// Example: subterra:data-service:objects:/default/TestProject${Module}{moduleFolder}MySpace#Example IDD%200000
    ///
    /// Note: "${Module}{moduleFolder}" are literal placeholder strings in Polarion's internal URI format.
    /// </remarks>
    /// <param name="projectName">The Polarion project name</param>
    /// <param name="moduleFolder">The module folder/space path (e.g., "MySpace")</param>
    /// <param name="documentId">The document ID</param>
    /// <param name="revision">The revision number</param>
    /// <returns>A properly formatted module URI with revision</returns>
    public static string BuildModuleUriWithRevision(string projectName, string moduleFolder, string documentId, string revision)
    {
        return $"{BuildModuleUri(projectName, moduleFolder, documentId)}%{revision}";
    }

    /// <summary>
    /// Builds the HEAD module URI for a document.
    /// </summary>
    /// <remarks>
    /// URI Format: subterra:data-service:objects:/default/{ProjectName}${Module}{moduleFolder}{Folder}#{DocumentId}
    /// where "${Module}{moduleFolder}" are literal placeholder strings. The folder and document ID are
    /// used as given (spaces are kept).
    /// </remarks>
    /// <param name="projectName">The Polarion project name</param>
    /// <param name="moduleFolder">The module folder/space path (e.g., "MySpace")</param>
    /// <param name="documentId">The document ID</param>
    /// <returns>The module URI, without a revision suffix</returns>
    internal static string BuildModuleUri(string projectName, string moduleFolder, string documentId)
    {
        return $"subterra:data-service:objects:/default/{projectName}${{Module}}{{moduleFolder}}{moduleFolder}#{documentId}";
    }
}
