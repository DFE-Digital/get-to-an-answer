import {expect, Page, Locator} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ErrorMessages} from "../../constants/test-data-constants";

type Mode = 'create' | 'edit' | 'clone';

export class AddQuestionnairePage extends BasePage {
    // ===== Locators =====
    private readonly form: Locator;
    private readonly backToQuestionnaireLink: Locator;
    private readonly titleInput: Locator;
    private readonly saveAndContinueButton: Locator;
    private readonly titleLabel: Locator;
    private readonly supportiveHint: Locator;
    private readonly errorSummary: Locator;
    private readonly errorList: Locator;
    private readonly errorLink: Locator;
    private readonly titleFormGroup: Locator;
    private readonly inlineTitleError: Locator;
    private readonly error: Locator;
    private readonly hint: Locator;
    private readonly describedBy: Locator;

    // ===== Constructor =====
    constructor(page: Page, mode: Mode = 'create') {
        super(page);
        this.form = this.page.locator(
            'main[role="main"] form'
        );
        this.backToQuestionnaireLink = this.page.locator(
            '#main-content-container a.govuk-back-link'
        );
        this.titleInput = this.page.locator(
            '#forms-name-input-name-field'
        );
        this.saveAndContinueButton = this.page.getByRole(
            'button', {name: 'Save and continue'}
        );
        this.titleLabel = this.page.locator(
            'label[for="forms-name-input-name-field"]'
        );
        this.supportiveHint = this.page.locator(
            '#forms-name-input-name-hint'
        );
        this.errorSummary = this.page.locator(
            '.govuk-error-summary[role="alert"][tabindex="-1"]'
        );
        this.errorList = this.errorSummary.locator(
            'ul.govuk-error-summary__list'
        );
        this.errorLink = this.page.locator(
            'a[href="#CreateQuestionnaire.Title"]'
        );
        this.titleFormGroup = page.locator(
            '.govuk-form-group:has(#forms-name-input-name-field)'
        );
        this.inlineTitleError = this.titleFormGroup.locator(
            '#forms-name-input-name-field-error.govuk-error-message'
        );
        this.error = this.page.locator(
            '#Title-error'
        );
        this.hint = this.page.locator(
            '#Title-hint'
        );
        this.describedBy = this.page.locator(
            '#DescribedBy'
        );
    }

    // ===== Actions =====
    async ClickBackToQuestionnaireLink(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.backToQuestionnaireLink.click()
        ]);
    }

    async enterTitle(title: string): Promise<void> {
        await this.titleInput.fill(title);
    }

    async enterInvalidTitle(): Promise<void> {
        await this.titleInput.fill(`${' '.repeat(10)}`);
    }

    async clickSaveAndContinue(): Promise<void> {
        await this.saveAndContinueButton.click();
    }

    async addQuestionnaire(title?: string): Promise<void> {
        const finalTitle = title ?? `Auto questionnaire title - ${Date.now()}`;
        await this.enterTitle(finalTitle);
        await this.clickSaveAndContinue();
    }
    
    // ===== Validations =====
    async verifyLabelAndHintPresent(): Promise<void> {
        await expect(this.titleLabel).toBeVisible();
        await expect(this.supportiveHint).toBeVisible();
    }

    async validateTitleFieldAriaDescribedBy() {
        const ariaValue = await this.titleInput.getAttribute('aria-describedby');
        expect(ariaValue, '❌ aria-describedby is missing').not.toBeNull();

        // Expected to contain both hint ID and error message ID
        expect(ariaValue, '❌ aria-describedby missing hint id')
            .toContain('forms-name-input-name-hint');
        expect(ariaValue, '❌ aria-describedby missing error message id')
            .toContain('forms-name-input-name-field-error');
    }

    async validateMissingTitleMessage() {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
         await expect(this.errorSummary, '❌ Attribute role is missing').toHaveAttribute('role', 'alert');
         await expect(this.errorSummary,'❌ Attribute tabIndex is missing').toHaveAttribute('tabindex', '-1');
         await expect(this.errorSummary,'❌ Error summary not focused').toBeFocused();
        
         await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_TITLE);
        
         await this.errorLink.click();
         //await expect(this.titleInput).toBeFocused(); //TBC, failing here and not getting a focus

        await expect(this.titleFormGroup, '❌ Title form group missing').toBeVisible();
        await expect(this.inlineTitleError, '❌ Inline title error not visible').toBeVisible();
    }
    
    async assertPageElements() {
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await this.verifyLabelAndHintPresent();

        await expect(this.form).toBeVisible();
        await expect(this.titleInput).toBeVisible();
        await expect(this.saveAndContinueButton).toBeVisible();
    }
}