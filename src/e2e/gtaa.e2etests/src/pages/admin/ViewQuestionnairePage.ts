import { expect, Page } from '@playwright/test';
import {BasePage} from "../BasePage";
import {ViewQuestionnaireTable} from "./components/ViewQuestionnaireTable";

export class ViewQuestionnairePage extends BasePage {

    // ===== Locators =====
    private heading = this.page.getByRole('heading', { name: 'Your questionnaires' });
    private createNewQuestionnaireButton = this.page.locator('[data-test="continue-button"]');

    readonly table: ViewQuestionnaireTable;
    constructor(page: Page) {
        super(page);
        this.table = new ViewQuestionnaireTable(page);
    }

    // --- Actions ---
    async ClickCreateNewQuestionnaire(): Promise<void> {
        await this.createNewQuestionnaireButton.click();
    }

    // --- Validations ---
    async verifyOnViewQuestionnairesPage(): Promise<void> {
        await expect(this.page).toHaveURL('/questionnaires');
        await expect(this.heading).toBeVisible();
        await expect(this.createNewQuestionnaireButton).toBeVisible();
        await this.table.verifyVisible();
        await this.table.verifyHeaders();
    }

    async verifyQuestionnaireListed(name: string): Promise<void> {
        await this.table.expectRowPresentByName(name);
    }
}