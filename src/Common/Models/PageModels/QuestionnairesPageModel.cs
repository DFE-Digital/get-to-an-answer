using Common.Models.ViewModels;

namespace Common.Models.PageModels;

public class QuestionnairesPageModel : BasePageModel
{
    public QuestionnairesViewModel ViewModel { get; set; } = new();
}