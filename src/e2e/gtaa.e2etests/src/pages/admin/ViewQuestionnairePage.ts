import {expect, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ViewQuestionnaireTable} from './components/ViewQuestionnaireTable';

export class ViewQuestionnairePage extends BasePage {
    private static readonly VIEW_URL = '/admin/questionnaires';
    // ===== Locators  =====
    private readonly section = this.page.locator(
        'div.app-scrolling-wrapper[role="region"]'
    );

    private readonly HelpUserHeading = this.page.locator(
        'section#main-content-header h1.govuk-heading-xl'
    );

    private readonly HelpUserDescription = this.page.locator(
        'section#main-content-header p.govuk-body'
    );

    private readonly questionnaireHeading = this.page.locator(
        'main h1.govuk-heading-l'
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

    async expectQuestionnaireHeadingOnPage(): Promise<void> {
        await expect(this.questionnaireHeading).toBeVisible();
    }
    
    async verifyHelpUserHeadingVisible() {
        await expect(this.HelpUserHeading).toBeVisible();
    }

    async verifyHelpUserDescriptionVisible() {
        await expect(this.HelpUserDescription).toBeVisible();
    }

    async verifyCreateButtonVisible() {
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
        await this.verifyCreateButtonVisible();

        // Delegate structural visibility of the table to the component
        await this.table.verifyVisible();
    }
}