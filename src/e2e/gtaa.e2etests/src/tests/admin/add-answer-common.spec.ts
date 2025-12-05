import {expect, test} from "@playwright/test";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {goToAddAnswerPageByUrl, signIn} from "../../helpers/admin-test-helper";
import {PageHeadings} from "../../constants/test-data-constants";
import {createQuestion} from "../../test-data-seeder/question-data";

test.describe('Get to an answer add an answer to a question', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;
    let question1Content: string;
    let question2Id: string;

    let addAnswerPage: AddAnswerPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        const q1Resp = await createQuestion(request, questionnaireId, token);
        question1Id = q1Resp.question.id;
        question1Content = q1Resp.question.content;

        const q2Resp = await createQuestion(request, questionnaireId, token);
        question2Id = q2Resp.question.id;
    });

    test('Validate presence of elements on add answer page upon landing', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);
        
        await addAnswerPage.asserPageElementsUponLanding(0, 2);
        await addAnswerPage.asserPageElementsUponLanding(1, 2);
    });

    test("Header section - H1 and questionnaire status", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.expectQuestionnaireStatusOnPage('Draft');
    });

    test('Clicking the back link takes to list questions page', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.clickBackLInk();
        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

    test('Remove button should remove answers option', async ({page}) => {
        let removeButtonsCount: number;
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        
        removeButtonsCount = await addAnswerPage.getRemoveButtonCount();
        expect(removeButtonsCount).toBe(2);
        await addAnswerPage.removeOption(1);
        removeButtonsCount = await addAnswerPage.getRemoveButtonCount();
        expect(removeButtonsCount).toBe(1);
        
        await addAnswerPage.clickAddAnotherOptionButton();
        await addAnswerPage.asserPageElementsUponLanding(0, 1);
    })

    test("Validate presence of elements on add another answer form", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'NextQuestion');

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.chooseDestination(1, 'NextQuestion');

        await addAnswerPage.clickAddAnotherOptionButton();
        await addAnswerPage.asserPageElementsUponLanding(2, 3);
    });
    
    //TBC, CARE-1579 bug raised to be covered later during accessibility testing
    // test('Accessible ids and aria-describedby for multiple options with hint', async ({page}) => {
    //     await signIn(page, token);
    //     addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);
    //
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //     await addAnswerPage.validateUniqueIdsForMultipleOptions(2);
    //
    //     await addAnswerPage.setOptionContent(0, 'First Answer Option');
    //     await addAnswerPage.setOptionContent(1, 'Second Answer Option');
    //    
    //     await addAnswerPage.clickAddAnotherOptionButton();
    //     await addAnswerPage.validateUniqueIdsForMultipleOptions(3);
    //
    //     await addAnswerPage.setOptionContent(2, 'Third Answer Option');
    //     await addAnswerPage.setOptionHint(0, 'First answer hint');
    //     await addAnswerPage.setOptionHint(1, 'Second answer hint');
    //     await addAnswerPage.setOptionHint(2, 'Third answer hint');
    //    
    //     // Verify aria-describedby includes hint ids (no errors present)
    //     await addAnswerPage.validateAriaDescribedByWithHintOnly(0);
    //     await addAnswerPage.validateAriaDescribedByWithHintOnly(1);
    //     await addAnswerPage.validateAriaDescribedByWithHintOnly(2);
    // })

    //TBC, CARE-1579 bug raised to be covered later during accessibility testing
    // test('Accessible ids and aria-describedby for multiple options with error', async ({page}) => {
    //     await signIn(page, token);
    //     addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);
    //
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //    
    //     await addAnswerPage.setOptionHint(0, 'First answer hint');
    //     await addAnswerPage.setOptionHint(1, 'Second answer hint');
    //    
    //     await addAnswerPage.clickSaveAndContinueButton();
    //     await addAnswerPage.validateUniqueIdsForMultipleOptions(2);
    //
    //     // Verify aria-describedby includes both hint id and error id when error is present
    //     await addAnswerPage.validateAriaDescribedByWithHintAndError(0);
    //     await addAnswerPage.validateAriaDescribedByWithHintAndError(1);
    // })
});