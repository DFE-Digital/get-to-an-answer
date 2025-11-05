import { expect, Locator, Page } from '@playwright/test';

export class EditAnswersTable {
    private readonly card: Locator; // summary card titled "Answers"
    private readonly table: Locator; // the govuk table inside the card
    private readonly rows: Locator;  // tbody rows

    constructor(private readonly page: Page) {
        // Scope to the summary card headed "Answers"
        const title = page.getByRole('heading', { level: 2, name: 'Answers' });
        this.card = title.locator('..').locator('..'); // title wrapper -> card
        this.table = this.card.getByRole('table');
        this.rows = this.table.locator('tbody tr');
    }

    // --- validations ---
    async assertLoaded(): Promise<void> {
        await expect(this.card).toBeVisible();
        await expect(this.table).toBeVisible();
        await expect(this.table.getByRole('columnheader', { name: 'Answer' })).toBeVisible();
        await expect(this.table.getByRole('columnheader', { name: 'Score' })).toBeVisible();
        await expect(this.table.getByRole('columnheader', { name: 'Target Type' })).toBeVisible();
    }

    async verifyVisible(): Promise<void> {
        await expect(this.table).toBeVisible();
    }

    // --- row helpers ---
    private rowByAnswer(answer: string): Locator {
        // match a row that contains a cell with the exact answer text
        return this.rows.filter({
            has: this.page.getByRole('cell', { name: answer, exact: true }),
        }).first();
    }

    async getRowCount(): Promise<number> {
        return this.rows.count();
    }

    async assertRowPresent(answer: string): Promise<void> {
        await expect(this.rowByAnswer(answer)).toBeVisible();
    }

    async getCellText(
        answer: string,
        column: 'Answer' | 'Score' | 'Target Type'
    ): Promise<string> {
        const row = this.rowByAnswer(answer);
        const colIndex = column === 'Answer' ? 0 : column === 'Score' ? 1 : 2;
        return (await row.locator('td').nth(colIndex).innerText()).trim();
    }

    // --- actions ---
    async openEdit(): Promise<void> {
        const editLink = this.card.getByRole('link', { name: 'Edit' });
        await expect(editLink).toBeVisible();
        await editLink.click();
    }
}