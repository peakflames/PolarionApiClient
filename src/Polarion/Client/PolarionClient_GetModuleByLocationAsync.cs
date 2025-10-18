namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Get a module by its location.
    /// </summary>
    /// <param name="location"></param>
    /// <returns>Module</returns>
    /// <exception cref="PolarionClientException"></exception>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<Module>> GetModuleByLocationAsync(string location)
    {
        try
        {
            try
            {
                var result = await _trackerClient.getModuleByLocationAsync(new(_config.ProjectId, location));
                return result is null ? Result.Fail("Module not found") : result.getModuleByLocationReturn;
            }
            catch (Exception ex)
            {
                throw new PolarionClientException($"Failed to get Module from location '{location}'", ex);
            }
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get documents. {ex.Message}");
        }
    }
}