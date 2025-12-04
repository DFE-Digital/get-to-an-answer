import { expect, Locator, Page } from '@playwright/test';
import {BasePage} from "../BasePage";

export class DeleteQuestionConfirmationPage extends BasePage{
    private readonly heading: Locator;
    private readonly backLink: Locator;

    private readonly yesRadio: Locator;
    private readonly noRadio: Locator;

    private readonly continueButton: Locator;

    constructor(readonly page: Page) {
        super(page);
        // Main heading
        this.heading = page.getByRole('heading', { level: 1, name: /delete this question/i });

        // Back link
        this.backLink = page.getByRole('link', { name: /back/i });

        // Yes / No radios
        this.yesRadio = page.locator('input#ConfirmDelete-true');
        this.noRadio = page.locator('input#ConfirmDelete-false');

        // Continue button
        this.continueButton = page.getByRole('button', { name: /continue/i });
    }

    // ====== VALIDATION ======
    async validateOnPage(): Promise<void> {
        await expect(this.heading, '❌ Heading is missing').toBeVisible();

        // The “Confirm you want to delete this question” text is stored
        // in the data-val-required attribute of the YES radio input.
        await expect(
            this.yesRadio,
            '❌ Confirmation text attribute missing or incorrect'
        ).toHaveAttribute('data-val-required', /confirm you want to delete this question/i);
    }

    // ====== ACTIONS ======
    async clickBackLink(): Promise<void> {
        await expect(this.backLink).toBeVisible();
        await this.backLink.click();
    }

    async clickContinue(): Promise<void> {
        await expect(this.continueButton).toBeVisible();
        await this.continueButton.click();
    }

    // ====== RADIO SELECTORS ======
    async selectYes(): Promise<void> {
        await expect(this.yesRadio).toBeVisible();
        await this.yesRadio.check();
    }

    async selectNo(): Promise<void> {
        await expect(this.noRadio).toBeVisible();
        await this.noRadio.check();
    }
}