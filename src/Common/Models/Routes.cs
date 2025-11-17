// C#
namespace Common.Models;

public static class Routes
{
    // Questionnaires
    public static readonly string QuestionnairesList = AdminRoot + "/questionnaires";
    public const string QuestionnairesManage = AdminRoot + "/questionnaires/manage";
    public static readonly string QuestionnairesCreate = AdminRoot + "/questionnaires/create";

    // Track a specific questionnaire (format with id)
    // Usage: string.Format(Routes.QuestionnaireTrackById, questionnaireId)
    public const string QuestionnaireTrackById = AdminRoot + "/questionnaires/{0}/track";

    // Terms of service agreement
    public static readonly string TermsOfServiceAgreement = AdminRoot + "/terms-of-service-agreement";

    // Admin home (area root)
    public static readonly string AdminHomeIndex = AdminRoot + "/home/index";

    // Root
    public const string Root = "/";
    
    // Admin area prefix
    private const string AdminRoot = "/admin";
    public const string GlobalErrorPage = "/error";
    
    public const string AddQuestion = AdminRoot + "/questionnaires/{0}/questions/add";
    
    //add answer option
    public const string AddAnswerOptions = AdminRoot + "/questionnaires/{0}/questions/{1}/answers/add";
}