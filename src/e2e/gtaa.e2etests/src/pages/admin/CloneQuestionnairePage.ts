import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ErrorMessages} from "../../constants/test-data-constants";
import {Timeouts} from "../../constants/timeouts";

export class CloneQuestionnairePage extends BasePage {
    private readonly heading: Locator;
    private readonly caption: Locator;
    private readonly titleInput: Locator;
    private readonly saveButton: Locator;
    private readonly backLink: Locator;

    private readonly titleFormGroup: Locator;

    private readonly errorSummary: Locator;
    private readonly inlineTitleError: Locator;
    private readonly errorLink: Locator;
    private readonly errorList: Locator;

    constructor(page: Page) {
        super(page);

        this.heading = page.locator('h1.govuk-heading-l');
        this.caption = page.locator('.govuk-caption-l');
        this.titleInput = page.locator('input#Title');
        this.saveButton = this.page.getByRole('button', {name: 'Make a copy'});
        this.errorSummary = this.page.locator(
            '.govuk-error-summary[role="alert"][tabindex="-1"]'
        );
        this.inlineTitleError = page.locator('#Title-error');

        this.titleFormGroup = page.locator(
            '.govuk-form-group:has(#Title)'
        );

        this.errorLink = this.page.locator(
            'a[href="#Title"]'
        );
        this.errorList = this.errorSummary.locator(
            'ul.govuk-error-summary__list'
        );
        this.backLink = this.page.locator(
            '#main-content-container a.govuk-back-link'
        );
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
        await this.titleInput.clear();
    }

    async clickMakeCopy(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.saveButton.click()
        ]);
    }

    async clickBackLink(): Promise<void> {
        await Promise.all([
            this.waitForPageLoad(),
            this.backLink.click()
        ]);
    }

    async validateMissingTitleMessageSummary(browserName: string) {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary, '❌ Attribute role is missing').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ Attribute tabIndex is missing').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE);

        await this.errorLink.click();
        if (browserName !== 'webkit') {
            await this.errorLink.waitFor({state: 'visible', timeout: Timeouts.LONG});
            await expect(this.errorSummary, '❌ Error summary text mismatch').toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE);
        }
    }

    async validateInlineTitleError() {
        await expect(this.inlineTitleError, '❌ Inline title error not visible').toBeVisible();
    }

    async validateErrorLinkBehaviour(link: Locator, expectedMessage: string, browserName: string) {
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

    async validateTitleFieldAriaDescribedBy() {
        const errorElement = this.inlineTitleError;

        await errorElement.waitFor({state: 'visible', timeout: Timeouts.LONG});

        const ariaValue = await this.titleInput.getAttribute('aria-describedby');
        expect(ariaValue, '❌ aria-describedby is missing').not.toBeNull();

        expect(ariaValue, '❌ aria-describedby missing hint id')
            .toContain('Title-hint');
        expect(ariaValue, '❌ aria-describedby missing error message id')
            .toContain('title-field-error');
    }
}
