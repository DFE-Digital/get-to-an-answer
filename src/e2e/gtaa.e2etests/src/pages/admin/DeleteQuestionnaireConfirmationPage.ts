import {expect, Locator, Page} from "@playwright/test";
import {BasePage} from "../BasePage";

export class DeleteQuestionnaireConfirmationPage extends BasePage {
    // ----- Core UI -----
    readonly backLink: Locator;
    readonly form: Locator;
    readonly continueButton: Locator;
    readonly radiosFormGroup: Locator;
    readonly radios: Locator;
    readonly yesRadio: Locator;
    readonly noRadio: Locator;

    // ----- Validation UI -----
    readonly errorSummary: Locator;
    readonly errorSummaryLinks: Locator;
    readonly inlineError: Locator;

    constructor(page: Page) {
        super(page)

        // Back link at the top
        this.backLink = page.getByRole('link', { name: /back/i });

        // Main POST form
        this.form = page.locator('form[method="post"]');

        // Continue button at the bottom of the form
        this.continueButton = this.form.locator(
            'button.govuk-button[type="submit"]'
        );

        // Radios group – "Do you want to delete this questionnaire?"
        this.radiosFormGroup = this.form
            .locator('.govuk-form-group')
            .filter({ has: this.form.locator('input[name="DeleteQuestionnaire"]') });

        // All radio inputs for this question
        this.radios = this.form.locator(
            'input[name="DeleteQuestionnaire"][type="radio"]'
        );

        // Individual radios (Yes / No)
        this.yesRadio = this.form.locator(
            'input#finished-yes[name="DeleteQuestionnaire"]'
        );
        this.noRadio = this.form.locator(
            'input#finished-no[name="DeleteQuestionnaire"]'
        );

        // Validation summary (after submitting with no selection)
        this.errorSummary = page.locator(
            '.govuk-error-summary[role="alert"][tabindex="-1"]'
        );
        this.errorSummaryLinks = this.errorSummary.locator(
            '.govuk-error-summary__list a'
        );

        // Inline error message above radios
        this.inlineError = this.radiosFormGroup.locator('.govuk-error-message');
    }

    // ===== Actions =====
    async clickBackLink(): Promise<void> {
        await this.backLink.click();
    }

    async chooseYes(): Promise<void> {
        await this.yesRadio.check();
    }

    async chooseNo(): Promise<void> {
        await this.noRadio.check();
    }

    async clickContinue(): Promise<void> {
        await this.continueButton.click();
    }

    // ===== Basic validations =====
    async expectBackLinkContainsTitle(title: string): Promise<void> {
        await expect(this.backLink, '❌ Back link not visible').toBeVisible();
        const text = (await this.backLink.innerText()).trim();
        expect(
            text,
            '❌ Back link does not include questionnaire title'
        ).toContain(title);
    }

    async expectRequiredChoiceValidation(): Promise<void> {
        // submit without selecting a radio
        await this.clickContinue();

        await expect(
            this.errorSummary,
            '❌ Error summary not visible'
        ).toBeVisible();

        await expect(
            this.errorSummaryLinks,
            '❌ Error summary does not contain links'
        ).toHaveCount(1);

        await expect(
            this.inlineError,
            '❌ Inline error not visible above radios'
        ).toBeVisible();

        const classes = (await this.radiosFormGroup.getAttribute('class')) ?? '';
        expect(
            classes,
            '❌ Radios form-group missing .govuk-form-group--error'
        ).toMatch(/\bgovuk-form-group--error\b/);
    }

    async expectTwoRadiosPresent(): Promise<void> {
        await expect(
            this.radios,
            '❌ Radios not visible'
        ).toHaveCount(2);
    }

    async submitAndReturnSelectedIndex(): Promise<number> {
        const yesChecked = await this.yesRadio.isChecked();
        const noChecked = await this.noRadio.isChecked();

        const selectedIndex = yesChecked ? 0 : noChecked ? 1 : -1;

        await this.clickContinue();
        return selectedIndex;
    }
}