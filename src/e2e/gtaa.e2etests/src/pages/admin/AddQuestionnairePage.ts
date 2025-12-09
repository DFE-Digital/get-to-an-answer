import {expect, Page, Locator} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ErrorMessages} from "../../constants/test-data-constants";
import {Timeouts} from "../../constants/timeouts";

type Mode = 'create' | 'update' | 'clone';

export class AddQuestionnairePage extends BasePage {
    private readonly mode: string;
    // ===== Locators =====
    private readonly form: Locator;
    private readonly backToQuestionnaireLink: Locator;
    private readonly titleInput: Locator;
    private readonly saveAndContinueButton: Locator;
    private readonly supportiveHint: Locator;
    private readonly errorSummary: Locator;
    private readonly errorList: Locator;
    private readonly errorLink: Locator;
    private readonly titleFormGroup: Locator;
    private readonly inlineTitleError: Locator;
    private readonly inlineUpdateTitleError: Locator;
    private readonly error: Locator;
    private readonly hint: Locator;
    private readonly describedBy: Locator;

    // ===== Constructor =====
    constructor(page: Page, mode: Mode = 'create') {
        super(page);

        this.mode = mode;
        this.form = this.page.locator(
            'main[role="main"] form'
        );
        this.backToQuestionnaireLink = this.page.locator(
            '#main-content-container a.govuk-back-link'
        );
        this.titleInput = this.page.locator(
            'input#Title'
        );
        this.saveAndContinueButton = this.page.getByRole(
            'button', {name: 'Save and continue'}
        );
        this.supportiveHint = this.page.locator(
            '#Title-hint'
        );
        this.errorSummary = this.page.locator(
            '.govuk-error-summary[role="alert"][tabindex="-1"]'
        );
        this.errorList = this.errorSummary.locator(
            'ul.govuk-error-summary__list'
        );
        this.errorLink = this.page.locator(
            'a[href="#Title"]'
        );
        this.titleFormGroup = page.locator(
            '.govuk-form-group:has(#Title)'
        );

        this.inlineUpdateTitleError = this.titleFormGroup.locator(
            '#Title-field-error'
        );
        this.inlineTitleError = this.titleFormGroup.locator(
            '#Title-error'
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
        await this.waitForPageLoad();
    }

    async addQuestionnaire(title?: string): Promise<void> {
        const finalTitle = title ?? `Auto questionnaire title - ${Date.now()}`;
        await this.enterTitle(finalTitle);
        await this.clickSaveAndContinue();
    }

    // ===== Validations =====
    async verifyHintPresent(): Promise<void> {
        await expect(this.supportiveHint).toBeVisible();
    }

    // Accessibility
    async validateTitleFieldAriaDescribedBy() {
        const errorElement = this.mode === 'update'
            ? this.inlineUpdateTitleError
            : this.inlineTitleError;

        await errorElement.waitFor({state: 'visible', timeout: Timeouts.LONG});

        const ariaValue = await this.titleInput.getAttribute('aria-describedby');
        expect(ariaValue, '❌ aria-describedby is missing').not.toBeNull();

        if (this.mode === 'update') {
            expect(ariaValue, '❌ aria-describedby missing hint id')
                .toContain('Title-hint');
            expect(ariaValue, '❌ aria-describedby missing error message id')
                .toContain('title-field-error');
        } else {
            expect(ariaValue, '❌ aria-describedby missing hint id')
                .toContain('Title-hint');
            expect(ariaValue, '❌ aria-describedby missing error message id')
                .toContain('title-field-error');
        }
    }

    async validateMissingTitleMessageSummary(browserName: string) {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary, '❌ Attribute role is missing').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ Attribute tabIndex is missing').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE);

        await this.errorLink.click();
        if (browserName !== 'webkit') {
            await this.errorLink.waitFor({state: 'visible', timeout: Timeouts.LONG});
            await expect(this.errorSummary, '❌ Error summary text mismatch').toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE);
        }
    }

    async validateErrorLinkBehaviour(link: Locator, expectedMessage: string, browserName: string) {
        await expect(link, '❌ Error summary link missing').toBeVisible();
        await expect(link, '❌ Error link text mismatch').toHaveText(expectedMessage);

        await link.click();

        if (browserName !== 'webkit') {
            await expect(this.titleInput, '❌ Title input not focused after error link click').toBeFocused();
            await expect(this.errorSummary, '❌ Error summary text mismatch').toContainText(expectedMessage);

            const outline = await this.titleInput.evaluate((el) => {
                return window.getComputedStyle(el).getPropertyValue('outline');
            });
            expect(outline, '❌ Title input does not show focus outline').not.toBe('none');
        }
    }

    async validateInlineTitleError() {
        if (this.mode === 'update') {
            await expect(this.inlineUpdateTitleError, '❌ Inline title error not visible').toBeVisible();
        } else {
            await expect(this.inlineTitleError, '❌ Inline title error not visible').toBeVisible();
        }
    }

    async validateTitleFormGroup() {
        await expect(this.titleFormGroup, '❌ Title form group missing').toBeVisible();
    }

    async assertPageElements() {
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await this.verifyHintPresent();

        await expect(this.form).toBeVisible();
        await expect(this.titleInput).toBeVisible();
        await expect(this.saveAndContinueButton).toBeVisible();
    }
}