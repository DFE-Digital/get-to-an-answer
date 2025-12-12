import {expect, test} from "@playwright/test";
import {JwtHelper} from "../../helpers/JwtHelper";
import {
    addContributor,
    createQuestionnaire,
    listQuestionnaires,
    updateQuestionnaire
} from "../../test-data-seeder/questionnaire-data";
import {createQuestion} from "../../test-data-seeder/question-data";
import {AnswerDestinationType, EntityStatus, PageHeadings, QuestionType} from "../../constants/test-data-constants";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {goToDesignQuestionnairePageByUrl, goToUpdateQuestionPageByUrl, signIn} from "../../helpers/admin-test-helper";
import {PublishQuestionnaireConfirmationPage} from "../../pages/admin/PublishQuestionnaireConfirmationPage";
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {QuestionnaireVersionHistoryPage} from "../../pages/admin/QuestionnaireVersionHistoryPage";
import {AddQuestionPage, QuestionRadioLabel} from "../../pages/admin/AddQuestionPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";

test.describe('Get to an answer publish questionnaire', () => {
    let token: string;
    let versionTitle: string
    let versionUserEmail: string
    let versionTimestamp: string
    let viewChangesText: string

    let addQuestionPage: AddQuestionPage
    let viewQuestionPage: ViewQuestionPage
    let designQuestionnairePage: DesignQuestionnairePage
    let questionnaireVersionHistoryPage: QuestionnaireVersionHistoryPage

    test.beforeEach(async ({request}) => {
        token = JwtHelper.NoRecordsToken();
    });

    test('Click back on Questionnaire version history page', async ({page, request}) => {
        const {questionnaire} = await createQuestionnaire(request, token);
        await updateQuestionnaire(request, questionnaire.id, {slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}`}, token);

        await addContributor(request, questionnaire.id, 'user-1', token)
        const {question} = await createQuestion(request, questionnaire.id, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)

        await signIn(page, token);

        // Go to Edit a Questionnaire page and trigger publish flow
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaire.id);

        await designQuestionnairePage.publishQuestionnaire();
        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaire.id}.*/track`));

        // Open questionnaire version history
        await designQuestionnairePage.openVersionHistory();
        questionnaireVersionHistoryPage = new QuestionnaireVersionHistoryPage(page);
        await questionnaireVersionHistoryPage.expectHeading(PageHeadings.VIEW_VERSION_HISTORY_PAGE_HEADING);
        
        await questionnaireVersionHistoryPage.clickBackLink();
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaire.id}`));
    });

    test('Validate "Show/Hide all versions" toggle text', async ({page, request}) => {
        const {questionnaire} = await createQuestionnaire(request, token);
        await updateQuestionnaire(request, questionnaire.id, {slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}`}, token);

        await addContributor(request, questionnaire.id, 'user-1', token)
        const {question} = await createQuestion(request, questionnaire.id, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)

        await signIn(page, token);

        // Go to Edit a Questionnaire page and trigger publish flow
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaire.id);

        await designQuestionnairePage.publishQuestionnaire();
        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaire.id}.*/track`));

        // Open questionnaire version history
        await designQuestionnairePage.openVersionHistory();
        questionnaireVersionHistoryPage = new QuestionnaireVersionHistoryPage(page);
        await questionnaireVersionHistoryPage.expectHeading(PageHeadings.VIEW_VERSION_HISTORY_PAGE_HEADING);

        // Check toggle text when initially loaded
        await questionnaireVersionHistoryPage.expectAllVersionsToggleText('Show all versions');

        // Click to toggle and verify text changes
        await questionnaireVersionHistoryPage.clickShowAllVersions();
        await questionnaireVersionHistoryPage.expectAllVersionsToggleText('Hide all versions');

        await questionnaireVersionHistoryPage.clickHideAllVersions();
        await questionnaireVersionHistoryPage.expectAllVersionsToggleText('Show all versions');
    });


    test('Questionnaire version history changes after first publish', async ({page, request}) => {
        const {questionnaire} = await createQuestionnaire(request, token);
        await updateQuestionnaire(request, questionnaire.id, {slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}`}, token);

        await addContributor(request, questionnaire.id, 'user-1', token)
        const {question} = await createQuestion(request, questionnaire.id, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)

        await signIn(page, token);

        // Go to Edit a Questionnaire page and trigger publish flow
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaire.id);

        await designQuestionnairePage.publishQuestionnaire();
        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaire.id}.*/track`));

        // Open questionnaire version history
        await designQuestionnairePage.openVersionHistory();
        questionnaireVersionHistoryPage = new QuestionnaireVersionHistoryPage(page);
        await questionnaireVersionHistoryPage.expectHeading(PageHeadings.VIEW_VERSION_HISTORY_PAGE_HEADING);

        const lisQuestionnaireResponse = await listQuestionnaires(request, token);
        const list: any[] = lisQuestionnaireResponse.questionnaireGetBody
        expect(list.length).toBeGreaterThan(0);
        const firstQuestionnaire = list[0];

        // Validations level - 1
        await validateVersionHistoryLevel(
            questionnaireVersionHistoryPage,
            1,
            'Questionnaire published',
            firstQuestionnaire.createdBy,
            'The following are the list of changes since the last publish.'
        );

        // Validations level - 2
        await validateVersionHistoryLevel(
            questionnaireVersionHistoryPage,
            2,
            'Current draft changes',
            firstQuestionnaire.createdBy,
            'The following are the list of changes since the last publish.'
        );
    });

    test('Questionnaire version history draft changes validation after first publish', async ({page, request}) => {
        const {questionnaire} = await createQuestionnaire(request, token);
        await updateQuestionnaire(request, questionnaire.id, {slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}`}, token);

        await addContributor(request, questionnaire.id, 'user-1', token)
        const {question} = await createQuestion(request, questionnaire.id, token, 'Custom test questionnaire title', QuestionType.SingleSelect, undefined);
        
        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)

        await signIn(page, token);

        // Go to Edit a Questionnaire page and trigger publish flow
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaire.id);

        await designQuestionnairePage.publishQuestionnaire();
        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaire.id}.*/track`));

        // Navigate to edit question page
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaire.id, question.id);

        // Update question data
        const updatedContent = `Updated Question - ${Date.now()}`;
        const updatedHintText = `Updated hint text - ${Date.now()}`;

        await addQuestionPage.enterQuestionContent(updatedContent);
        await addQuestionPage.enterQuestionHintText(updatedHintText);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.SingleSelectLong);
        await addQuestionPage.clickSaveQuestion();

        // Verify the update via form data
        await addQuestionPage.clickBackLink();
        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
        await viewQuestionPage.markFinishedEditing(true);
        await viewQuestionPage.saveAndContinue();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.taskStatusAddEditQuestionsAnswers('Completed', 'Add or edit questions and answers');

        // Open questionnaire version history
        await designQuestionnairePage.openVersionHistory();
        questionnaireVersionHistoryPage = new QuestionnaireVersionHistoryPage(page);

        const lisQuestionnaireResponse = await listQuestionnaires(request, token);
        const list: any[] = lisQuestionnaireResponse.questionnaireGetBody
        expect(list.length).toBeGreaterThan(0);
        const firstQuestionnaire = list[0];

        // Validations level - 1
        await validateVersionHistoryLevel(
            questionnaireVersionHistoryPage,
            1,
            'Questionnaire published',
            firstQuestionnaire.createdBy,
            'The following are the list of changes since the last publish.'
        );

        // Validations level - 2
        await validateVersionHistoryLevel(
            questionnaireVersionHistoryPage,
            2,
            'Current draft changes',
            firstQuestionnaire.createdBy,
            'The following are the list of changes since the last publish.',
            true
        );
    });
    
    async function validateVersionHistoryLevel(
        questionnaireVersionHistoryPage: QuestionnaireVersionHistoryPage,
        level: number,
        expectedTitle: string,
        expectedUserEmail: string,
        expectedChangesText?: string,
        changesHaveListItems: boolean = false
    ) {
        // Validate version title
        const versionTitle = await questionnaireVersionHistoryPage.getVersionTitle(level);
        expect(versionTitle, `Version title should match for level ${level}`).toBe(expectedTitle);

        // Validate user email
        const versionUserEmail = await questionnaireVersionHistoryPage.getVersionUserEmail(level);
        expect(versionUserEmail, `User email should match for level ${level}`).toBe(expectedUserEmail);

        // Validate timestamp
        const versionTimestamp = await questionnaireVersionHistoryPage.getVersionTimestamp(level);
        expect(versionTimestamp, `Timestamp should not be empty for level ${level}`).toBeTruthy();

        // Click and validate show/hide changes
        await questionnaireVersionHistoryPage.clickShowChanges(level);
        await questionnaireVersionHistoryPage.expectToggleButtonText(level, 'Hide Changes');

        const viewChangesText = await questionnaireVersionHistoryPage.getChangesTextForLevel(level);
        if(changesHaveListItems)
        {
            await questionnaireVersionHistoryPage.expectVersionListHasItems(level);
        }else{
            expect(viewChangesText, `Changes text should match for level ${level}`).toBe(expectedChangesText);
        }
    }
});