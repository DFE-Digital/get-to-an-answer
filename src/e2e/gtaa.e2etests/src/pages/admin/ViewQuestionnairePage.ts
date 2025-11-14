import { expect, Page, Locator } from '@playwright/test';
import {BasePage} from '../BasePage';
import {ViewQuestionnaireTable} from './components/ViewQuestionnaireTable';
import {ViewQuestionTable} from "./components/ViewQuestionTable";

export class ViewQuestionnairePage extends BasePage {

    private static readonly VIEW_URL = '/admin/questionnaires';
    // ===== Locators  =====
    private readonly section: Locator;
    private readonly HelpUserHeading: Locator;
    private readonly HelpUserDescription: Locator;
    private readonly questionnaireHeading: Locator;
    private readonly createNewQuestionnaireButton: Locator;
    // ===== Embedded component =====
    readonly table: ViewQuestionnaireTable;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);
        this.section = this.page.locator(
            'div.app-scrolling-wrapper[role="region"]'
        );
        this.HelpUserHeading = this.page.locator(
            'section#main-content-header h1.govuk-heading-xl'
        );
        this.HelpUserDescription = this.page.locator(
            'section#main-content-header p.govuk-body'
        );
        this.questionnaireHeading = this.page.locator(
            'main h1.govuk-heading-l'
        );
        this.createNewQuestionnaireButton = this.page.locator(
            'a.govuk-button.govuk-button--start[href$="/questionnaires/create"]'
        );
        this.table = new ViewQuestionnaireTable(page);
    }

    // ===== Actions =====
    async clickCreateNewQuestionnaire(): Promise<void> {
        await this.createNewQuestionnaireButton.click();
    }

    // ===== Validations =====
    async expectQuestionnaireHeadingOnPage(): Promise<void> {
        await expect(this.questionnaireHeading, '❌ Questionnaire heading not visible').toBeVisible();
    }

    async verifyHelpUserHeadingVisible() {
        await expect(this.HelpUserHeading, '❌ Help user heading not visible').toBeVisible();
    }

    async verifyHelpUserDescriptionVisible() {
        await expect(this.HelpUserDescription, '❌ Help user description not visible').toBeVisible();
    }

    async verifyCreateButtonVisible() {
        await expect(this.createNewQuestionnaireButton, '❌ Create new questionnaire button not visible').toBeVisible();
    }

    async verifyQuestionnaireListedByStructure(): Promise<void> {
        await this.table.verifyVisible();
    }

    async assertPageElements() {
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await expect(this.section, '❌ Section not visible').toBeVisible();
        await this.verifyCreateButtonVisible();

        await this.table.verifyVisible();
    }
}