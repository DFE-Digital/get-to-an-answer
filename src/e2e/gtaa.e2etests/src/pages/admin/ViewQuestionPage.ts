import {Page, Locator, expect} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ViewQuestionTable} from './components/ViewQuestionTable';

export class ViewQuestionPage extends BasePage {
    // Container that holds the “Your questions” block 
    private readonly section: Locator;

    // Top action links 
    private readonly addQuestionLink: Locator; 
    private readonly previewLink: Locator; 

    // Radios: name/value
    private readonly finishedYesRadio: Locator;
    private readonly finishedNoRadio: Locator; 

    // Primary submit button
    private readonly saveAndContinueButton: Locator; 

    // Embedded component
    readonly table: ViewQuestionTable;

    constructor(protected readonly page: Page) {
        super(page);

        // Root region of the questions section
        this.section = page.locator('div.app-page-list');

        // Action links (href fragments are stable even if link text changes)
        this.addQuestionLink = page.locator('a.govuk-link[href$="/questions/create"]');
        this.previewLink = page.locator('a.govuk-link[href*="/start/preview"]');

        // Radios: rely on 'name' and 'value'
        const radioName = 'forms_mark_pages_section_complete_input_mark_complete';
        this.finishedYesRadio = page.locator(`input[type="radio"][name="${radioName}"][value="true"]`);
        
        this.finishedNoRadio = page
            .locator(`input[type="radio"][name="${radioName}"][value="false"]`)
            .or(page.locator(`input[type="radio"][name="${radioName}"]`).nth(1));

        // Submit button
        this.saveAndContinueButton = page.locator('button.govuk-button[type="submit"]');

        // Reuse your table component
        this.table = new ViewQuestionTable(page);
    }

    // --- Actions ---
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
