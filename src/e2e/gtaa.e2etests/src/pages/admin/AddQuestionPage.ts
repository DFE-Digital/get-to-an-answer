import { expect, Locator, Page } from '@playwright/test';
import {BasePage} from "../BasePage";
import {ViewQuestionTable} from "./components/ViewQuestionTable";
import {EditAnswerTable} from "./components/EditAnswerTable";

export enum QuestionType {
    SmallList = 'One option from a small list',
    LongList = 'One option from a long list',
    Multiple = 'One or more options',
}

export class AddQuestionPage extends BasePage{
    
    // Form locators
    readonly backLink = this.page.getByRole('link', { name: /Back to questionnaire questions/i });
    readonly heading: Locator = this.page.getByRole('heading', { name: /(Add|Edit) question/ });
    readonly questionText: Locator = this.page.getByLabel('Question text');
    readonly hintText: Locator = this.page.getByLabel('Hint text (optional)');
    readonly smallListRadio: Locator = this.page.getByRole('radio', { name: QuestionType.SmallList });
    readonly longListRadio: Locator = this.page.getByRole('radio', { name: QuestionType.LongList });
    readonly multipleRadio: Locator = this.page.getByRole('radio', { name: QuestionType.Multiple });
    readonly saveBtn: Locator = this.page.getByRole('button', { name: /(Create|Save) question/ });
    readonly deleteBtn: Locator = this.page.getByRole('button', { name: 'Delete question' });

    readonly table: EditAnswerTable;
    constructor(page: Page) {
        super(page);
        this.table = new EditAnswerTable(page);
    }
    
    // --- Validations (form presence only) ---
    async assertLoaded(): Promise<void> {
        await expect(this.heading).toBeVisible();
        await expect(this.questionText).toBeVisible();
        await expect(this.hintText).toBeVisible();
        await expect(this.smallListRadio).toBeVisible();
        await expect(this.longListRadio).toBeVisible();
        await expect(this.multipleRadio).toBeVisible();
        await expect(this.saveBtn).toBeVisible();
        await this.table.verifyVisible();
    }

    // --- Actions (form only) ---
    async enterQuestion(text: string): Promise<void> {
        await this.questionText.fill(text);
    }

    async enterHint(text: string): Promise<void> {
        await this.hintText.fill(text);
    }

    async chooseType(type: QuestionType): Promise<void> {
        if (type === QuestionType.SmallList) await this.smallListRadio.check();
        else if (type === QuestionType.LongList) await this.longListRadio.check();
        else await this.multipleRadio.check();
    }

    async save(): Promise<void> {
        await this.saveBtn.click();
    }

    async delete(): Promise<void> {
        await this.deleteBtn.click();
    }
}