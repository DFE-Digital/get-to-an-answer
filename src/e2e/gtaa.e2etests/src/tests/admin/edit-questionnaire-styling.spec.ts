import { expect, test } from '@playwright/test';
import { JwtHelper } from '../../helpers/JwtHelper';
import { signIn } from '../../helpers/admin-test-helper';
import { createQuestionnaire } from '../../test-data-seeder/questionnaire-data';
import { QuestionnaireStylingPage } from '../../pages/admin/QuestionnaireStylingPage';
import { DesignQuestionnairePage } from '../../pages/admin/DesignQuestionnairePage';

test.describe('Questionnaire custom styling', () => {
    let token: string;
    let questionnaireId: string;
    let stylingPage: QuestionnaireStylingPage;

    test.beforeEach(async ({ page, request }) => {
        token = JwtHelper.NoRecordsToken();

        // Seed a questionnaire and capture its id
        const { questionnaire } = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        // Sign in and navigate to track page
        await signIn(page, token);
        await page.goto(`/admin/questionnaires/${questionnaireId}/track`);

        // Open the "Customise styling" task
        await page.getByRole('link', { name: 'Customise styling' }).click();

        stylingPage = new QuestionnaireStylingPage(page);
        await stylingPage.expectOnStylingPage();
    });

    // ===== Happy paths =====

    test('Back link takes user to track questionnaire page', async ({ page }) => {
        await stylingPage.clickBackLink();

        await expect(page).toHaveURL(
            new RegExp(`/admin/questionnaires/${questionnaireId}.*?/track`)
        );
        await expect(page.locator('#track-questionnaire-page-heading')).toBeVisible();
    });

    test('Successfully saving customised styling returns to track with success banner', async ({ page }) => {
        await stylingPage.fillValidStylingAndSubmit({
            textColor: '#111111',
            backgroundColor: '#222222',
            primaryButtonColor: '#333333',
            secondaryButtonColor: '#444444',
            stateColor: '#555555',
            errorMessageColor: '#666666'
        });

        // Back on track page
        await expect(page).toHaveURL(
            new RegExp(`/admin/questionnaires/${questionnaireId}.*?/track`)
        );

        const editQuestionnairePage = new DesignQuestionnairePage(page);
        await editQuestionnairePage.expectSuccessBannerVisible();
        await editQuestionnairePage.assertUpdatedStylingSuccessBanner();
    });

    test('Reset to DfE styling redirects to track with reset-styling banner', async ({ page }) => {
        await stylingPage.clickResetStyling();

        await expect(page).toHaveURL(
            new RegExp(`/admin/questionnaires/${questionnaireId}.*?/track`)
        );

        const editQuestionnairePage = new DesignQuestionnairePage(page);
        await editQuestionnairePage.expectSuccessBannerVisible();
        await editQuestionnairePage.assertResetStylingSuccessBanner();
    });

    // ===== Validation unhappy paths =====

    test('Submitting with all fields empty and no accessibility agreement shows validation error', async ({ page }) => {
        // All inputs left as-is (empty by default), do NOT tick checkbox
        await stylingPage.saveAndContinue();

        // Still on styling page
        await stylingPage.expectOnStylingPage();

        // Error summary is visible
        const errorSummary = page.locator('.govuk-error-summary');
        await expect(errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(errorSummary, '❌ Error summary should be an alert').toHaveAttribute('role', 'alert');

        // Accessibility checkbox has inline error
        const accessibilityError = page.locator('#forms-name-input-name-error');
        await expect(accessibilityError, '❌ Accessibility agreement inline error missing').toBeVisible();

        // Checkbox should reference the error via aria-describedby
        const checkbox = page.locator('#UpdateRequest-IsAccessibilityAgreementAccepted');
        const ariaDescribedBy = await checkbox.getAttribute('aria-describedby');
        expect(ariaDescribedBy ?? '', '❌ aria-describedby must reference error element')
            .toContain('isaccessibilityagreementaccepted-error');
    });

    test('Submitting with colours filled but without accessibility agreement shows validation error', async ({ page }) => {
        // Fill all colour fields with valid looking hex codes
        await stylingPage.setTextColor('#111111');
        await stylingPage.setBackgroundColor('#222222');
        await stylingPage.setPrimaryButtonColor('#333333');
        await stylingPage.setSecondaryButtonColor('#444444');
        await stylingPage.setStateColor('#555555');
        await stylingPage.setErrorMessageColor('#666666');

        // Do NOT tick the accessibility agreement checkbox
        await stylingPage.saveAndContinue();

        // Still on styling page with an error summary
        await stylingPage.expectOnStylingPage();

        const errorSummary = page.locator('.govuk-error-summary');
        await expect(errorSummary, '❌ Error summary missing').toBeVisible();

        // Accessibility agreement should still be the field showing an error
        const accessibilityError = page.locator('#forms-name-input-name-error');
        await expect(accessibilityError, '❌ Accessibility agreement inline error missing').toBeVisible();
    });

    test('Submitting with clearly invalid colour values shows colour field validation errors', async ({ page }) => {
        // Intentionally invalid values (not hex codes)
        await stylingPage.setTextColor('not-a-hex');
        await stylingPage.setBackgroundColor('12345');
        await stylingPage.setPrimaryButtonColor('red');
        await stylingPage.setSecondaryButtonColor('blue');
        await stylingPage.setStateColor('!!invalid!!');
        await stylingPage.setErrorMessageColor('1234567');

        // Accept accessibility agreement so only colour validation should fail
        await stylingPage.acceptAccessibilityAgreement();
        await stylingPage.saveAndContinue();

        // Still on styling page
        await stylingPage.expectOnStylingPage();

        const errorSummary = page.locator('.govuk-error-summary');
        await expect(errorSummary, '❌ Error summary missing for invalid colours').toBeVisible();

        // Check that at least one error summary link points to the text colour input
        const textColorErrorLink = errorSummary.locator('a[href="#UpdateRequest-TextColor"]');
        await expect(textColorErrorLink, '❌ Error summary link for text colour missing').toBeVisible();

        // Inline error containers for at least the text colour should be visible
        await expect(
            page.locator('#text-color-error'),
            '❌ Inline error for text colour missing'
        ).toBeVisible();
    });

    // ===== Other unhappy paths =====

    test('Attempting to remove image when none is set does nothing visible (no button rendered)', async ({ page }) => {
        // On a fresh questionnaire there should be no "Remove image" button
        const removeImageButton = page.locator('button[name="RemoveImage"]');
        await expect(removeImageButton, '❌ Remove image button should not be visible when no image is set')
            .toHaveCount(0);

        // Sanity: page still in valid state
        await stylingPage.expectOnStylingPage();
    });

    test('Uploading an image then saving renders remove image button on re-visit', async ({ page }) => {
        // Upload any image-like file from test fixtures
        await stylingPage.uploadDecorativeImage('../../assets/sample-start-page-image.jpg');
        await stylingPage.acceptAccessibilityAgreement();
        await stylingPage.saveAndContinue();

        // Back to track page
        await expect(page).toHaveURL(
            new RegExp(`/admin/questionnaires/${questionnaireId}.*?/track`)
        );

        // Navigate back to styling page
        await page.getByRole('link', { name: 'Customise styling' }).click();
        stylingPage = new QuestionnaireStylingPage(page);
        await stylingPage.expectOnStylingPage();

        // Now the remove image button should be present
        const removeImageButton = page.locator('button[name="RemoveImage"]');
        await expect(removeImageButton, '❌ Remove image button should be visible once an image is set')
            .toBeVisible();
    });

    test('Removing an uploaded image redirects to track with appropriate banner', async ({ page }) => {
        // Upload an image and save
        await stylingPage.uploadDecorativeImage('../../assets/sample-start-page-image.jpg');
        await stylingPage.acceptAccessibilityAgreement();
        await stylingPage.saveAndContinue();

        await expect(page).toHaveURL(
            new RegExp(`/admin/questionnaires/${questionnaireId}.*?/track`)
        );

        // Go back to styling page
        await page.getByRole('link', { name: 'Customise styling' }).click();
        stylingPage = new QuestionnaireStylingPage(page);
        await stylingPage.expectOnStylingPage();

        // Remove the image
        await stylingPage.clickRemoveImage();

        await expect(page).toHaveURL(
            new RegExp(`/admin/questionnaires/${questionnaireId}/track`)
        );

        const editQuestionnairePage = new DesignQuestionnairePage(page);
        await editQuestionnairePage.expectSuccessBannerVisible();
        await editQuestionnairePage.assertRemovedStartPageImageSuccessBanner();
    });
});