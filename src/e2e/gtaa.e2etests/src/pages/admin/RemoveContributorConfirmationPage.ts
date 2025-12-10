import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from '../BasePage';

export class RemoveContributorConfirmationPage extends BasePage {
    // ===== Locators =====
    private readonly form: Locator;

    private readonly heading: Locator;
    private readonly caption: Locator;
    private readonly hint: Locator;

    private readonly yesRadio: Locator;
    private readonly noRadio: Locator;

    private readonly continueButton: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.form = this.page.locator('form[method="post"]');

        this.heading = this.form.locator('#remove-contributor-title');
        this.caption = this.heading.locator('#current-questionnaire-title');
        this.hint = this.form.locator('#remove-contributor-hint');

        this.yesRadio = this.form.locator('input.govuk-radios__input#finished-yes');
        this.noRadio = this.form.locator('input.govuk-radios__input#finished-no');

        this.continueButton = this.form.locator(
            '#continue-btn'
        );
    }

    // ===== Actions =====

    async chooseRemoveYes(): Promise<void> {
        await this.yesRadio.check();
    }

    async chooseRemoveNo(): Promise<void> {
        await this.noRadio.check();
    }

    async clickContinue(): Promise<void> {
        await this.continueButton.click();
        await this.waitForPageLoad();
    }

    // ===== Assertions =====

    async assertPageElements(): Promise<void> {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();

        await expect(this.form).toBeVisible();

        await expect(this.heading).toBeVisible();
        await expect(this.heading).toContainText('Are you sure you want to remove access');

        await expect(this.caption).toBeVisible();
        const captionText = (await this.caption.textContent())?.trim() ?? '';
        expect(captionText.length).toBeGreaterThan(0);

        await expect(this.hint).toBeVisible();
        await expect(this.hint).toContainText('will no longer be able to view or edit');

        await expect(this.yesRadio).toBeVisible();
        await expect(this.noRadio).toBeVisible();

        await expect(this.continueButton).toBeVisible();
        await expect(this.continueButton).toHaveText(/Continue/i);
    }

    async expectYesNoRadiosWork(): Promise<void> {
        await this.yesRadio.check();
        await expect(this.yesRadio).toBeChecked();
        await expect(this.noRadio).not.toBeChecked();

        await this.noRadio.check();
        await expect(this.noRadio).toBeChecked();
        await expect(this.yesRadio).not.toBeChecked();
    }
}