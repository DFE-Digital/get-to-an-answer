using Admin.Pages.Contents;
using Common.Client;
using Common.Domain;
using Common.Domain.Request.Update;
using Common.Enum;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Unit.Tests.Pages.Admin;

public class EditContentShowResultsSummaryTests
{
    private readonly Mock<IApiClient> _mockApiClient = new();
    private readonly Mock<ILogger<EditContent>> _mockLogger = new();

    [Fact]
    public async Task OnPostSaveContent_ShowResultsSummaryToggle_Is_Sent_To_Api()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var pageModel = new EditContent(_mockApiClient.Object, _mockLogger.Object)
        {
            ContentId = contentId,
            QuestionnaireId = Guid.NewGuid(),
            ContentTitle = "Result",
            ContentValue = "Result content",
            ShowResultsSummary = true
        };

        // Act
        var result = await pageModel.OnPostSaveContentAsync();

        // Assert
        _mockApiClient.Verify(x => x.UpdateContentAsync(
            contentId,
            It.Is<UpdateContentRequestDto>(r => r.ShowResultsSummary == true)), Times.Once);
        result.Should().BeOfType<RedirectResult>();
    }
}
