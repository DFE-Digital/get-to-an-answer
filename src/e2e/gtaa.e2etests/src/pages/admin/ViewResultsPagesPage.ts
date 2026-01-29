import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ViewQuestionTable} from './components/ViewQuestionTable';
import {ErrorMessages, SuccessBannerMessages} from "../../constants/test-data-constants";
import {ViewResultPageTable} from "./components/ViewResultPageTable";

export class ViewResultsPagesPage extends BasePage {
    // ===== Locators  ===== 
    private readonly banner: Locator;
    private readonly heading: Locator;
    private readonly section: Locator;
    private readonly backToEditQuestionnaireLink: Locator;
    private readonly resultsPagesHeading: Locator;
    private readonly statusTag: Locator;
    private readonly addResultsPageBtn: Locator;
    private readonly finishedYesRadio: Locator;
    private readonly finishedNoRadio: Locator;
    private readonly saveAndContinueButton: Locator;
    private readonly errorSummary: Locator;
    private readonly errorList: Locator;
    private readonly justCreatedBannerText: Locator;
    private readonly justUpdatedBannerText: Locator;
    private readonly justDeletedBannerText: Locator;

    // ===== Embedded component =====
    readonly table: ViewResultPageTable;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);
        
        this.banner = this.page.locator('div.govuk-notification-banner--success[role="alert"]');
        this.heading = this.banner.locator('.govuk-notification-banner__title');
        
        this.justCreatedBannerText = this.page.locator('#just-created-results-page-banner-text');
        this.justUpdatedBannerText = this.page.locator('#just-updated-results-page-text');
        this.justDeletedBannerText = this.page.locator('#just-deleted-banner-text');
        
        this.resultsPagesHeading = this.page.locator('main h1.govuk-heading-l');
        this.section = this.page.locator('div.govuk-grid-row.gtaa-add-edit-results-pages');
        this.backToEditQuestionnaireLink = page.locator('a.govuk-back-link');
        this.statusTag = this.section.locator('strong.govuk-tag');
        this.addResultsPageBtn = this.page.locator('#add-results-page-btn');
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
        this.table = new ViewResultPageTable(page);
    }

    // ===== Actions =====
    async ClickBackToEditQuestionnaireLink(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.backToEditQuestionnaireLink.click()
        ]);
    }
    
    async clickAddResultsPage(): Promise<void> {
        await this.addResultsPageBtn.click();
    }

    async markFinishedEditing(done: boolean): Promise<void> {
        await (done ? this.finishedYesRadio : this.finishedNoRadio).check();
    }

    async saveAndContinue(): Promise<void> {
        await this.saveAndContinueButton.click();
    }

    // ===== Validations =====
    async expectResultsPagesHeadingOnPage(expectedText?: string): Promise<void> {
        await expect(this.resultsPagesHeading, '❌ View question page heading not visible').toBeVisible();
        
        if (expectedText) {
            await expect(
                this.resultsPagesHeading,
                `❌ View question page heading text does not match: expected "${expectedText}"`
            ).toContainText(expectedText);
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

    async assertPageElements() {
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await expect(this.section, '❌ Section not visible').toBeVisible();
        await expect(this.addResultsPageBtn, '❌ Add results page button not visible').toBeVisible();
        await expect(this.finishedYesRadio, '❌ Finished Yes radio not visible').toBeVisible();
        await expect(this.finishedNoRadio, '❌ Finished No radio not visible').toBeVisible();
        await expect(this.saveAndContinueButton, '❌ Save and continue button not visible').toBeVisible();
        await this.expectComeBackLaterRadioIsSelected();
        await this.table.verifyVisible();
    }

    async expectSuccessBannerVisible(): Promise<void> {
        await expect(this.banner).toBeVisible();
        await expect(this.heading).toBeVisible();

        const text = await this.heading.textContent();
        expect(text?.trim().length).toBeGreaterThan(0);
    }

    async assertCreatedResultsPageSuccessBanner(expectedText: string) {
        await expect(this.justCreatedBannerText).toBeVisible();
        await expect(this.justCreatedBannerText).toHaveText(expectedText);
    }

    async assertUpdatedResultsPageSuccessBanner(expectedText: string) {
        await expect(this.justUpdatedBannerText).toBeVisible();
        
        await expect(this.justUpdatedBannerText).toHaveText(expectedText);
    }

    async assertDeletedResultsPageSuccessBanner() {
        await expect(this.justDeletedBannerText).toBeVisible();
        await expect(this.justDeletedBannerText).toHaveText(SuccessBannerMessages.DELETED_RESULTS_PAGE_SUCCESS_MESSAGE);
    }
}