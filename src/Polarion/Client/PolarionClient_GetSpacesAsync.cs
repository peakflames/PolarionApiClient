namespace Polarion;

public partial class PolarionClient : IPolarionClient
{    
    [RequiresUnreferencedCode("Uses WCF services which require reflection")]
    public async Task<Result<List<string>>> GetSpacesAsync(string? excludeSpaceNameContains = null)
    {
        try
        {
            var result = await _trackerClient.getDocumentSpacesAsync(new(_config.ProjectId));
            if (result is null)
            {
                return Result.Fail<List<string>>("Failed to get spaces");
            }

            var names = result.getDocumentSpacesReturn;

            // filter out spaces that contain the skipIfSpaceNameContain string
            if (excludeSpaceNameContains is not null)
            {
                names = [.. names.Where(x => !x.ToString().Contains(excludeSpaceNameContains))];
            }

            // sort the list of spaces
            var spaces = names.Select(x => x.ToString()).OrderBy(x => x).ToList();

            return Result.Ok(spaces);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<string>>($"Failed to get spaces. {ex.Message}");
        }
    }
}
