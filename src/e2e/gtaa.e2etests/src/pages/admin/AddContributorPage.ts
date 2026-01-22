import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from '../BasePage';

export class AddContributorPage extends BasePage {
    // ===== Locators =====
    private readonly backLink: Locator;
    private readonly form: Locator;

    private readonly heading: Locator;
    private readonly caption: Locator;
    private readonly hint: Locator;

    private readonly emailInput: Locator;
    private readonly inlineError: Locator;
    private readonly errorSummary: Locator;

    private readonly saveAndContinueBtn: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.backLink = this.page.locator('a.govuk-back-link');
        this.form = this.page.locator('form[method="post"]');

        this.heading = this.page.locator('#add-person-title');
        this.caption = this.heading.locator('#current-questionnaire-title');
        this.hint = this.page.locator('#add-person-hint');

        this.emailInput = this.form.locator('#ContributorEmail');
        this.inlineError = this.form.locator('#contributoremail-error');
        this.errorSummary = this.page.locator('.govuk-error-summary[role="alert"][tabindex="-1"]');

        this.saveAndContinueBtn = this.form.locator(
            '#save-and-continue'
        );
    }

    // ===== Actions =====

    async fillEmail(value: string): Promise<void> {
        await this.emailInput.fill(value);
    }

    async clickSaveAndContinue(): Promise<void> {
        await this.saveAndContinueBtn.click();
        await this.waitForPageLoad();
    }

    async clickBack(): Promise<void> {
        await this.backLink.click();
        await this.waitForPageLoad();
    }

    // ===== Assertions =====

    async expectBasicStructure(): Promise<void> {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();

        await expect(this.backLink).toBeVisible();
        await expect(this.form).toBeVisible();

        await expect(this.heading).toBeVisible();
        await expect(this.heading).toContainText('Add person');

        await expect(this.caption).toBeVisible();
        const captionText = (await this.caption.textContent())?.trim() ?? '';
        expect(captionText.length).toBeGreaterThan(0);

        await expect(this.hint).toBeVisible();
        const hintText = await this.hint.innerText();
        expect(hintText).toMatch(/email address/i);

        await expect(this.emailInput).toBeVisible();
        const typeAttr = await this.emailInput.getAttribute('type');
        expect(['text', 'email']).toContain(typeAttr ?? 'text');

        await expect(this.saveAndContinueBtn).toBeVisible();
        await expect(this.saveAndContinueBtn).toHaveText(/Save and continue/i);
    }

    async expectValidationErrorVisible(): Promise<void> {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary).toHaveAttribute('role', 'alert');
        await expect(this.errorSummary).toHaveAttribute('tabindex', '-1');

        await expect(this.inlineError, '❌ Inline email error missing').toBeVisible();
    }
}