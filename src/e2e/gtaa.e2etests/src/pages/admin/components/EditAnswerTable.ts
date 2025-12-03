import { expect, Locator, Page } from '@playwright/test';

export class EditAnswerTable {
    // ===== Locators =====
    private readonly card: Locator;
    private readonly table: Locator;
    private readonly rows: Locator;

    // ===== Constructor =====
    constructor(private readonly page: Page) {
        const title = page.getByRole('heading', { level: 2, name: 'Answers' });
        this.card = title.locator('..').locator('..'); // wrapper div -> card container
        this.table = this.card.locator('table.govuk-table');
        this.rows = this.table.locator('tbody tr');
    }

    // ===== Validations =====
    async assertLoaded(): Promise<void> {
        await expect(this.card).toBeVisible();
        await expect(this.table).toBeVisible();

        await expect(this.table.locator('th').nth(0)).toHaveText(/Answer/i);
        await expect(this.table.locator('th').nth(1)).toHaveText(/Priority/i);
        await expect(this.table.locator('th').nth(2)).toHaveText(/Target Type/i);
    }

    async verifyVisible(): Promise<void> {
        await expect(this.table).toBeVisible();
    }

    // ===== Row helpers =====
    private rowByAnswer(answer: string): Locator {
        return this.rows.filter({
            has: this.page.locator('td', { hasText: answer })
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
        column: 'Answer' | 'Priority' | 'Target Type'
    ): Promise<string> {
        const row = this.rowByAnswer(answer);
        const colIndex = column === 'Answer' ? 0 : column === 'Priority' ? 1 : 2;
        return (await row.locator('td').nth(colIndex).innerText()).trim();
    }

    // ===== Actions =====
    async openEdit(): Promise<void> {
        const editButton = this.card.getByRole('button', { name: /edit/i });
        await expect(editButton).toBeVisible();
        await editButton.click();
    }
}