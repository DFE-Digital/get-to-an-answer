import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ViewQuestionTable} from './components/ViewQuestionTable';

export class ViewQuestionPage extends BasePage {
    private readonly radioName = 'forms_mark_pages_section_complete_input_mark_complete';
    // ===== Locators  ===== 
    private readonly section = this.page.locator('div.app-page-list');

    private readonly addQuestionLink = this.page.locator('a.govuk-link[href$="/questions/create"]');
    private readonly previewLink = this.page.locator('a.govuk-link[href*="/start/preview"]');

    private readonly finishedYesRadio = this.page.locator(`input[type="radio"][name="${this.radioName}"][value="true"]`);
    private readonly finishedNoRadio = this.page
        .locator(`input[type="radio"][name="${this.radioName}"][value="false"]`)
        .or(this.page.locator(`input[type="radio"][name="${this.radioName}"]`).nth(1));

    private readonly saveAndContinueButton = this.page.locator('button.govuk-button[type="submit"]');

    // ===== Embedded component =====
    readonly table: ViewQuestionTable;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);
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

    // --- Assertions (structure-only; no wording checks) ---
    async verifyOnViewQuestionsPage(): Promise<void> {
        await expect(this.page).toHaveURL(/\/questions(\/)?$/);
        await expect(this.section).toBeVisible();
        await expect(this.addQuestionLink).toBeVisible();
        await expect(this.previewLink).toBeVisible();
        await expect(this.finishedYesRadio).toBeVisible();
        await expect(this.finishedNoRadio).toBeVisible();
        await expect(this.saveAndContinueButton).toBeVisible();
        await this.table.verifyVisible();
    }

    async verifyQuestionListedByStructure(): Promise<void> {
        // delegates to the component; keep this if you want a structural “table visible” check only
        await this.table.verifyVisible();
    }
}
