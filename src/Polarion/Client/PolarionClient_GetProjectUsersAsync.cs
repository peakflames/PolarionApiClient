namespace Polarion;

public partial class PolarionClient : IPolarionClient
{
    /// <summary>
    /// Gets the users explicitly assigned to a project (its member list), via
    /// <c>ProjectWebService.getProjectUsers</c>.
    /// </summary>
    /// <param name="projectId">The Polarion project ID</param>
    /// <returns>The project's assigned users</returns>
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<Polarion.Generated.Project.User[]>> GetProjectUsersAsync(string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return Result.Fail("Project ID cannot be null or empty");
        }

        try
        {
            var response = await _projectClient.getProjectUsersAsync(new(projectId));

            return Result.Ok(response?.getProjectUsersReturn ?? []);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get users for project '{projectId}'. {ex.Message}");
        }
    }
}
