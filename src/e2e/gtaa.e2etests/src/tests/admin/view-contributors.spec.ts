// add-edit-contributors.spec.ts
import { expect, test } from '@playwright/test';
import { JwtHelper } from '../../helpers/JwtHelper';
import { signIn, goToQuestionnaireContributorsPageByUrl } from '../../helpers/admin-test-helper';
import {addContributor, createQuestionnaire} from '../../test-data-seeder/questionnaire-data';
import { expect200HttpStatusCode } from '../../helpers/api-assertions-helper';
import { ViewContributorPage } from '../../pages/admin/ViewContributorPage';
import { AddContributorPage } from '../../pages/admin/AddContributorPage';
import { RemoveContributorConfirmationPage } from '../../pages/admin/RemoveContributorConfirmationPage';
import { DesignQuestionnairePage } from '../../pages/admin/DesignQuestionnairePage';
import {ErrorMessages, PageHeadings} from "../../constants/test-data-constants";

test.describe('Manage questionnaire contributors', () => {
    let token: string;
    let questionnaireId: string;

    let contributorsPage: ViewContributorPage;
    let addContributorPage: AddContributorPage;
    let confirmRemoveContributorPage: RemoveContributorConfirmationPage;
    let designQuestionnairePage: DesignQuestionnairePage;

    test.beforeEach(async ({ page, request }) => {
        token = JwtHelper.NoRecordsToken();

        const { questionnaire, questionnairePostResponse } = await createQuestionnaire(request, token);
        expect200HttpStatusCode(questionnairePostResponse, 201);

        questionnaireId = questionnaire.id;

        await signIn(page, token);
        contributorsPage = await goToQuestionnaireContributorsPageByUrl(page, questionnaireId);
        await contributorsPage.waitForPageLoad();
    });

    test('Contributors page – presence of elements', async () => {
        await contributorsPage.assertPageElements();
    });

    test('Back link navigates to edit questionnaire page', async ({ page }) => {
        await contributorsPage.clickBackLink();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING);
    });

    test('Add person button navigates to Add contributor page', async ({ page }) => {
        await contributorsPage.clickAddPerson();

        addContributorPage = await AddContributorPage.create(page);
        await addContributorPage.expectBasicStructure();
    });

    test('Contributors table lists at least one contributor with name and email', async () => {
        const tableRegion = contributorsPage['contributorsTableRegion'];
        await expect(tableRegion).toBeVisible();

        const rows = tableRegion.locator('tbody.govuk-table__body tr.govuk-table__row');
        const rowCount = await rows.count();
        expect(rowCount).toBeGreaterThan(0);

        for (let i = 0; i < rowCount; i++) {
            const row = rows.nth(i);
            const nameCell = row.locator('td').nth(0);
            const emailCell = row.locator('td').nth(1);

            await expect(nameCell, `❌ Name cell missing on row ${i + 1}`).toBeVisible();
            await expect(emailCell, `❌ Email cell missing on row ${i + 1}`).toBeVisible();

            const emailText = (await emailCell.textContent())?.trim() ?? '';
            expect(emailText).not.toBe('');
            expect(emailText).toContain('@');
        }
    });

    test('Minimum contributors constraint (enabled vs disabled delete/remove buttons)', async () => {
        await contributorsPage.expectMinimumContributorsConstraint();
    });

    test('Add contributor – structure and validation error display', async ({ page }) => {
        await contributorsPage.clickAddPerson();

        addContributorPage = await AddContributorPage.create(page);
        await addContributorPage.expectBasicStructure();

        await addContributorPage.fillEmail(''); // invalid
        await addContributorPage.clickSaveAndContinue();

        await addContributorPage.expectValidationErrorVisible();
    });

    test('Add contributor – successful email submission navigates back to contributors list', async ({ page }) => {
        await contributorsPage.clickAddPerson();

        addContributorPage = await AddContributorPage.create(page);
        await addContributorPage.expectBasicStructure();

        const email = `auto-contrib-${Date.now()}@education.gov.uk`;
        await addContributorPage.fillEmail(email);
        await addContributorPage.clickSaveAndContinue();

        contributorsPage = await ViewContributorPage.create(page);
        await contributorsPage.assertPageElements();
        await contributorsPage.expectContributorListed('', email); // name may be empty/derived
    });

    test('Notification banner is visible after successful change', async ({ page }) => {
        // Simulate a change via add contributor (this should set "just updated" state server-side)
        await contributorsPage.clickAddPerson();

        addContributorPage = await AddContributorPage.create(page);
        const email = `auto-contrib-${Date.now()}@education.gov.uk`;
        await addContributorPage.fillEmail(email);
        await addContributorPage.clickSaveAndContinue();

        contributorsPage = await ViewContributorPage.create(page);
        await contributorsPage.expectNotificationBannerVisible();
    });

    test('Confirm remove contributor page – presence of elements and radios', async ({ request, page }) => {
        // confirm the remove button is disabled with only one contributor
        await contributorsPage.table.expectedRemoveContributorButtonsDisabled();
        
        const user2Guid = "00000000-0000-0000-0000-000000000002";
        await addContributor(request, questionnaireId, user2Guid, token);
        await page.reload();

        // confirm the remove buttons are still disabled with only two contributors
        await contributorsPage.table.expectedRemoveContributorButtonsDisabled();

        const user3Guid = "00000000-0000-0000-0000-000000000003";
        await addContributor(request, questionnaireId, user3Guid, token);
        await page.reload();
        
        // confirm the remove buttons are now enabled with three contributors
        await contributorsPage.table.expectedRemoveContributorButtonsEnabled()
        
        const secondRow = contributorsPage.table.rowByIndex(2);
        const deleteButtonOrLink = secondRow.locator('#remove-contributor-2');

        await expect(deleteButtonOrLink).toBeVisible();
        await Promise.all([
            page.waitForLoadState('networkidle'),
            deleteButtonOrLink.click(),
        ]);

        confirmRemoveContributorPage = await RemoveContributorConfirmationPage.create(page);
        await confirmRemoveContributorPage.assertPageElements();
        await confirmRemoveContributorPage.expectYesNoRadiosWork();
    });

    test('Confirm remove contributor – choosing No keeps contributor and returns to list', async ({ request, page }) => {
        const user2Guid = "00000000-0000-0000-0000-000000000002";
        await addContributor(request, questionnaireId, user2Guid, token);

        const user3Guid = "00000000-0000-0000-0000-000000000003";
        await addContributor(request, questionnaireId, user3Guid, token);
        
        await page.reload();

        const secondRow = contributorsPage.table.rowByIndex(2);
        const emailCell = secondRow.locator('td').nth(1);
        const email = (await emailCell.textContent())?.trim() ?? '';

        // confirm the remove buttons are now enabled with three contributors
        await contributorsPage.table.expectedRemoveContributorButtonsEnabled()

        const deleteButtonOrLink = secondRow.locator('#remove-contributor-2');
        await Promise.all([
            page.waitForLoadState('networkidle'),
            deleteButtonOrLink.click(),
        ]);

        confirmRemoveContributorPage = await RemoveContributorConfirmationPage.create(page);
        await confirmRemoveContributorPage.chooseRemoveNo();
        await confirmRemoveContributorPage.clickContinue();

        contributorsPage = await ViewContributorPage.create(page);
        await contributorsPage.expectContributorListed('', email);
    });

    test('Confirm remove contributor – choosing Yes removes contributor and returns to list', async ({ request, page }) => {
        const user2Guid = "00000000-0000-0000-0000-000000000002";
        await addContributor(request, questionnaireId, user2Guid, token);

        const user3Guid = "00000000-0000-0000-0000-000000000003";
        await addContributor(request, questionnaireId, user3Guid, token);

        await page.reload();

        const secondRow = contributorsPage.table.rowByIndex(2);
        const emailCell = secondRow.locator('td').nth(1);
        const email = (await emailCell.textContent())?.trim() ?? '';
        const deleteButtonOrLink = secondRow.locator('#remove-contributor-2');

        await Promise.all([
            page.waitForLoadState('networkidle'),
            deleteButtonOrLink.click(),
        ]);

        confirmRemoveContributorPage = await RemoveContributorConfirmationPage.create(page);
        await confirmRemoveContributorPage.chooseRemoveYes();
        await confirmRemoveContributorPage.clickContinue();

        contributorsPage = await ViewContributorPage.create(page);

        expect(await contributorsPage.table.hasPerson(email)).toBeFalsy();
    });
    
    // collaboration test, two browsers, 2 tokens (or users), create and design a questionnaire, 
    // then another user reviews preview, then the user that didn't create the questionnaire, 
    // but is a contributor, publishes it. Then the another user is added as a contributor to the questionnaire.
    // They update and publish, then remove the initial creator.
});