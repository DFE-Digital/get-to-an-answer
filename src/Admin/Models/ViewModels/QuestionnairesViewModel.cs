namespace Admin.Models.ViewModels;

public class QuestionnairesViewModel
{
    public bool JustCreated { get; set; }
    public bool JustCloned { get; set; }
    public bool JustUpdated { get; set; }
    public bool JustDeleted { get; set; }
    public bool JustMovedUp { get; set; }
    public bool JustMovedDown { get; set; }
    public bool JustPublished { get; set; }
    public bool JustUnpublished { get; set; }
    public bool InviteAccepted { get; set; }
}