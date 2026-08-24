namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Gets every user known to this Polarion instance, via <c>ProjectWebService.getUsers</c>.
    /// This SOAP operation takes no filter — callers needing to resolve a single user by email
    /// or id must filter the returned array themselves, and should cache the result rather than
    /// calling this per lookup.
    /// </summary>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<Polarion.Generated.Project.User[]>> GetUsersAsync()
    {
        try
        {
            var response = await _projectClient.getUsersAsync(new());

            return Result.Ok(response?.getUsersReturn ?? []);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get users. {ex.Message}");
        }
    }
}
