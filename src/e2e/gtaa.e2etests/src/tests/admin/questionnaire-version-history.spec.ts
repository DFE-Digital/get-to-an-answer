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
import {
    goToAddQuestionPageByUrl,
    goToDesignQuestionnairePageByUrl, goToUpdateAnswerPageByUrl,
    goToUpdateQuestionPageByUrl,
    signIn
} from "../../helpers/admin-test-helper";
import {PublishQuestionnaireConfirmationPage} from "../../pages/admin/PublishQuestionnaireConfirmationPage";
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {QuestionnaireVersionHistoryPage} from "../../pages/admin/QuestionnaireVersionHistoryPage";
import {AddQuestionPage, QuestionRadioLabel} from "../../pages/admin/AddQuestionPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {DeleteQuestionConfirmationPage} from "../../pages/admin/DeleteQuestionConfirmationPage";

test.describe('Get to an answer questionnaire versions history', () => {
    let token: string;
    let versionTitle: string
    let versionUserEmail: string
    let versionTimestamp: string
    let viewChangesText: string

    let addQuestionPage: AddQuestionPage
    let addAnswerPage: AddAnswerPage
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

    test('Questionnaire version history draft changes presence after first publish', async ({page, request}) => {
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

    test('Questionnaire version history text changes validation after first publish - edit question', async ({page, request}) => {
        const {questionnaire} = await createQuestionnaire(request, token);
        await updateQuestionnaire(request, questionnaire.id, {slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}`}, token);

        await addContributor(request, questionnaire.id, 'user-1', token)
        const {question} = await createQuestion(request, questionnaire.id, token, 'Custom test question content', 
            QuestionType.SingleSelect, 'Custom test question hint text');

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

        await questionnaireVersionHistoryPage.clickShowChanges(2);

        // Expected text values
        const expectedList = [
            "the status was changed from 'Published' to 'Draft'",
            "'Completed' was added to the add questions and answers",
            `the content of question 1 was changed from 'Custom test question content' to ${updatedContent}`,
            `the description of question 1 was changed from 'Custom test question hint text' to ${updatedHintText}`,
            "the type of question 1 was changed from 'SingleSelect' to 'DropdownSelect'",
        ];
        await questionnaireVersionHistoryPage.expectChangeTexts(2, expectedList);
    });

    // CARE-1609 bug raise to remove a line from the validation
    test('Questionnaire version history text changes validation after first publish - add question answers', async ({page, request}) => {
        const {questionnaire} = await createQuestionnaire(request, token);
        await updateQuestionnaire(request, questionnaire.id, {slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}`}, token);
        
        await addContributor(request, questionnaire.id, 'user-1', token)
        const {question} = await createQuestion(request, questionnaire.id, token, 'Custom test question content',
            QuestionType.SingleSelect, 'Custom test question hint text');

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

        // Navigate to add question page
        addQuestionPage = await goToAddQuestionPageByUrl(page, questionnaire.id);
        await addQuestionPage.expectAddQuestionHeadingOnPage(PageHeadings.ADD_QUESTION_PAGE_HEADING);
        
        // Add question data
        const questionContent = `Test Question - ${Date.now()}`;
        const questionHintText = `Test Hint - ${Date.now()}`;

        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.enterQuestionHintText(questionHintText);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.SingleSelectLong);
        await addQuestionPage.clickSaveAndContinue();

        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setExternalLink(0, 'https://www.example.com');

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.setExternalLink(1, 'https://www.example.com');

        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
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

        await questionnaireVersionHistoryPage.clickShowChanges(2);

        // Expected text values
        const expectedList = [
            "the status was changed from 'Published' to 'Draft'",
            "'Completed' was added to the add questions and answers",
            `${questionContent} was added to the content of question 2`,
            `${questionHintText} was added to the description of question 2`,
            "'2' was added to the order of question 2",
            "'DropdownSelect' was added to the type of question 2",
            "'First Answer Option' was added to the content of answer 1 of question 2",
            "'This is the first answer hint' was added to the description of answer 1 of question 2",
            "'0' was added to the priority of answer 1 of question 2",
            "'ExternalLink' was added to the destination type of answer 1 of question 2",
            "'https://www.example.com' was added to the destination url of answer 1 of question 2",
            "'Second Answer Option' was added to the content of answer 2 of question 2",
            "'This is the second answer hint' was added to the description of answer 2 of question 2",
            "'0' was added to the priority of answer 2 of question 2",
            "'ExternalLink' was added to the destination type of answer 2 of question 2",
            "'https://www.example.com' was added to the destination url of answer 2 of question 2"
        ];
        await questionnaireVersionHistoryPage.expectChangeTexts(2, expectedList);
    });

    // CARE-1607 bug raised
    // test('Questionnaire version history text changes validation after first publish - delete question answers', async ({page, request}) => {
    //     const {questionnaire} = await createQuestionnaire(request, token);
    //     await updateQuestionnaire(request, questionnaire.id, {slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}`}, token);
    //
    //     await addContributor(request, questionnaire.id, 'user-1', token)
    //     const {question} = await createQuestion(request, questionnaire.id, token, 'Custom test question content',
    //         QuestionType.SingleSelect, 'Custom test question hint text');
    //
    //     await createSingleAnswer(request, {
    //         questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
    //         destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
    //     }, token)
    //
    //     await signIn(page, token);
    //
    //     // Go to Edit a Questionnaire page and trigger publish flow
    //     designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaire.id);
    //
    //     await designQuestionnairePage.publishQuestionnaire();
    //     const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
    //     await confirmPublishPage.expectTwoRadiosPresent();
    //     await confirmPublishPage.chooseYes();
    //     await confirmPublishPage.clickContinue();
    //
    //     // Should land back on the tracking page for this questionnaire
    //     await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaire.id}.*/track`));
    //
    //     // Delete an answer
    //     addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaire.id, question.id);
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //
    //     await addAnswerPage.removeOption(0);
    //     await addAnswerPage.clickSaveAnswersButton();
    //    
    //     // Delete the question
    //     addQuestionPage = await AddQuestionPage.create(page);
    //     await addQuestionPage.clickDeleteQuestion();
    //
    //     const deleteConfirmationPage = new DeleteQuestionConfirmationPage(page);
    //     await deleteConfirmationPage.validateOnPage();
    //     await deleteConfirmationPage.selectYes();
    //     await deleteConfirmationPage.clickContinue();
    //
    //     viewQuestionPage = new ViewQuestionPage(page);
    //     await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    //     await viewQuestionPage.assertQuestionDeletionSuccessBanner();
    //     await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    //     await viewQuestionPage.markFinishedEditing(true);
    //     await viewQuestionPage.saveAndContinue();
    //
    //     // Open questionnaire version history
    //     await designQuestionnairePage.openVersionHistory();
    //     questionnaireVersionHistoryPage = new QuestionnaireVersionHistoryPage(page);
    //
    //     const lisQuestionnaireResponse = await listQuestionnaires(request, token);
    //     const list: any[] = lisQuestionnaireResponse.questionnaireGetBody
    //     expect(list.length).toBeGreaterThan(0);
    //     const firstQuestionnaire = list[0];
    //
    //     await questionnaireVersionHistoryPage.clickShowChanges(2);
    //
    //     // Expected text values
    //     const expectedList = [
    //         "the status was changed from 'Published' to 'Draft'",
    //         "'Completed' was added to the add questions and answers",
    //         //`${questionContent} was added to the content of question 2`,
    //         //`${questionHintText} was added to the description of question 2`,
    //         "'2' was added to the order of question 2",
    //         "'0' was added to the status of question 2",
    //         "'DropdownSelect' was added to the type of question 2",
    //         "'First Answer Option' was added to the content of answer 1 of question 2",
    //         "'This is the first answer hint' was added to the description of answer 1 of question 2",
    //         "'0' was added to the priority of answer 1 of question 2",
    //         "'ExternalLink' was added to the destination type of answer 1 of question 2",
    //         "'https://www.example.com' was added to the destination url of answer 1 of question 2",
    //         "'Second Answer Option' was added to the content of answer 2 of question 2",
    //         "'This is the second answer hint' was added to the description of answer 2 of question 2",
    //         "'0' was added to the priority of answer 2 of question 2",
    //         "'ExternalLink' was added to the destination type of answer 2 of question 2",
    //         "'https://www.example.com' was added to the destination url of answer 2 of question 2"
    //     ];
    //     await questionnaireVersionHistoryPage.expectChangeTexts(2, expectedList);
    // });
    
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