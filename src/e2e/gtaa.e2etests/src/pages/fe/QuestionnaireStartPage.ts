
import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from "../BasePage";

export class QuestionnaireStartPage extends BasePage {
    // ===== Locators =====
    private pageHeading(): Locator {
        return this.page.locator('#main-content-header h1.govuk-heading-xl');
    }

    private descriptionContent(): Locator {
        return this.page.locator('#primary-content');
    }

    private startButton(): Locator {
        return this.page.locator('a.govuk-button.govuk-button--start[role="button"]');
    }

    private errorSummary(): Locator {
        return this.page.locator('.govuk-error-summary');
    }

    constructor(page: Page) {
        super(page);
    }

    // ===== Actions =====
    async goto(questionnaireSlug: string, embed: boolean = false) {
        await this.page.goto(`/questionnaires/${questionnaireSlug}/start?embed=${embed}`);
    }

    async expectOnPage(expectedTitle?: string) {
        if (expectedTitle) {
            await expect(this.pageHeading()).toHaveText(expectedTitle);
        } else {
            await expect(this.pageHeading()).toBeVisible();
        }
    }

    async expectDescription() {
        await expect(this.descriptionContent()).toBeVisible();
    }

    async getHeading(): Promise<string> {
        return await this.pageHeading().textContent() || '';
    }

    async getDescription(): Promise<string> {
        return await this.descriptionContent().textContent() || '';
    }

    async clickStartNow() {
        await this.startButton().click();
    }

    async expectErrorSummary() {
        await expect(this.errorSummary()).toBeVisible();
    }

    async expectNoErrors() {
        await expect(this.errorSummary()).not.toBeVisible();
    }

    async isStartButtonVisible(): Promise<boolean> {
        return await this.startButton().isVisible();
    }
}