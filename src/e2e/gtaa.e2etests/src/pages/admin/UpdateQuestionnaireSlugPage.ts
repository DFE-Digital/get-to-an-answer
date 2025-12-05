import {Locator, Page, expect} from '@playwright/test';
import {BasePage} from "../BasePage";
import {ErrorMessages} from "../../constants/test-data-constants";

export class UpdateQuestionnaireSlugPage extends BasePage {
    readonly backLink: Locator;
    readonly heading: Locator;
    readonly slugInput: Locator;
    readonly saveAndContinueButton: Locator;
    readonly errorSummary: Locator;
    readonly errorList: Locator;
    readonly errorLink: Locator;
    readonly inlineSlugError: Locator;

    constructor(page: Page) {
        super(page);

        this.backLink = page.getByRole('link', {name: /back to/i});
        this.heading = page.getByRole('heading', {level: 1});
        this.slugInput = page.locator('#questionnaire-slug');
        this.saveAndContinueButton = page.getByRole('button', {name: /save and continue/i});

        this.errorSummary = this.page.locator('.govuk-error-summary');
        this.errorList = this.page.locator('.govuk-error-summary__list');
        this.errorLink = this.page.locator('.govuk-error-summary__list a');

        this.inlineSlugError = this.page.locator('#questionnaire-slug-field-error');
    }

    async expectHeadingOnEditSlugPage(expectedText?: string): Promise<void> {
        await expect(this.heading, '❌ Edit slug page heading not visible').toBeVisible();

        if (expectedText) {
            await expect(
                this.heading,
                `❌ Edit slug page heading text mismatch: expected "${expectedText}"`
            ).toContainText(expectedText);
        }
    }

    async validateDuplicateSlugMessageSummary(browserName: string) {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary, '❌ Role attribute missing').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ tabindex attribute missing').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_DUPLICATE_QUESTIONNAIRE_SLUG);

        await this.errorLink.click();
        if (browserName !== 'webkit') {
            await expect(this.errorLink).toBeFocused();
        }
    }

    async validateInlineSlugError() {
        await expect(this.inlineSlugError, '❌ Inline slug error not visible').toBeVisible();
        await expect(this.inlineSlugError).toContainText(ErrorMessages.ERROR_MESSAGE_DUPLICATE_QUESTIONNAIRE_SLUG);
    }

    async enterSlug(value: string): Promise<void> {
        await this.slugInput.fill(value);
    }

    async submit(): Promise<void> {
        await this.saveAndContinueButton.click();
    }
}