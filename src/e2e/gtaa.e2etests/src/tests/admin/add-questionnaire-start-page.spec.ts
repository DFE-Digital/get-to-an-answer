import {expect, test} from "@playwright/test";
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {signIn, goToAddQuestionnaireStartPageByUrl} from '../../helpers/admin-test-helper';
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire, getQuestionnaire, updateQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {PageHeadings} from "../../constants/test-data-constants";
import {AddQuestionnaireStartPage} from "../../pages/admin/AddQuestionnaireStartPage";
import {RemoveStartPageConfirmationPage} from "../../pages/admin/RemoveStartPageConfirmationPage";

test.describe('Get to an answer add question to questionnaire', () => {
    let token: string;
    let questionnaire: any;
    let addQuestionnaireStartPage: AddQuestionnaireStartPage;
    let designQuestionnairePage: DesignQuestionnairePage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        const response = await createQuestionnaire(request, token);

        questionnaire = response.questionnaire;

        await signIn(page, token);
        addQuestionnaireStartPage = await goToAddQuestionnaireStartPageByUrl(page, questionnaire.id);
    });
    
    test('Back link takes to edit questionnaire page', async ({page}) => {
        await addQuestionnaireStartPage.clickBackLink();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);
    });

    test('Successful submit of with optional fields populated', async ({request, page}) => {
        await addQuestionnaireStartPage.enterQuestionnaireDisplayTitleInput("Questionnaire Start Page")
        await addQuestionnaireStartPage.enterQuestionnaireDescriptionText("Questionnaire Description");
        await addQuestionnaireStartPage.clickSaveAndContinue();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.expectSuccessBannerVisible();
        await designQuestionnairePage.assertSavedStartPageSuccessBanner();
        
        const { questionnaireGetBody: updatedQuestionnaire } = await getQuestionnaire(request, questionnaire.id, token);
        
        expect(updatedQuestionnaire.displayTitle).toBe("Questionnaire Start Page");
        expect(updatedQuestionnaire.description).toBe("Questionnaire Description");
    });

    test('Unsuccessful submit of with only display title field populated', async ({request, page}) => {
        await addQuestionnaireStartPage.enterQuestionnaireDisplayTitleInput("Questionnaire Start Page")
        await addQuestionnaireStartPage.clickSaveAndContinue();

        await addQuestionnaireStartPage.assertErrorSummaryLinkForDescription();
        await addQuestionnaireStartPage.assertInlineErrorForDescription();

        const { questionnaireGetBody: updatedQuestionnaire } = await getQuestionnaire(request, questionnaire.id, token);

        expect(updatedQuestionnaire.displayTitle).toBeNull();
        expect(updatedQuestionnaire.description).toBeNull();
    });

    test('Unsuccessful submit of with only description field populated', async ({request, page}) => {
        await addQuestionnaireStartPage.enterQuestionnaireDescriptionText("Questionnaire Description");
        await addQuestionnaireStartPage.clickSaveAndContinue();

        await addQuestionnaireStartPage.assertErrorSummaryLinkForDisplayTitle();
        await addQuestionnaireStartPage.assertInlineErrorForDisplayTitle();

        const { questionnaireGetBody: updatedQuestionnaire } = await getQuestionnaire(request, questionnaire.id, token);

        expect(updatedQuestionnaire.displayTitle).toBeNull();
        expect(updatedQuestionnaire.description).toBeNull();
    });

    test('Successful remove start page', async ({request, page}) => {
        await updateQuestionnaire(request, questionnaire.id, {
            displayTitle: "Questionnaire Start Page",
            description: "Questionnaire Description"
        }, token)
        
        await addQuestionnaireStartPage.clickRemoveStartPage();
        
        const removeStartPageConfirmationPage = new RemoveStartPageConfirmationPage(page);

        await removeStartPageConfirmationPage.expectTwoRadiosPresent();
        await removeStartPageConfirmationPage.chooseYes();
        await removeStartPageConfirmationPage.clickContinue();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.expectSuccessBannerVisible();
        await designQuestionnairePage.assertRemovedStartPageSuccessBanner();

        const { questionnaireGetBody: updatedQuestionnaire } = await getQuestionnaire(request, questionnaire.id, token);

        expect(updatedQuestionnaire.displayTitle).toBe('');
        expect(updatedQuestionnaire.description).toBe('');
    });
});