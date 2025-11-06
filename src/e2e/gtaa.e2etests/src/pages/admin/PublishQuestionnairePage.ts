import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from '../BasePage';

export class PublishQuestionnairePage extends BasePage {
    // ===== Locators =====
    private readonly section: Locator = this.page.locator('main[role="main"]');
    private readonly backLink: Locator = this.section.locator('a.govuk-back-link');
    private readonly h1: Locator = this.section.locator('h1');
    private readonly form: Locator = this.section.locator('form[method="post"][action$="/publish"]');
    
    private readonly yesRadio: Locator =
        this.form.locator('#publish-questionnaire-yes-input')
            .or(this.form.locator('input[type="radio"][value="true"]'))
            .first();

    private readonly noRadio: Locator =
        this.form.locator('#publish-questionnaire-no-input')
            .or(this.form.locator('input[type="radio"][value="false"]'))
            .first();

    private readonly radios: Locator = this.form.locator('input[type="radio"]');
    
    private readonly continueBtn: Locator =
        this.form.locator('button.govuk-button.govuk-button--primary[type="submit"]');

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
    async expectBasicStructure(): Promise<void> {
        await expect(this.section).toBeVisible();
        await expect(this.backLink).toBeVisible();
        await expect(this.h1).toBeVisible();
        await expect(this.form).toBeVisible();

        const radioCount = await this.radios.count();
        expect(radioCount).toBeGreaterThan(1);

        await expect(this.yesRadio).toBeVisible();
        await expect(this.noRadio).toBeVisible();

        await expect(this.continueBtn).toBeVisible();
        await expect(this.continueBtn).toHaveAttribute('type', 'submit');
    }
}