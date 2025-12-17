import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ErrorMessages} from "../../constants/test-data-constants";
import {Timeouts} from "../../constants/timeouts";

export class CloneQuestionnairePage extends BasePage {
    private readonly heading: Locator;
    private readonly caption: Locator;
    private readonly titleInput: Locator;
    private readonly saveButton: Locator;
    private readonly errorSummary: Locator;
    private readonly inlineError: Locator;

    constructor(page: Page) {
        super(page);

        this.heading = page.locator('h1.govuk-heading-l');
        this.caption = page.locator('.govuk-caption-l');
        this.titleInput = page.locator('#questionnaire-title');
        this.saveButton = page.locator('button.govuk-button');
        this.errorSummary = page.locator('.govuk-error-summary[role="alert"]');
        this.inlineError = page.locator('#questionnaire-title-field-error');
    }

    async expectOnPage(): Promise<void> {
        await expect(this.heading).toHaveText(/Make a copy/i);
        await expect(this.caption).toBeVisible();
        await expect(this.titleInput).toBeVisible();
        await expect(this.saveButton).toBeVisible();
    }

    async enterTitle(title: string): Promise<void> {
        await this.titleInput.fill(title);
    }

    async clearTitle(): Promise<void> {
        await this.titleInput.fill('');
    }

    async clickMakeCopy(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.saveButton.click()
        ]);
    }

    async expectValidationErrors(browserName: string): Promise<void> {
        await expect(this.errorSummary, 'Error summary missing').toBeVisible();
        await expect(this.errorSummary, 'Error summary missing message').toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE);
        await this.validateErrorLinkBehaviour(this.errorSummaryLink(""), ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE, browserName);

        await expect(this.inlineError, 'Inline error missing').toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE);
    }

    async validateErrorLinkBehaviour(link: Locator, expectedMessage: string, browserName: string): Promise<void> {
        await expect(link, '❌ Error summary link missing').toBeVisible();
        await expect(link, '❌ Error link text mismatch').toHaveText(expectedMessage);

        await link.click();

        if (browserName !== 'webkit') {
            await expect(this.titleInput, '❌ Title input not focused after error link click').toBeFocused();
        }
    }

    async expectPrefilledTitle(originalTitle: string): Promise<void> {
        await expect(this.titleInput).toHaveValue(`Copy of ${originalTitle}`);
    }
}
