import {expect, Locator, Page} from '@playwright/test';

export class EditAnswerTable {
    private readonly root: Locator;
    private readonly section: Locator;
    private readonly editLink: Locator;

    constructor(private page: Page) {
        // Scope: the "Answers" section only
        this.root = page.locator('table');
        this.section = this.page.getByRole('region', {name: 'Answers'});
        this.editLink = this.section.getByRole('link', {name: 'Edit'});
    }

    // --- Validations ---
    async assertLoaded(): Promise<void> {
        await expect(this.section).toBeVisible();
        await expect(this.root).toBeVisible();
        await expect(this.root.getByRole('columnheader', {name: 'Answer'})).toBeVisible();
        await expect(this.root.getByRole('columnheader', {name: 'Score'})).toBeVisible();
        await expect(this.root.getByRole('columnheader', {name: 'Target Type'})).toBeVisible();
    }

    async verifyVisible(): Promise<void> {
        await expect(this.root).toBeVisible();
    }
    
    // --- Helpers for rows/cells ---
    private rowByAnswer(answer: string): Locator {
        return this.root.getByRole('row', {name: new RegExp(`^${answer}\\b`, 'i')});
    }

    async getRowCount(): Promise<number> {
        return await this.root.getByRole('row').filter({has: this.root.getByRole('cell')}).count();
    }

    async assertRowPresent(answer: string): Promise<void> {
        await expect(this.rowByAnswer(answer)).toBeVisible();
    }

    async getCellText(answer: string, column: 'Answer' | 'Score' | 'Target Type'): Promise<string> {
        const row = this.rowByAnswer(answer);
        const cell =
            column === 'Answer'
                ? row.getByRole('cell').first()
                : row.getByRole('cell', {name: /\S/}).nth(column === 'Score' ? 1 : 2);
        return (await cell.textContent())?.trim() ?? '';
    }

    // --- Actions inside the table section ---
    async openEdit(): Promise<void> {
        await this.editLink.click();
    }
}