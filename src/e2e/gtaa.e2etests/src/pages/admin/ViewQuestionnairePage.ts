import {expect, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ViewQuestionnaireTable} from './components/ViewQuestionnaireTable';

export class ViewQuestionnairePage extends BasePage {
    private static readonly VIEW_URL = '/admin/questionnaires';
    // ===== Locators  =====
    private readonly section = this.page.locator(
        'div.app-scrolling-wrapper[role="region"]'
    );

    private readonly createNewQuestionnaireButton = this.page.locator(
        'a.govuk-button.govuk-button--start[href$="/questionnaires/create"]'
    );

    // ===== Embedded component =====
    readonly table: ViewQuestionnaireTable;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);
        this.table = new ViewQuestionnaireTable(page);
    }

    // ===== Actions =====
    async clickCreateNewQuestionnaire(): Promise<void> {
        await this.createNewQuestionnaireButton.click();
    }

    // ===== Validations (structure-only) =====
    async expectUrlOnPage(): Promise<void> {
        await this.validateUrlMatches(ViewQuestionnairePage.VIEW_URL);
    }

    async expectTitleLabelOnPage(): Promise<void> {
        await expect(this.createNewQuestionnaireButton).toBeVisible();
    }
    
    // Optional: expose a structural check that the table exists
    async verifyQuestionnaireListedByStructure(): Promise<void> {
        await this.table.verifyVisible();
    }

    async assertPageElements() {
        await this.expectUrlOnPage();
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await expect(this.section).toBeVisible();
        await expect(this.createNewQuestionnaireButton).toBeVisible();

        // Delegate structural visibility of the table to the component
        await this.table.verifyVisible();
    }
}