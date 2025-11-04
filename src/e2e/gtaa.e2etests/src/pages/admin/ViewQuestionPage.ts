import { Page, Locator, expect } from '@playwright/test';
import {BasePage} from "../BasePage";
import {ViewQuestionTable} from "./components/ViewQuestionTable";

export class ViewQuestionPage extends BasePage{

    // Region containing the list of questions
    private heading = this.page.getByRole('heading', { name: /Your questions/i }).first();
    private questionsRegion = this.heading.locator('..'); // parent wrapper of the section

    // Buttons / radios
    private addQuestionButton = this.page.getByRole('button', { name: 'Add a question' });
    private saveAndContinueButton = this.page.getByRole('button', { name: 'Save and continue' });

    private finishedYesRadio = this.page.getByRole('radio', { name: /^Yes$/ });
    private finishedNoRadio =
        this.page.getByRole('radio', { name: /^No, I’ll come back later$/ }).or(
            this.page.getByRole('radio', { name: /^No/ })
        );

    readonly table: ViewQuestionTable;
    constructor(page: Page) {
        super(page);
        this.table = new ViewQuestionTable(page);
    }
    
    // -------- Page-level actions --------
    async addQuestion() {
        await this.addQuestionButton.click();
    }

    async saveAndContinue() {
        await this.saveAndContinueButton.click();
    }

    async markFinishedEditing(done: boolean) {
        if (done) await this.finishedYesRadio.check();
        else await this.finishedNoRadio.check();
    }

    // --- Validations ---
    async verifyOnViewQuestionsPage(): Promise<void> {
        await expect(this.page).toHaveURL('/questions');
        await expect(this.heading).toBeVisible();
        await expect(this.addQuestionButton).toBeVisible();
        await this.table.verifyVisible();
        await this.table.verifyHeaders();
    }

    async verifyQuestionListed(text: string): Promise<void> {
        await this.table.expectRowPresentByName(text);
    }
}