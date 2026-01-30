using System.Diagnostics;

namespace Common.Models;

public class QuestionnaireState
{
    public bool JustCreated { get; set; }
    public bool JustCloned { get; set; }
    public bool JustUpdated { get; set; }
    public bool JustAddedStartPage { get; set; }
    public bool JustRemovedStartPage { get; set; }
    public bool JustDeleted { get; set; }
    public bool JustMovedUp { get; set; }
    public bool JustMovedDown { get; set; }
    public bool JustPublished { get; set; }
    public bool JustUnpublished { get; set; }
    public bool InviteAccepted { get; set; }
    public bool JustResetStyling { get; set; }
    public bool JustRemovedStartPageImage { get; set; }
    public bool JustCustomisedStyling { get; set; }
    public bool JustAddedCustomButtonText { get; set; }
    
    public bool JustUpdatedQuestionnaireName { get; set; }
    public bool JustAddedContributor { get; set; }
    public bool JustRemovedContributor { get; set; }
    public bool JustRemovedAsContributor { get; set; }
    public bool JustDeletedQuestion { get; set; }
    public bool JustAddedResultsPage { get; set; }
    public bool JustDeletedResultsPage { get; set; }
    public bool JustUpdatedResultsPage { get; set; }
    public bool JustUpdatedQuestionnaireSlugId { get; set; }

    
    public string? DeletedQuestionTitle { get; set; }
    public string? ContributorEmail { get; set; }
    
    public string? QuestionnaireTitle { get; set; }
    public string? CopiedQuestionnaireTitle { get; set; }
    public string? ResultsPageTitle { get; set; }
}