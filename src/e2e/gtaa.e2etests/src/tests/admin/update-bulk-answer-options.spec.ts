import {test, expect} from "@playwright/test";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {
    goToAddQuestionnairePage, goToUpdateAnswerPageByUrl,
    goToUpdateQuestionPageByUrl,
    goToViewQuestionsPageByUrl,
    signIn
} from '../../helpers/admin-test-helper';
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {AnswerDestinationType, ErrorMessages} from "../../constants/test-data-constants";
import {CloneQuestionnairePage} from "../../pages/admin/CloneQuestionnairePage";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddQuestionPage} from "../../pages/admin/AddQuestionPage";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {AddBulkAnswerOptionsPage} from "../../pages/admin/AddBulkAnswerOptionsPage";
import {createContent} from "../../test-data-seeder/content-data";

test.describe('Get to an answer Edit bulk answers options to question', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;
    let question2Id: string;
    let question1Content: string;
    let question2Content: string;
    let questionnaireTitle: string;
    let cloneQuestionnairePage: CloneQuestionnairePage;
    let viewQuestionnairePage: ViewQuestionnairePage;
    let designQuestionnairePage: DesignQuestionnairePage;
    let viewQuestionsPage: ViewQuestionPage;
    let addAnswerPage: AddAnswerPage;
    let bulkAddAnswersPage: AddBulkAnswerOptionsPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
        questionnaireTitle = questionnaire.title;

        const {question: question1} = await createQuestion(request, questionnaire.id, token);
        expect(question1.id).toBeDefined();
        question1Id = question1.id;
        question1Content = question1.content;

        const {question: question2} = await createQuestion(request, questionnaire.id, token);
        expect(question2.id).toBeDefined();
        question2Id = question2.id;
        question2Content = question2.content;
    });

    //TODO: Bug raised CARE-1651
    test('Update bulk answers and validate updated answers - specific question', async ({
                                                                                            request,
                                                                                            page
                                                                                        }) => {
        const expectedBulkOptions = generateUniqueOptions(2);

        const {answer: answer1} = await createSingleAnswer(request, {
            questionId: question1Id,
            questionnaireId,
            content: expectedBulkOptions[0]
        }, token)

        const {answer: answer2} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[1],
            destinationType: AnswerDestinationType.Question,
            destinationQuestionId: question2Id
        }, token)

        viewQuestionnairePage = await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        await addAnswerPage.validateAllOptionContents(expectedBulkOptions);

        await addAnswerPage.expectDestinationRadioSelected(0, 'NextQuestion');

        //await addAnswerPage.expectDestinationRadioSelected(1, 'SpecificQuestion'); 
        //await addAnswerPage.expectSpecificQuestionDropdownSelected(1, question2Content);

        await addAnswerPage.clickEnterAllOptionsButton();

        bulkAddAnswersPage = await AddBulkAnswerOptionsPage.create(page);
        await bulkAddAnswersPage.expectOnPage();

        await bulkAddAnswersPage.assertAllOptionNumberLabelsInOrder(2);
        await bulkAddAnswersPage.validateAllOptionContents(expectedBulkOptions);

        await bulkAddAnswersPage.moveBottomOptionToTop();
        await bulkAddAnswersPage.clickContinue();

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        const reorderedOptions = moveToTopInExpectedBulkOptions(expectedBulkOptions);
        await addAnswerPage.validateAllOptionContents(reorderedOptions);

        //await addAnswerPage.expectDestinationRadioSelected(0, 'SpecificQuestion'); 
        //await addAnswerPage.expectSpecificQuestionDropdownSelected(0, question2Content);

        await addAnswerPage.expectDestinationRadioSelected(1, 'NextQuestion');
    })
    
    test('Update bulk answers and validate updated answers - External link', async ({
                                                                                            request,
                                                                                            page
                                                                                        }) => {
        const expectedBulkOptions = generateUniqueOptions(2);
        const destinationUrl = 'https://example.com';

        const {answer: answer1} = await createSingleAnswer(request, {
            questionId: question1Id,
            questionnaireId,
            content: expectedBulkOptions[0]
        }, token)

        const {answer: answer2} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[1],
            destinationType: AnswerDestinationType.ExternalLink,
            destinationUrl
        }, token)

        viewQuestionnairePage = await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        await addAnswerPage.validateAllOptionContents(expectedBulkOptions);

        await addAnswerPage.expectDestinationRadioSelected(0, 'NextQuestion');
        
        await addAnswerPage.expectDestinationRadioSelected(1, 'ExternalResultsPage'); 
        await addAnswerPage.expectExternalLinkInputValue(1, destinationUrl);

        await addAnswerPage.clickEnterAllOptionsButton();

        bulkAddAnswersPage = await AddBulkAnswerOptionsPage.create(page);
        await bulkAddAnswersPage.expectOnPage();

        await bulkAddAnswersPage.assertAllOptionNumberLabelsInOrder(2);
        await bulkAddAnswersPage.validateAllOptionContents(expectedBulkOptions);

        await bulkAddAnswersPage.moveBottomOptionToTop();
        await bulkAddAnswersPage.clickContinue();

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        const reorderedOptions = moveToTopInExpectedBulkOptions(expectedBulkOptions);
        await addAnswerPage.validateAllOptionContents(reorderedOptions);

        await addAnswerPage.expectDestinationRadioSelected(0, 'ExternalResultsPage');
        await addAnswerPage.expectExternalLinkInputValue(0, destinationUrl);

        await addAnswerPage.expectDestinationRadioSelected(1, 'NextQuestion');
    })

    test('Update bulk answers and validate updated answers - Internal results page', async ({
                                                                                        request,
                                                                                        page
                                                                                    }) => {
        const expectedBulkOptions = generateUniqueOptions(2);
        const destinationUrl = 'https://example.com';

        const {answer: answer1} = await createSingleAnswer(request, {
            questionId: question1Id,
            questionnaireId,
            content: expectedBulkOptions[0]
        }, token)

        const contentTitle = 'test-content';
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title: contentTitle,
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)
        
        const {answer: answer2} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[1],
            destinationType: AnswerDestinationType.CustomContent,
            destinationContentId: apiContentResponse.content.id
        }, token)
        
        viewQuestionnairePage = await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        await addAnswerPage.validateAllOptionContents(expectedBulkOptions);

        await addAnswerPage.expectDestinationRadioSelected(0, 'NextQuestion');

        await addAnswerPage.expectDestinationRadioSelected(1, 'InternalResultsPage');
        await addAnswerPage.expectResultsPageDropdownSelected(1, contentTitle);

        await addAnswerPage.clickEnterAllOptionsButton();

        bulkAddAnswersPage = await AddBulkAnswerOptionsPage.create(page);
        await bulkAddAnswersPage.expectOnPage();

        await bulkAddAnswersPage.assertAllOptionNumberLabelsInOrder(2);
        await bulkAddAnswersPage.validateAllOptionContents(expectedBulkOptions);

        await bulkAddAnswersPage.moveBottomOptionToTop();
        await bulkAddAnswersPage.clickContinue();

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        const reorderedOptions = moveToTopInExpectedBulkOptions(expectedBulkOptions);
        await addAnswerPage.validateAllOptionContents(reorderedOptions);

        await addAnswerPage.expectDestinationRadioSelected(0, 'InternalResultsPage');
        await addAnswerPage.expectResultsPageDropdownSelected(0, contentTitle);

        await addAnswerPage.expectDestinationRadioSelected(1, 'NextQuestion');
    })

    test('Update bulk answers and validate updated answers for 3 options', async ({
                                                                                        request,
                                                                                        page
                                                                                    }) => {
        const expectedBulkOptions = generateUniqueOptions(3);
        const destinationUrl = 'https://example.com';

        const {answer: answer1} = await createSingleAnswer(request, {
            questionId: question1Id,
            questionnaireId,
            content: expectedBulkOptions[0]
        }, token)

        const contentTitle = 'test-content';
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title: contentTitle,
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)
        
        const {answer: answer2} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[1],
            destinationType: AnswerDestinationType.CustomContent,
            destinationContentId: apiContentResponse.content.id
        }, token)

        const {answer: answer3} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[2],
            destinationType: AnswerDestinationType.ExternalLink,
            destinationUrl
        }, token)

        viewQuestionnairePage = await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        await addAnswerPage.validateAllOptionContents(expectedBulkOptions);

        await addAnswerPage.expectDestinationRadioSelected(0, 'NextQuestion');

        await addAnswerPage.expectDestinationRadioSelected(1, 'InternalResultsPage');
        await addAnswerPage.expectResultsPageDropdownSelected(1);

        await addAnswerPage.expectDestinationRadioSelected(2, 'ExternalResultsPage');
        await addAnswerPage.expectExternalLinkInputValue(2, destinationUrl);

        await addAnswerPage.clickEnterAllOptionsButton();

        bulkAddAnswersPage = await AddBulkAnswerOptionsPage.create(page);
        await bulkAddAnswersPage.expectOnPage();

        await bulkAddAnswersPage.assertAllOptionNumberLabelsInOrder(3);
        await bulkAddAnswersPage.validateAllOptionContents(expectedBulkOptions);

        await bulkAddAnswersPage.moveBottomOptionToTop();
        await bulkAddAnswersPage.clickContinue();

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        const reorderedOptions = moveToTopInExpectedBulkOptions(expectedBulkOptions);
        await addAnswerPage.validateAllOptionContents(reorderedOptions);

        await addAnswerPage.expectDestinationRadioSelected(0, 'ExternalResultsPage');
        await addAnswerPage.expectExternalLinkInputValue(0, destinationUrl);

        await addAnswerPage.expectDestinationRadioSelected(1, 'NextQuestion');

        await addAnswerPage.expectDestinationRadioSelected(2, 'InternalResultsPage');
        await addAnswerPage.expectResultsPageDropdownSelected(2);
    })

    test('Update bulk answers and validate updated answers after removing an option', async ({
                                                                                      request,
                                                                                      page
                                                                                  }) => {
        const expectedBulkOptions = generateUniqueOptions(3);
        const destinationUrl = 'https://example.com';

        const {answer: answer1} = await createSingleAnswer(request, {
            questionId: question1Id,
            questionnaireId,
            content: expectedBulkOptions[0]
        }, token)

        const contentTitle = 'test-content';
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title: contentTitle,
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)

        const {answer: answer2} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[1],
            destinationType: AnswerDestinationType.CustomContent,
            destinationContentId: apiContentResponse.content.id
        }, token)

        const {answer: answer3} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[2],
            destinationType: AnswerDestinationType.ExternalLink,
            destinationUrl
        }, token)

        viewQuestionnairePage = await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        await addAnswerPage.validateAllOptionContents(expectedBulkOptions);

        await addAnswerPage.expectDestinationRadioSelected(0, 'NextQuestion');

        await addAnswerPage.expectDestinationRadioSelected(1, 'InternalResultsPage');
        await addAnswerPage.expectResultsPageDropdownSelected(1);

        await addAnswerPage.expectDestinationRadioSelected(2, 'ExternalResultsPage');
        await addAnswerPage.expectExternalLinkInputValue(2, destinationUrl);

        await addAnswerPage.clickEnterAllOptionsButton();

        bulkAddAnswersPage = await AddBulkAnswerOptionsPage.create(page);
        await bulkAddAnswersPage.expectOnPage();

        await bulkAddAnswersPage.assertAllOptionNumberLabelsInOrder(3);
        await bulkAddAnswersPage.validateAllOptionContents(expectedBulkOptions);

        await bulkAddAnswersPage.moveBottomOptionToTop();
        await bulkAddAnswersPage.removeRandomEntry();
        const currentOptions = await bulkAddAnswersPage.getCurrentBulkOptions();
        await bulkAddAnswersPage.clickContinue();

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        await addAnswerPage.validateAllOptionContents(currentOptions);
    })

    test('Update bulk answers and validate updated answers after adding a new option', async ({
                                                                                                 request,
                                                                                                 page
                                                                                             }) => {
        const expectedBulkOptions = generateUniqueOptions(3);
        const destinationUrl = 'https://example.com';

        const {answer: answer1} = await createSingleAnswer(request, {
            questionId: question1Id,
            questionnaireId,
            content: expectedBulkOptions[0]
        }, token)

        const contentTitle = 'test-content';
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title: contentTitle,
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)

        const {answer: answer2} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[1],
            destinationType: AnswerDestinationType.CustomContent,
            destinationContentId: apiContentResponse.content.id
        }, token)

        const {answer: answer3} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[2],
            destinationType: AnswerDestinationType.ExternalLink,
            destinationUrl
        }, token)

        viewQuestionnairePage = await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        await addAnswerPage.validateAllOptionContents(expectedBulkOptions);

        await addAnswerPage.expectDestinationRadioSelected(0, 'NextQuestion');

        await addAnswerPage.expectDestinationRadioSelected(1, 'InternalResultsPage');
        await addAnswerPage.expectResultsPageDropdownSelected(1);

        await addAnswerPage.expectDestinationRadioSelected(2, 'ExternalResultsPage');
        await addAnswerPage.expectExternalLinkInputValue(2, destinationUrl);

        await addAnswerPage.clickEnterAllOptionsButton();

        bulkAddAnswersPage = await AddBulkAnswerOptionsPage.create(page);
        await bulkAddAnswersPage.expectOnPage();

        await bulkAddAnswersPage.assertAllOptionNumberLabelsInOrder(3);
        await bulkAddAnswersPage.validateAllOptionContents(expectedBulkOptions);
        
        await bulkAddAnswersPage.addRandomEntryAtBottom()
        const currentOptions = await bulkAddAnswersPage.getCurrentBulkOptions();
        await bulkAddAnswersPage.clickContinue();

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        await addAnswerPage.validateAllOptionContents(currentOptions);
    })

    //TODO: CARE-1654 bug reported
    test('Update bulk answers and validate error message after adding a duplicate option', async ({
                                                                                                  request,
                                                                                                  page, browserName
                                                                                              }) => {
        const expectedBulkOptions = generateUniqueOptions(3);
        const destinationUrl = 'https://example.com';

        const {answer: answer1} = await createSingleAnswer(request, {
            questionId: question1Id,
            questionnaireId,
            content: expectedBulkOptions[0]
        }, token)

        const contentTitle = 'test-content';
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title: contentTitle,
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)

        const {answer: answer2} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[1],
            destinationType: AnswerDestinationType.CustomContent,
            destinationContentId: apiContentResponse.content.id
        }, token)

        const {answer: answer3} = await createSingleAnswer(request, {
            questionnaireId,
            questionId: question1Id,
            content: expectedBulkOptions[2],
            destinationType: AnswerDestinationType.ExternalLink,
            destinationUrl
        }, token)

        viewQuestionnairePage = await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        await addAnswerPage.validateAllOptionContents(expectedBulkOptions);

        await addAnswerPage.expectDestinationRadioSelected(0, 'NextQuestion');

        await addAnswerPage.expectDestinationRadioSelected(1, 'InternalResultsPage');
        await addAnswerPage.expectResultsPageDropdownSelected(1);

        await addAnswerPage.expectDestinationRadioSelected(2, 'ExternalResultsPage');
        await addAnswerPage.expectExternalLinkInputValue(2, destinationUrl);

        await addAnswerPage.clickEnterAllOptionsButton();

        bulkAddAnswersPage = await AddBulkAnswerOptionsPage.create(page);
        await bulkAddAnswersPage.expectOnPage();

        await bulkAddAnswersPage.assertAllOptionNumberLabelsInOrder(3);
        await bulkAddAnswersPage.validateAllOptionContents(expectedBulkOptions);

        await bulkAddAnswersPage.addDuplicateEntryAtBottom();
        const currentOptions = await bulkAddAnswersPage.getCurrentBulkOptions();
        await bulkAddAnswersPage.clickContinue();

        //await bulkAddAnswersPage.validateDuplicateEntriesError(browserName);
    })

    function generateUniqueOptions(count: number, prefix: string = 'Answer', separator: string = ' - '): string[] {
        const expectedBulkOptions: string[] = [];

        for (let i = 1; i <= count; i++) {
            const uniqueContent = `${prefix}${i}Content${separator}${Date.now()}-${Math.random().toString(36).substring(2, 7)}`;
            expectedBulkOptions.push(uniqueContent);
        }
        return expectedBulkOptions;
    }

    function moveToTopInExpectedBulkOptions(options: string[]): string[] {
        if (options.length < 2) {
            return options;
        }

        const [first, ...rest] = options.reverse();
        return [first, ...rest.reverse()];
    }
})