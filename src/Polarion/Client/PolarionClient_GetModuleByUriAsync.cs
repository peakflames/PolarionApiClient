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
}