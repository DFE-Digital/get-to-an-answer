import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from '../BasePage';

export class UnpublishQuestionnaireConfirmationPage extends BasePage {
    // ===== Locators =====
    private readonly section: Locator = this.page.locator('main[role="main"]');
    private readonly backLink: Locator = this.section.locator('a.govuk-back-link');
    private readonly h1: Locator = this.section.locator('h1');

    // Form and radio buttons
    private readonly form: Locator = this.section.locator('form[method="post"]');

    private readonly yesRadio: Locator =
        this.form.locator('#finished-yes')
            .or(this.form.locator('input[type="radio"][name="UnpublishQuestionnaire"][value="true"]'))
            .first();

    private readonly noRadio: Locator =
        this.form.locator('#finished-no')
            .or(this.form.locator('input[type="radio"][name="UnpublishQuestionnaire"][value="false"]'))
            .first();

    private readonly radios: Locator = this.form.locator('input[type="radio"][name="UnpublishQuestionnaire"]');

    private readonly continueBtn: Locator =
        this.form.locator('button[type="submit"]');

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);
    }

    // ===== Actions =====
    async chooseYes(): Promise<void> {
        await this.yesRadio.check();
    }

    async chooseNo(): Promise<void> {
        await this.noRadio.check();
    }

    async clickContinue(): Promise<void> {
        await this.continueBtn.click();
    }

    async clickBack(): Promise<void> {
        await this.backLink.click();
    }

    // ===== Assertions (structure-only) =====
    async expectTwoRadiosPresent(): Promise<void> {
        await expect(this.section).toBeVisible();
        await expect(this.h1).toBeVisible();
        await expect(this.form).toBeVisible();

        const radioCount = await this.radios.count();
        expect(radioCount).toBeGreaterThanOrEqual(2);

        await expect(this.yesRadio).toBeVisible();
        await expect(this.noRadio).toBeVisible();

        await expect(this.continueBtn).toBeVisible();
        await expect(this.continueBtn).toHaveAttribute('type', 'submit');
    }
}