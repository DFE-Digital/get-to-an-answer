import {expect, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ErrorMessages} from "../../constants/test-data-constants";

type Mode = 'create' | 'edit' | 'clone';

export class AddQuestionnairePage extends BasePage {
    private static readonly ADD_URL: RegExp =
        /\/admin\/questionnaires\/[0-9a-f-]+\/edit\/?$/i;
    // ===== Locators =====
    private readonly form = this.page.locator(
        'form[action*="/questionnaires/"][action$="/edit"][method="post"]'
    );

    private readonly backToQuestionnaireLink = this.form.locator(
        'a.govuk-back-link.govuk-!-display-none-print'
    );

    private readonly titleInput = this.form.locator(
        'input#forms-name-input-name-field[name="Title"][type="text"]'
    );

    private readonly saveAndContinueButton = this.form.locator(
        'button.govuk-button[type="submit"]'
    );

    private readonly titleLabel = this.page.locator(
        'label[for="forms-name-input-name-field"]'
    );

    private readonly supportiveHint = this.page.locator(
        '#forms-name-input-name-hint'
    );

    private readonly errorSummary = this.page.locator(
        '.govuk-error-summary[role="alert"][tabindex="-1"]'
    );

    private readonly errorList = this.errorSummary.locator(
        'ul.govuk-error-summary__list'
    );

    private readonly errorLink = this.page.locator(
        'a[href="#Title"]'
    );

    private readonly titleFormGroup  = this.titleInput.locator(
        'xpath=ancestor::div[contains(@class,"govuk-form-group")][1]'
    );

    private readonly inlineTitleError  = this.titleFormGroup.locator(
        '.govuk-error-message'
    );

    private readonly error  = this.page.locator(
        '#Title-error'
    );

    private readonly hint  = this.page.locator(
        '#Title-hint'
    );

    private readonly describedBy   = this.titleInput.getAttribute(
        'aria-describedby'
    );

    // If using id-based errors, we can also use:
    // this.inlineTitleError = page.locator('#Title-error');

    // ===== Constructor =====
    constructor(page: Page, mode: Mode = 'create') {
        super(page);
    }

    // ===== Actions =====
    async ClickBackToQuestionnaireLink(): Promise<void> {
        await this.backToQuestionnaireLink.click();
    }

    async enterTitle(title: string): Promise<void> {
        await this.titleInput.fill(title);
    }

    async clickSaveAndContinue(): Promise<void> {
        await this.saveAndContinueButton.click();
    }

    async addQuestionnaire(title?: string): Promise<void> {
        const finalTitle = title ?? `Auto questionnaire title - ${Date.now()}`;
        await this.enterTitle(finalTitle);
        await this.enterTitle(`title - ${Date.now()}`);
        await this.clickSaveAndContinue();
    }

    async submitFormAndCheckNoDoubleSubmit(): Promise<void> {
        const requests: string[] = [];
        this.page.on('request', req => {
            if (req.url().includes('/admin/questionnaires/create') && req.method() === 'POST') {
                requests.push(req.url());
            }
        });

        await Promise.all([
            this.waitForPageLoad(),
            this.saveAndContinueButton.click(),
            this.saveAndContinueButton.click(),
        ]);

        expect(requests.length).toBe(1);
        await expect(this.saveAndContinueButton).toBeDisabled();
    }

    // Validations (structure only)
    async expectUrlOnPage(): Promise<void> {
        await this.validateUrlMatches(AddQuestionnairePage.ADD_URL);
    }

    async verifyLabelAndHintPresent(): Promise<void> {
        await expect(this.titleLabel).toBeVisible();
        await expect(this.supportiveHint).toBeVisible();
    }
    
    async expectTitleAriaDescribedByIncludesHintAndError(
        hintId = 'Title-hint',
        errorId = 'Title-error'
    ) {
        await expect(this.hint, 'Hint element should exist').toHaveCount(1);
        await expect(this.error, 'Error element should exist').toHaveCount(1);
        
        expect(this.describedBy, 'aria-describedby should be present').not.toBeNull();
        
        // using a soft check so we can give a clearer message if one is missing
        await expect(async () => {
            expect(this.describedBy!).toContain(hintId);
            expect(this.describedBy!).toContain(errorId);
        }).toPass();
    }
    
    async validateMissingTitleMessageFlow() {
        await expect(this.errorSummary).toBeVisible();
        await expect(this.errorSummary).toHaveAttribute('role', 'alert');
        await expect(this.errorSummary).toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary).toBeFocused();
        
        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_TITLE);
        
        await this.errorLink.click();
        await expect(this.titleInput).toBeFocused();
    }

    async validateInvalidTitleMessageFlow() {
        await expect(this.inlineTitleError).toBeVisible();
        await expect(this.inlineTitleError).toContainText(ErrorMessages.ERROR_MESSAGE_INVALID_TITLE);
        
        await expect(this.titleInput).toHaveClass(/govuk-input--error/);
        
        await expect(this.titleFormGroup).toHaveClass(/govuk-form-group--error/);
    }
    
    async assertPageElements() {
        await this.expectUrlOnPage();
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await this.verifyLabelAndHintPresent();

        await expect(this.form).toBeVisible();
        await expect(this.titleInput).toBeVisible();
        await expect(this.saveAndContinueButton).toBeVisible();
    }
}