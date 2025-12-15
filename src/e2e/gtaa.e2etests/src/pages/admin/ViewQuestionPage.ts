import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ViewQuestionTable} from './components/ViewQuestionTable';
import {ErrorMessages} from "../../constants/test-data-constants";
import {Timeouts} from "../../constants/timeouts";

export class ViewQuestionPage extends BasePage {
    // ===== Locators  ===== 
    private readonly section: Locator;
    private readonly backToEditQuestionnaireLink: Locator;
    private readonly questionHeading: Locator;
    private readonly successBanner: Locator;
    private readonly successBannerHeading: Locator;
    private readonly statusTag: Locator;
    private readonly addQuestionLink: Locator;
    private readonly finishedYesRadio: Locator;
    private readonly finishedNoRadio: Locator;
    private readonly saveAndContinueButton: Locator;
    private readonly errorSummary: Locator;
    private readonly errorList: Locator;

    // ===== Embedded component =====
    readonly table: ViewQuestionTable;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.questionHeading = this.page.locator('main h1.govuk-heading-l');
        this.section = this.page.locator('div.govuk-grid-row.gtaa-add-edit-questions');
        this.backToEditQuestionnaireLink = page.locator('a.govuk-back-link');
        this.statusTag = this.section.locator('strong.govuk-tag');
        this.addQuestionLink = this.page.locator(
            'div.govuk-button-group a.govuk-button[href*="/questions/"][href$="/add"]'
        );
        this.finishedYesRadio = this.page.locator(
            'input.govuk-radios__input[name="FinishedEditing"][value="true"]'
        );
        this.finishedNoRadio = this.page.locator(
            'input.govuk-radios__input[name="FinishedEditing"][value="false"]'
        );
        this.saveAndContinueButton = this.page.locator(
            'button.govuk-button[type="submit"][formaction*="handler=SaveAndContinue"]'
        );
        this.errorSummary = this.page.locator('.govuk-error-summary, [role="alert"]').first();
        this.errorList = this.errorSummary.locator(
            'ul.govuk-error-summary__list'
        );
        this.successBanner = page.locator('.govuk-notification-banner--success');
        this.successBannerHeading = this.successBanner.locator('.govuk-notification-banner__heading');

        this.table = new ViewQuestionTable(page);
    }

    // ===== Actions =====
    async ClickBackToEditQuestionnaireLink(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.backToEditQuestionnaireLink.click()
        ]);
    }

    async clickAddQuestion(): Promise<void> {
        await this.addQuestionLink.click();
    }

    async markFinishedEditing(done: boolean): Promise<void> {
        await (done ? this.finishedYesRadio : this.finishedNoRadio).check();
    }

    async saveAndContinue(): Promise<void> {
        await this.saveAndContinueButton.click();
    }

    // ===== Validations =====
    async expectQuestionHeadingOnPage(expectedText?: string): Promise<void> {
        await expect(this.questionHeading).toBeVisible({
            timeout: Timeouts.MEDIUM  
        });

        if (expectedText) {
            // Use toHaveText instead of toContainText for more precise matching
            await expect(this.questionHeading).toHaveText(
                new RegExp(expectedText, 'i'),  // Case-insensitive regex match
                {
                    timeout: Timeouts.MEDIUM,  
                    ignoreCase: true  // Case-insensitive matching
                }
            );
        }
    }

    async expectQuestionnaireStatusOnPage(expectedText?: string): Promise<void> {
        await expect(this.statusTag, '❌ Questionnaire status not visible').toBeVisible();

        if (expectedText) {
            await expect(this.statusTag, `❌ Questionnaire status text does not match: expected "${expectedText}"`).toHaveText(expectedText);
        }
    }

    async verifyQuestionListedByStructure(): Promise<void> {
        await this.table.verifyVisible();
    }

    async expectErrorSummaryVisible(): Promise<void> {
        await expect(this.errorSummary, '❌ Error summary not visible').toBeVisible();
    }

    async validateMoveUpErrorMessageContains(): Promise<void> {
        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_TOP_QUESTION_UP);
    }

    async validateMoveDownErrorMessageContains(): Promise<void> {
        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_BOTTOM_QUESTION_DOWN);
    }

    async getMatchingErrorMessages(expectedMessage: string): Promise<string[]> {
        const errorItems = await this.errorList.locator('li').all();
        const messages: string[] = [];

        for (const item of errorItems) {
            const text = await item.textContent();
            if (text && text.trim() === expectedMessage) {
                messages.push(text.trim());
            }
        }

        return messages;
    }

    async expectComeBackLaterRadioIsSelected(): Promise<void> {
        await expect(this.finishedNoRadio).toBeChecked();
    }

    async expectYesRadioIsNotSelected(): Promise<void> {
        await expect(this.finishedYesRadio).not.toBeChecked();
    }

    async assertQuestionDeletionSuccessBanner(): Promise<void> {
        await expect(
            this.successBanner,
            '❌ Delete question success banner should be visible',
        ).toBeVisible();

        await expect(this.successBannerHeading).toHaveText(
            /The question, .*, has been deleted\./
        );
    }

    async assertPageElements() {
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await expect(this.section, '❌ Section not visible').toBeVisible();
        await expect(this.addQuestionLink, '❌ Add question link not visible').toBeVisible();
        await expect(this.finishedYesRadio, '❌ Finished Yes radio not visible').toBeVisible();
        await expect(this.finishedNoRadio, '❌ Finished No radio not visible').toBeVisible();
        await expect(this.saveAndContinueButton, '❌ Save and continue button not visible').toBeVisible();
        await this.expectComeBackLaterRadioIsSelected();
        await this.table.verifyVisible();
    }
}