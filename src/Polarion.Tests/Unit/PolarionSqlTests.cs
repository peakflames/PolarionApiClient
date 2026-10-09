using FluentAssertions;
using Polarion;
using Polarion.Generated.Tracker;
using Polarion.Tests.Unit.Fakes;
using Xunit;

namespace Polarion.Tests.Unit;

/// <summary>
/// Unit tests for SQL value escaping and its use in module queries - no server connection required.
/// </summary>
public class PolarionSqlTests
{
    #region EscapeLiteral

    [Theory]
    [InlineData("plain", "plain")]
    [InlineData("", "")]
    [InlineData("O'Brien", "O''Brien")]
    [InlineData("''", "''''")]
    [InlineData("x' OR '1'='1", "x'' OR ''1''=''1")]
    [InlineData("50%_off\\", "50%_off\\")]
    public void EscapeLiteral_DoublesSingleQuotesOnly(string input, string expected)
    {
        PolarionSql.EscapeLiteral(input).Should().Be(expected);
    }

    [Fact]
    public void EscapeLiteral_Null_Throws()
    {
        var act = () => PolarionSql.EscapeLiteral(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region EscapeLikePattern

    [Theory]
    [InlineData("plain", "plain")]
    [InlineData("", "")]
    [InlineData("100%", "100\\%")]
    [InlineData("a_b", "a\\_b")]
    [InlineData("C:\\dir", "C:\\\\dir")]
    [InlineData("O'Brien", "O''Brien")]
    [InlineData("\\%", "\\\\\\%")]
    [InlineData("%' OR '1'='1' --", "\\%'' OR ''1''=''1'' --")]
    public void EscapeLikePattern_EscapesWildcardsBackslashAndQuotes(string input, string expected)
    {
        PolarionSql.EscapeLikePattern(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("x' OR '1'='1")]
    [InlineData("'; DROP TABLE MODULE; --")]
    [InlineData("a'b'c'")]
    public void EscapeLikePattern_ResultNeverContainsUnpairedQuote(string input)
    {
        var escaped = PolarionSql.EscapeLikePattern(input);
        escaped.Replace("''", string.Empty).Should().NotContain("'");
    }

    #endregion

    #region GetModulesThinAsync / GetModulesInSpaceThinAsync query construction

    private static (PolarionClient Client, FakeService Tracker) CreateModuleClient(string projectId = FakeClient.ProjectId)
    {
        var (client, tracker) = FakeClient.Create(projectId);
        tracker.On<queryModulesBySQLRequest, queryModulesBySQLResponse>(
            "queryModulesBySQLAsync", _ => new queryModulesBySQLResponse([]));
        return (client, tracker);
    }

    private static string SentSql(FakeService tracker) =>
        tracker.RequestsFor<queryModulesBySQLRequest>("queryModulesBySQLAsync").Single().sqlQuery;

    [Fact]
    public async Task GetModulesThinAsync_TitleFilter_IsEscapedWithEscapeClause()
    {
        var (client, tracker) = CreateModuleClient();

        var result = await client.GetModulesThinAsync(titleContains: "x' OR '1'='1");

        result.IsSuccess.Should().BeTrue();
        SentSql(tracker).Should().Contain(@"AND UPPER(doc.C_TITLE) LIKE '%X'' OR ''1''=''1%' ESCAPE '\'");
    }

    [Fact]
    public async Task GetModulesThinAsync_ExcludeSpaceFilter_EscapesWildcards()
    {
        var (client, tracker) = CreateModuleClient();

        await client.GetModulesThinAsync(excludeSpaceNameContains: "a_b%");

        SentSql(tracker).Should().Contain(@"AND UPPER(doc.C_MODULEFOLDER) NOT LIKE '%A\_B\%%' ESCAPE '\'");
    }

    [Fact]
    public async Task GetModulesThinAsync_NoFilters_HasNoLikeClause()
    {
        var (client, tracker) = CreateModuleClient();

        await client.GetModulesThinAsync();

        SentSql(tracker).Should().NotContain("LIKE");
        SentSql(tracker).Should().Contain($"proj.C_ID = '{FakeClient.ProjectId}'");
    }

    [Fact]
    public async Task GetModulesThinAsync_ProjectIdWithQuote_IsEscaped()
    {
        var (client, tracker) = CreateModuleClient("Proj'X");

        await client.GetModulesThinAsync();

        SentSql(tracker).Should().Contain("proj.C_ID = 'Proj''X'");
    }

    [Fact]
    public async Task GetModulesInSpaceThinAsync_SpaceNameWithQuote_IsEscaped()
    {
        var (client, tracker) = CreateModuleClient();

        var result = await client.GetModulesInSpaceThinAsync("space' OR '1'='1");

        result.IsSuccess.Should().BeTrue();
        SentSql(tracker).Should().Contain("doc.C_MODULEFOLDER = 'space'' OR ''1''=''1'");
    }

    #endregion
}
