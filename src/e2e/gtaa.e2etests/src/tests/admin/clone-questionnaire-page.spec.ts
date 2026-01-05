import {expect, test} from '@playwright/test';
import {JwtHelper} from '../../helpers/JwtHelper';
import {goToDesignQuestionnairePageByUrl, signIn} from '../../helpers/admin-test-helper';
import {addContributor, createQuestionnaire, updateQuestionnaire} from '../../test-data-seeder/questionnaire-data';
import {CloneQuestionnairePage} from '../../pages/admin/CloneQuestionnairePage';
import {AnswerDestinationType, ErrorMessages, PageHeadings, QuestionType} from "../../constants/test-data-constants";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {PublishQuestionnaireConfirmationPage} from "../../pages/admin/PublishQuestionnaireConfirmationPage";
import {UpdateQuestionnaireSlugPage} from "../../pages/admin/UpdateQuestionnaireSlugPage";

test.describe('Clone questionnaire page', () => {
    let token: string;
    let questionnaireId: string;
    let questionnaireTitle: string;
    let cloneQuestionnairePage: CloneQuestionnairePage;
    let viewQuestionnairePage: ViewQuestionnairePage;
    let designQuestionnairePage: DesignQuestionnairePage;
    let updateQuestionnaireSlugPage: UpdateQuestionnaireSlugPage;
    
    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
        questionnaireTitle = questionnaire.title;
    });

    test('Make a copy/Clone questionnaire page render with original title', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();

        await designQuestionnairePage.openMakeACopyPage();

        cloneQuestionnairePage = await CloneQuestionnairePage.create(page);

        await cloneQuestionnairePage.expectPrefilledTitle(questionnaireTitle);
    });
    
    test('shows validation errors when the title is blank', async ({page, browserName}) => {
        viewQuestionnairePage = await signIn(page, token);
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

    test('Cloning/making copy of questionnaire redirects to Design a questionnaire page of new copied questionnaire', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
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
        viewQuestionnairePage = await signIn(page, token);
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

    test('click back on clone questionnaire page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();

        await designQuestionnairePage.openMakeACopyPage();

        cloneQuestionnairePage = await CloneQuestionnairePage.create(page);
        await cloneQuestionnairePage.clickBackLink();
        
        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();
    });

    test('Validate inline error message missing title', async ({page, browserName}) => {
        viewQuestionnairePage = await signIn(page, token);
        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();

        await designQuestionnairePage.openMakeACopyPage();
        cloneQuestionnairePage = await CloneQuestionnairePage.create(page);
        
        await cloneQuestionnairePage.clearTitle();
        await cloneQuestionnairePage.clickMakeCopy();
        
        await cloneQuestionnairePage.validateInlineTitleError();
    });
    
    test('Clone a published questionnaire and validate Nor started statuses on design questionnaire page', async ({page, request}) => {
        await addContributor(request, questionnaireId, 'user-1', token)
        
        const { question } = await createQuestion(request, questionnaireId, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);
        
        await createSingleAnswer(request, {
            questionnaireId: questionnaireId, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)

        await signIn(page, token);
        
        // Go to design a Questionnaire page and trigger publish flow
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        await designQuestionnairePage.createQuestionnaireId();
        updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);
        await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING);

        const slugInput = `test-slug-${Date.now()}`;
        await updateQuestionnaireSlugPage.enterSlug(slugInput);
        await updateQuestionnaireSlugPage.submit();
        
        // This mirrors the existing pattern used for delete confirmation flows
        await designQuestionnairePage.publishQuestionnaire();

        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaireId}.*/track`));
        await designQuestionnairePage.taskStatusAddEditQuestionnaireId('Completed', 'Add or edit questionnaire ID');

        // Make a clone of a questionnaire
        await designQuestionnairePage.openMakeACopyPage();
        cloneQuestionnairePage = await CloneQuestionnairePage.create(page);
        await cloneQuestionnairePage.expectPrefilledTitle(questionnaireTitle);
        
        await cloneQuestionnairePage.clickMakeCopy();
        await designQuestionnairePage.taskStatusAddEditQuestionnaireId('Not started', 'Add or edit questionnaire ID');
        await designQuestionnairePage.taskStatusAddEditQuestionsAnswers('Not started', 'Add or edit questions and answers');
    })
});