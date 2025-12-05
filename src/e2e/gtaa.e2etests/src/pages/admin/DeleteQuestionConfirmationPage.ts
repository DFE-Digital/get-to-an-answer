import { expect, Locator, Page } from '@playwright/test';
import {BasePage} from "../BasePage";
import {Timeouts} from "../../constants/timeouts";

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
    async clickContinue(): Promise<void> {
        await this.continueButton.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.continueButton.click();
    }

    // ====== RADIO SELECTORS ======
    async selectYes(): Promise<void> {
        await this.yesRadio.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.yesRadio.check();
    }

    async selectNo(): Promise<void> {
        await this.noRadio.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.noRadio.check();
    }
}