using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using Polarion;

namespace Polarion.Tests.Unit;

/// <summary>
/// Unit tests for PolarionUriParser - no server connection required.
/// </summary>
public class PolarionUriParserTests
{
    private readonly ITestOutputHelper _output;

    public PolarionUriParserTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region ExtractIdFromUri Tests

    [Theory]
    // Work item URIs - extracts ID correctly
    [InlineData("subterra:data-service:objects:/default/Project${WorkItem}MD-67890%100", "MD-67890")]
    [InlineData("subterra:data-service:objects:/default/TestProject${WorkItem}TEST-123%1", "TEST-123")]
    // Module URIs - extracts everything after last '}' and before '%' (includes folder#docId for modules)
    [InlineData("subterra:data-service:objects:/default/Project${Module}L4_fcs#MD-12345%200000", "L4_fcs#MD-12345")]
    [InlineData("subterra:data-service:objects:/default/Midnight${Module}{moduleFolder}L4_fcs/Doc#WI-999%50000", "Doc#WI-999")]
    // Edge cases
    [InlineData("", "")]
    [InlineData(null, "")]
    public void ExtractIdFromUri_ShouldExtractCorrectId(string? uri, string expectedId)
    {
        // Act
        var result = PolarionUriParser.ExtractIdFromUri(uri!);

        // Assert
        result.Should().Be(expectedId);
        _output.WriteLine($"URI: '{uri}' => ID: '{result}'");
    }

    [Fact]
    public void ExtractIdFromUri_WithWhitespaceOnlyUri_ShouldReturnEmpty()
    {
        // Arrange
        var uri = "   ";

        // Act
        var result = PolarionUriParser.ExtractIdFromUri(uri);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractIdFromUri_WithNoPercentSign_ShouldReturnFullLastSegment()
    {
        // Arrange - URI without revision (no %)
        var uri = "subterra:data-service:objects:/default/Project${WorkItem}MD-12345";

        // Act
        var result = PolarionUriParser.ExtractIdFromUri(uri);

        // Assert
        result.Should().Be("MD-12345");
    }

    #endregion

    #region ExtractRevisionFromUri Tests

    [Theory]
    [InlineData("subterra:data-service:objects:/default/Project${Module}L4_fcs#MD-12345%200000", "200000")]
    [InlineData("subterra:data-service:objects:/default/Project${WorkItem}MD-67890%100", "100")]
    [InlineData("subterra:data-service:objects:/default/Midnight${Module}{moduleFolder}L4_fcs/Doc#WI-999%50000", "50000")]
    [InlineData("subterra:data-service:objects:/default/TestProject${WorkItem}TEST-123%1", "1")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void ExtractRevisionFromUri_ShouldExtractCorrectRevision(string? uri, string expectedRevision)
    {
        // Act
        var result = PolarionUriParser.ExtractRevisionFromUri(uri!);

        // Assert
        result.Should().Be(expectedRevision);
        _output.WriteLine($"URI: '{uri}' => Revision: '{result}'");
    }

    [Fact]
    public void ExtractRevisionFromUri_WithWhitespaceOnlyUri_ShouldReturnEmpty()
    {
        // Arrange
        var uri = "   ";

        // Act
        var result = PolarionUriParser.ExtractRevisionFromUri(uri);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractRevisionFromUri_WithNoPercentSign_ShouldReturnWholeUri()
    {
        // Arrange - URI without revision marker
        var uri = "uri:without:revision";

        // Act
        var result = PolarionUriParser.ExtractRevisionFromUri(uri);

        // Assert - When no % exists, Split returns original string as single element array
        // so .Last() returns the whole string
        result.Should().Be("uri:without:revision");
    }

    #endregion

    #region BuildModuleUriWithRevision Tests

    [Fact]
    public void BuildModuleUriWithRevision_ShouldConstructValidUri()
    {
        // Arrange
        var projectName = "Midnight";
        var moduleFolder = "L4_fcs";
        var documentId = "FCS Memory Loader IDD";
        var revision = "200000";

        // Act
        var result = PolarionUriParser.BuildModuleUriWithRevision(projectName, moduleFolder, documentId, revision);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("subterra:data-service:objects:/default/");
        result.Should().Contain(projectName);
        result.Should().Contain(moduleFolder);
        result.Should().Contain(documentId);
        result.Should().EndWith($"%{revision}");

        _output.WriteLine($"Built URI: {result}");
    }

    [Fact]
    public void BuildModuleUriWithRevision_ShouldContainLiteralModulePlaceholders()
    {
        // Arrange
        var projectName = "TestProject";
        var moduleFolder = "TestFolder";
        var documentId = "TestDoc";
        var revision = "12345";

        // Act
        var result = PolarionUriParser.BuildModuleUriWithRevision(projectName, moduleFolder, documentId, revision);

        // Assert - Verify literal placeholder strings are present
        result.Should().Contain("${Module}");
        result.Should().Contain("{moduleFolder}");

        _output.WriteLine($"Built URI: {result}");
    }

    [Fact]
    public void BuildModuleUriWithRevision_WithSpecialCharacters_ShouldPreserveCharacters()
    {
        // Arrange
        var projectName = "Midnight";
        var moduleFolder = "L4_fcs";
        var documentId = "FCS Memory Loader IDD"; // Contains spaces
        var revision = "200000";

        // Act
        var result = PolarionUriParser.BuildModuleUriWithRevision(projectName, moduleFolder, documentId, revision);

        // Assert
        result.Should().Contain("FCS Memory Loader IDD");
        
        _output.WriteLine($"Built URI with spaces: {result}");
    }

    [Fact]
    public void BuildModuleUriWithRevision_ShouldFollowExpectedFormat()
    {
        // Arrange
        var projectName = "Midnight";
        var moduleFolder = "L4_fcs";
        var documentId = "FCS Memory Loader IDD";
        var revision = "200000";

        // Act
        var result = PolarionUriParser.BuildModuleUriWithRevision(projectName, moduleFolder, documentId, revision);

        // Expected format: subterra:data-service:objects:/default/{ProjectName}${Module}{moduleFolder}{Folder}#{DocumentId}%{Revision}
        var expectedUri = $"subterra:data-service:objects:/default/{projectName}${{Module}}{{moduleFolder}}{moduleFolder}#{documentId}%{revision}";

        // Assert
        result.Should().Be(expectedUri);
        
        _output.WriteLine($"Expected: {expectedUri}");
        _output.WriteLine($"Actual:   {result}");
    }

    #endregion

    #region URI Parsing Consistency Tests

    [Fact]
    public void ExtractIdAndRevision_FromBuiltUri_ShouldBeConsistent()
    {
        // Arrange
        var projectName = "TestProject";
        var moduleFolder = "TestFolder";
        var documentId = "TEST-12345";
        var revision = "99999";

        // Act - Build a URI then extract components
        var builtUri = PolarionUriParser.BuildModuleUriWithRevision(projectName, moduleFolder, documentId, revision);
        var extractedRevision = PolarionUriParser.ExtractRevisionFromUri(builtUri);

        // Assert
        extractedRevision.Should().Be(revision, "Extracted revision should match the input revision");

        _output.WriteLine($"Built URI: {builtUri}");
        _output.WriteLine($"Extracted Revision: {extractedRevision}");
    }

    #endregion

    #region URI Format Documentation Tests

    [Fact]
    public void DocumentUriFormatComponents()
    {
        // This test documents the expected URI format components for reference
        
        // Standard work item URI format:
        // subterra:data-service:objects:/default/{Project}${WorkItem}{WorkItemId}%{Revision}
        
        // Module URI format:
        // subterra:data-service:objects:/default/{ProjectName}${Module}{moduleFolder}{Folder}#{DocumentId}%{Revision}
        
        // Note: ${Module}{moduleFolder} are literal placeholder strings in Polarion's internal URI format
        
        var workItemUri = "subterra:data-service:objects:/default/Midnight${WorkItem}MD-12345%100";
        var moduleUri = "subterra:data-service:objects:/default/Midnight${Module}{moduleFolder}L4_fcs#FCS Memory Loader IDD%200000";
        
        // Log for documentation purposes
        _output.WriteLine("=== Polarion URI Format Documentation ===");
        _output.WriteLine($"Work Item URI Example: {workItemUri}");
        _output.WriteLine($"  - Extracted ID: {PolarionUriParser.ExtractIdFromUri(workItemUri)}");
        _output.WriteLine($"  - Extracted Revision: {PolarionUriParser.ExtractRevisionFromUri(workItemUri)}");
        _output.WriteLine("");
        _output.WriteLine($"Module URI Example: {moduleUri}");
        _output.WriteLine($"  - Extracted ID: {PolarionUriParser.ExtractIdFromUri(moduleUri)}");
        _output.WriteLine($"  - Extracted Revision: {PolarionUriParser.ExtractRevisionFromUri(moduleUri)}");
        
        // Work item URIs extract the ID correctly
        PolarionUriParser.ExtractIdFromUri(workItemUri).Should().Be("MD-12345");
        PolarionUriParser.ExtractRevisionFromUri(workItemUri).Should().Be("100");
        
        // Module URIs - ExtractIdFromUri returns folder#documentId (includes # separator)
        // This is expected behavior as module URIs have different structure than work item URIs
        PolarionUriParser.ExtractIdFromUri(moduleUri).Should().Be("L4_fcs#FCS Memory Loader IDD");
        PolarionUriParser.ExtractRevisionFromUri(moduleUri).Should().Be("200000");
    }

    #endregion
}
