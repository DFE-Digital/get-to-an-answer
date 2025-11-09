using Common.Models.ViewModels;
using Common.Domain;
using Common.Domain.Frontend;
using Common.Domain.Request.Create;
using Common.Domain.Request.Update;

namespace Common.Models.PageModels;

public class QuestionnairesPageModel : BasePageModel
{
    public QuestionnairesViewModel ViewModel { get; set; } = new();
}