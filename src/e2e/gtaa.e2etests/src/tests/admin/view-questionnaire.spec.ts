
import {test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {localSignIn, goToAddQuestionnairePage} from "../../helpers/admin-test-helper";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";

test.describe('Get to an answer views questionnaire', () => {
    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;

    test.beforeEach(async ({page}) => {
        viewQuestionnairePage = await localSignIn(page);
    });

    test('Validate presence of elements on view questionnaire page', async ({page}) => {
        await viewQuestionnairePage.assertPageElements();
    });

    test("Header section - H1 and paragraph presence", async () => {
        await viewQuestionnairePage.verifyHelpUserHeadingVisible();
        await viewQuestionnairePage.verifyHelpUserDescriptionVisible();
    });

    test("Create questionnaire CTA navigates to Add page", async ({page}) => {
        await viewQuestionnairePage.clickCreateNewQuestionnaire();

        addQuestionnairePage = await AddQuestionnairePage.create(page);
        //await addQuestionnairePage.expectUrlOnPage();
        await addQuestionnairePage.verifyLabelAndHintPresent();
    });

    test("Questionnaires table - columns, title link navigates", async ({page}) => {
        await viewQuestionnairePage.table.verifyHeaders();
        await viewQuestionnairePage.table.verifyFirstTitleIsLink();

        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();
        editQuestionnairePage = await EditQuestionnairePage.create(page);
        //await editQuestionnairePage.expectUrlOnPage();
        await editQuestionnairePage.validateHeadingAndStatus();
    });

    test("Questionnaires table - status as draft", async ({page}) => {
        await viewQuestionnairePage.table.expectRowPresentByName('pass the unique questionnaire title from api');

        // based on unique title this will return the status as draft 
        await viewQuestionnairePage.table.getStatus('pass the unique questionnaire title from api');
    });

    //AI generated tests
    // test.describe('Header section', () => {
    //     test('should display H1 heading "Help users to get to an answer"', async () => {
    //         // Then I see an H1 "Help users to get to an answer" - validate presence only no text validation
    //         await viewQuestionnairePage.verifyHelpUserHeadingVisible();
    //     });
    //
    //     test('should display explanatory paragraph in header', async () => {
    //         // And a paragraph explaining the service - validate presence only no text validation
    //         await viewQuestionnairePage.verifyHelpUserDescriptionVisible();
    //     });
    // });
    //
    // test.describe('Create questionnaire CTA', () => {
    //     test('should display Create a questionnaire start button', async () => {
    //         // Then I see a "Create a questionnaire" start button
    //         await viewQuestionnairePage.verifyCreateButtonVisible();
    //     });
    //
    //     test('clicking Create a questionnaire button should navigate to AddQuestionnaire page', async ({page}) => {
    //         // And clicking it navigates to AddQuestionnaire page
    //         await viewQuestionnairePage.clickCreateNewQuestionnaire();
    //         addQuestionnairePage = new AddQuestionnairePage(page, 'create');
    //         await addQuestionnairePage.assertPageElements();
    //     });
    // });
    //
    // test.describe('Questionnaires table', () => {
    //     test('should display table with correct columns', async () => {
    //         // TODO: questionnaires will be added later by APIs
    //         // Then I see a table with columns Name, Created by, and Status
    //         await viewQuestionnairePage.table.verifyHeaders();
    //     });
    //
    //     test('each questionnaire title should be a link to its track page', async () => {
    //         // TODO: questionnaires will be added later by APIs
    //         // And each questionnaire title is a link to its track page
    //         await viewQuestionnairePage.table.verifyFirstTitleIsLink();
    //     });
    //
    //     test('should display status as GOV.UK tag (Draft, Published, Deleted, or Archived)', async () => {
    //         // TODO: questionnaires will be added later by APIs
    //         // And the status is displayed as a Welcome to GOV.UK tag
    //         // (4 statuses: draft, published, deleted or archived)
    //         const rowCount = await viewQuestionnairePage.table.getRowCount();
    //         if (rowCount > 0) {
    //             // Status verification would be done once questionnaires are populated via API
    //             // Example: await viewQuestionnairePage.table.getStatus(questionnaireName);
    //         }
    //     });
    // });
});
