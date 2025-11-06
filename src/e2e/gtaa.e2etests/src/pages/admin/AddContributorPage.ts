import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from "../BasePage";

export class AddContributorPage extends BasePage {
    // ===== Locators =====
    private readonly section = this.page.locator('main[role="main"]');
    private readonly backLink = this.section.locator('a.govuk-back-link');
    private readonly h1 = this.section.locator('h1');
    private readonly form = this.section.locator('form[method="post"]');
    private readonly emailInput = this.form.locator(
        '#forms-name-input-name, input[type="email"], input[name="Email"], input[name="email"]'
    ).first();
    private readonly saveAndContinueBtn = this.form.locator('button.govuk-button[type="submit"]');

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);
    }

    // ===== Actions =====
    async fillEmail(value: string): Promise<void> {
        await this.emailInput.fill(value);
    }

    async clickSaveAndContinue(): Promise<void> {
        await this.saveAndContinueBtn.click();
    }

    async clickBack(): Promise<void> {
        await this.backLink.click();
    }

    // ===== Assertions (structure-only, no wording checks) =====
    async expectBasicStructure(): Promise<void> {
        await expect(this.section).toBeVisible();
        await expect(this.backLink).toBeVisible();
        await expect(this.h1).toBeVisible();
        await expect(this.form).toBeVisible();
        await expect(this.emailInput).toBeVisible();

        // Input should be a text/email type
        const typeAttr = await this.emailInput.getAttribute('type');
        expect(['text', 'email']).toContain(typeAttr ?? 'text');

        await expect(this.saveAndContinueBtn).toBeVisible();
        await expect(this.saveAndContinueBtn).toHaveAttribute('type', 'submit');
    }
}