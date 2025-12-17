import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {Timeouts} from "../../constants/timeouts";

export class AddBulkAnswerOptionsPage extends BasePage {
    private readonly heading: Locator;
    private readonly caption: Locator;
    private readonly bulkTextArea: Locator;
    private readonly continueButton: Locator;
    private readonly hint: Locator;

    private readonly errorSummary: Locator;
    private readonly errorList: Locator;
    private readonly errorLink: Locator;
    private readonly inlineError: Locator;
    private readonly formGroup: Locator;

    constructor(page: Page) {
        super(page);

        this.heading = page.locator('h1.govuk-heading-l');
        this.caption = page.locator('.govuk-caption-l');
        this.bulkTextArea = page.locator('textarea#BulkAnswerOptions');
        this.continueButton = page.getByRole('button', {name: 'Continue'});
        this.hint = page.locator('#BulksAnswerOptions-hint');

        this.errorSummary = page.locator('.govuk-error-summary[role="alert"][tabindex="-1"]');
        this.errorList = this.errorSummary.locator('ul.govuk-error-summary__list');
        this.errorLink = page.locator('a[href="#BulkAnswerOptions"]');
        this.inlineError = page.locator('#BulkAnswerOptions-error');
        this.formGroup = page.locator('.govuk-form-group:has(#BulkAnswerOptions)');
    }

    async expectOnPage(): Promise<void> {
        await expect(this.heading).toContainText(/Enter all answer options/i);
        await expect(this.bulkTextArea).toBeVisible();
        await expect(this.continueButton).toBeVisible();
    }

    async enterBulkOptions(text: string): Promise<void> {
        await this.bulkTextArea.fill(text);
    }

    async clearBulkOptions(): Promise<void> {
        await this.bulkTextArea.clear();
    }

    async clickContinue(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.continueButton.click()
        ]);
    }

    async validateDuplicateEntriesError(browserName: string) {
        const expectedMessage = 'Duplicate entries found';
        await expect(this.errorSummary).toBeVisible();
        await expect(this.errorList).toContainText(expectedMessage);
        await expect(this.inlineError).toContainText(expectedMessage);

        if (browserName !== 'webkit') {
            await this.errorLink.click();
            await expect(this.bulkTextArea).toBeFocused();
        }
    }

    async validateAriaDescribedBy() {
        await this.bulkTextArea.waitFor({state: 'visible', timeout: Timeouts.LONG});
        const ariaValue = await this.bulkTextArea.getAttribute('aria-describedby');

        expect(ariaValue, '❌ aria-describedby is missing').not.toBeNull();
        expect(ariaValue).toContain('BulksAnswerOptions-hint');
    }
}