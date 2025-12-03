import {expect, test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {UpdateQuestionnaireSlugPage} from "../../pages/admin/UpdateQuestionnaireSlugPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {
    createQuestionnaire,
    getQuestionnaire,
    listQuestionnaires,
    updateQuestionnaire
} from "../../test-data-seeder/questionnaire-data";
import {
    goToEditQuestionnairePageByUrl,
    goToUpdateQuestionnairePageByUrl, goToUpdateQuestionPageByUrl,
    signIn
} from "../../helpers/admin-test-helper";
import {EntityStatus, PageHeadings} from "../../constants/test-data-constants";
import {expect200HttpStatusCode} from "../../helpers/api-assertions-helper";
import {createQuestion} from "../../test-data-seeder/question-data";
import {AddQuestionPage} from "../../pages/admin/AddQuestionPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";

test.describe('Get to an answer update question', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;

    let viewQuestionnairePage: ViewQuestionnairePage;
    let viewQuestionPage: ViewQuestionPage;
    let addQuestionPage: AddQuestionPage;
    let editQuestionnairePage: EditQuestionnairePage;
    let updateQuestionnaireSlugPage: UpdateQuestionnaireSlugPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        const q1Resp = await createQuestion(request, questionnaireId, token);
        question1Id = q1Resp.question.id;
    });

    test('Edit question page displays all required elements', async ({page}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);
        await addQuestionPage.assertPageElements();
    });

    test('Back link takes to view question list from Add question page', async ({page}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);
        
        await addQuestionPage.verifyBackLinkPresent();
        await addQuestionPage.clickBackLink();

        viewQuestionPage = new ViewQuestionPage(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });
});