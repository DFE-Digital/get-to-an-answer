import {expect, test} from '@playwright/test';
import {JwtHelper} from '../../helpers/JwtHelper';
import {signIn} from '../../helpers/admin-test-helper';
import {createQuestionnaire} from '../../test-data-seeder/questionnaire-data';
import {CloneQuestionnairePage} from '../../pages/admin/CloneQuestionnairePage';
import {ErrorMessages} from "../../constants/test-data-constants";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";

test.describe('Clone questionnaire page', () => {
    let token: string;
    let questionnaireId: string;
    let questionnaireTitle: string;
    let cloneQuestionnairePage: CloneQuestionnairePage;
    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let designQuestionnairePage: DesignQuestionnairePage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
        questionnaireTitle = questionnaire.title;

        viewQuestionnairePage = await signIn(page, token);

    });

    test('Make a copy/Clone questionnaire page render with original title', async ({page}) => {
        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();

        await designQuestionnairePage.openMakeACopyPage();

        cloneQuestionnairePage = await CloneQuestionnairePage.create(page);

        await cloneQuestionnairePage.expectPrefilledTitle(questionnaireTitle);
    });


    test('shows validation errors when the title is blank', async ({page, browserName}) => {
        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();

        await designQuestionnairePage.openMakeACopyPage();

        cloneQuestionnairePage = await CloneQuestionnairePage.create(page);

        await cloneQuestionnairePage.expectPrefilledTitle(questionnaireTitle);

        await cloneQuestionnairePage.clearTitle();

        await cloneQuestionnairePage.clickMakeCopy();

        await cloneQuestionnairePage.validateMissingTitleMessageSummary(browserName);
        await cloneQuestionnairePage.validateErrorLinkBehaviour(
            cloneQuestionnairePage.errorSummaryLink('#Title'),
            ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE,
            browserName
        )
    });

    test('Cloning/making copy of questionnaire redirects to Design a questionnaire page of new copied questionnaire', async ({
                                                                                                                                 browserName,
                                                                                                                                 page
                                                                                                                             }) => {
        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();

        await designQuestionnairePage.openMakeACopyPage();

        cloneQuestionnairePage = await CloneQuestionnairePage.create(page);

        await cloneQuestionnairePage.expectPrefilledTitle(questionnaireTitle);

        const newTitle = `Cloned from ${questionnaireTitle} - ${Date.now()}`;

        await cloneQuestionnairePage.enterTitle(newTitle);

        await cloneQuestionnairePage.clickMakeCopy();

        await designQuestionnairePage.validateHeadingAndStatus();

        await designQuestionnairePage.expectSuccessBannerVisible();

        await designQuestionnairePage.assertQuestionnaireTitle(newTitle);
    });
    
    
    test('Cloning questionnaire with invalid title to validate aria-describedby', async ({page}) => {
        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();


        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();

        await designQuestionnairePage.openMakeACopyPage();

        cloneQuestionnairePage = await CloneQuestionnairePage.create(page);

        await cloneQuestionnairePage.expectPrefilledTitle(questionnaireTitle);

        await cloneQuestionnairePage.clearTitle();

        await cloneQuestionnairePage.clickMakeCopy();
        
        await cloneQuestionnairePage.validateTitleFieldAriaDescribedBy();
    })
});