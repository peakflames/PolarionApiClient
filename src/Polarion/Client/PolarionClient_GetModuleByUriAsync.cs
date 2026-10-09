namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Get a module by its uri.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<Module>> GetModuleByUriAsync(string uri)
    {
        try
        {
            try
            {
                var result = await _trackerClient.getModuleByUriAsync(new(uri));
                return result is null ? Result.Fail("Module not found") : result.getModuleByUriReturn;
            }
            catch (Exception ex)
            {
                throw new PolarionClientException($"Failed to get Module for uri '{uri}'", ex);
            }
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get documents. {ex.Message}");
        }
    }

    /// <summary>
    /// Tells an empty document from a missing one after a module read returned no rows.
    /// </summary>
    /// <remarks>
    /// <c>getModuleWorkItems</c> and <c>getModuleWorkItemUris</c> can answer a URI with no document
    /// behind it (a mistyped location, or a revision before the document existed) with an empty body,
    /// the same as an empty document. <c>getModuleByUri</c> on that URI returns a <see cref="Module"/>
    /// with <c>unresolvable</c> set, which is the signal used here. Only called on the zero-row path,
    /// so reads that return rows cost no extra call.
    /// </remarks>
    /// <param name="moduleUri">The module URI exactly as read, including any <c>%revision</c> suffix</param>
    /// <param name="notFoundMessage">Failure message when the module is unresolvable</param>
    /// <returns>Ok when the module resolves; otherwise a failure. A failed lookup (or an empty lookup
    /// response) is also a failure, because the empty result cannot then be confirmed as an empty
    /// document</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    private async Task<Result> EnsureModuleResolvableAsync(string moduleUri, string notFoundMessage)
    {
        Module? module;
        try
        {
            // Called directly rather than through GetModuleByUriAsync, which drops the server's message.
            module = (await _trackerClient.getModuleByUriAsync(new(moduleUri)))?.getModuleByUriReturn;
        }
        catch (Exception ex)
        {
            return Result.Fail($"{notFoundMessage} (lookup failed: {ex.Message})");
        }

        return module is null || module.unresolvable
            ? Result.Fail(notFoundMessage)
            : Result.Ok();
    }
}
