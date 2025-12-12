import {Page, Locator} from '@playwright/test';
import {BasePage} from "../BasePage";

export class EditContinueButtonTextPage extends BasePage {
    readonly backLink: Locator;
    readonly main: Locator;
    readonly heading: Locator;
    readonly caption: Locator;
    readonly introHint: Locator;
    readonly warningText: Locator;
    readonly buttonTextLabel: Locator;
    readonly buttonTextHint: Locator;
    readonly buttonTextInput: Locator;
    readonly submitButton: Locator;

    constructor(page: Page) {
        super(page);

        this.backLink = page.locator('a.govuk-back-link');
        this.main = page.locator('main#main-content.govuk-main-wrapper');
        this.heading = page.locator('h1.govuk-heading-l');
        this.caption = this.heading.locator('span.govuk-caption-l');
        this.introHint = page.locator('.govuk-hint.govuk-!-margin-bottom-8');
        this.warningText = page.locator('.govuk-warning-text');

        this.buttonTextLabel = page.locator('label[for="continue-button-text"]');
        this.buttonTextHint = page.locator('#continue-button-text-hint');
        this.buttonTextInput = page.locator('#continue-button-text');

        this.submitButton = page.locator('button.govuk-button[type="submit"]');
    }

    async enterButtonText(text: string): Promise<void> {
        await this.buttonTextInput.fill(text);
    }

    async clickSaveAndContinue(): Promise<void> {
        await this.submitButton.click();
    }

    async clickBack(): Promise<void> {
        await this.backLink.click();
    }
}