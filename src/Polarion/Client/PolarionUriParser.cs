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
    /// Example: subterra:data-service:objects:/default/Midnight${Module}{moduleFolder}L4_fcs#FCS Memory Loader IDD%200000
    /// 
    /// Note: "${Module}{moduleFolder}" are literal placeholder strings in Polarion's internal URI format.
    /// Based on verified Python implementation in ple_systest_utils/polarion.py
    /// </remarks>
    /// <param name="projectName">The Polarion project name</param>
    /// <param name="moduleFolder">The module folder/space path (e.g., "L4_fcs")</param>
    /// <param name="documentId">The document ID</param>
    /// <param name="revision">The revision number</param>
    /// <returns>A properly formatted module URI with revision</returns>
    public static string BuildModuleUriWithRevision(string projectName, string moduleFolder, string documentId, string revision)
    {
        // Module URIs in Polarion use the format: 
        // subterra:data-service:objects:/default/{ProjectName}${Module}{moduleFolder}{Folder}#{DocumentId}%{Revision}
        // Where ${Module}{moduleFolder} are literal placeholder strings
        return $"subterra:data-service:objects:/default/{projectName}${{Module}}{{moduleFolder}}{moduleFolder}#{documentId}%{revision}";
    }
}
