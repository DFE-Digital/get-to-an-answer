import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ViewQuestionTable} from './components/ViewQuestionTable';

export class ViewQuestionPage extends BasePage {

    private readonly questionHeading: Locator;
    private readonly radioName = 'forms_mark_pages_section_complete_input_mark_complete';

    // ===== Locators  ===== 
    private readonly section: Locator;
    private readonly addQuestionLink: Locator;
    private readonly previewLink: Locator;
    private readonly finishedYesRadio: Locator;
    private readonly finishedNoRadio: Locator;
    private readonly saveAndContinueButton: Locator;

    // ===== Embedded component =====
    readonly table: ViewQuestionTable;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.questionHeading = this.page.locator('main h1.govuk-heading-l');
        this.section = this.page.locator('div.app-page-list');
        this.addQuestionLink = this.page.locator('a.govuk-link[href$="/questions/create"]');
        this.previewLink = this.page.locator('a.govuk-link[href*="/start/preview"]');
        this.finishedYesRadio = this.page.locator(`input[type="radio"][name="${this.radioName}"][value="true"]`);
        this.finishedNoRadio = this.page
            .locator(`input[type="radio"][name="${this.radioName}"][value="false"]`)
            .or(this.page.locator(`input[type="radio"][name="${this.radioName}"]`).nth(1));
        this.saveAndContinueButton = this.page.locator('button.govuk-button[type="submit"]');

        this.table = new ViewQuestionTable(page);
    }

    // ===== Actions =====
    async addQuestion(): Promise<void> {
        await this.addQuestionLink.click();
    }

    async clickPreview(): Promise<void> {
        await this.previewLink.click();
    }

    async markFinishedEditing(done: boolean): Promise<void> {
        await (done ? this.finishedYesRadio : this.finishedNoRadio).check();
    }

    async saveAndContinue(): Promise<void> {
        await this.saveAndContinueButton.click();
    }

    // ===== Validations =====
    async expectQuestionHeadingOnPage(expectedText?: string): Promise<void> {
        await expect(this.questionHeading, '❌ Question heading not visible').toBeVisible();

        if (expectedText) {
            await expect(this.questionHeading, `❌ Question heading text does not match: expected "${expectedText}"`).toHaveText(expectedText);
        }
    }

    async verifyQuestionListedByStructure(): Promise<void> {
        await this.table.verifyVisible();
    }

    async assertPageElements() {
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await expect(this.section, '❌ Section not visible').toBeVisible();
        await expect(this.addQuestionLink, '❌ Add question link not visible').toBeVisible();
        await expect(this.previewLink, '❌ Preview link not visible').toBeVisible();
        await expect(this.finishedYesRadio, '❌ Finished Yes radio not visible').toBeVisible();
        await expect(this.finishedNoRadio, '❌ Finished No radio not visible').toBeVisible();
        await expect(this.saveAndContinueButton, '❌ Save and continue button not visible').toBeVisible();
        await this.table.verifyVisible();
    }
}