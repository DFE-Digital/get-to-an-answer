import { Page, Locator, expect } from '@playwright/test';
import {BasePage} from "../BasePage";

type Mode = 'create' | 'edit';

export class CreateAnswerOptionsPage extends BasePage{
    
    constructor(page: Page, mode: Mode = 'create') {
        super(page);
    }

    // ---- Core locators (0-based option index) ----
    private optionContent(i: number): Locator {
        return this.page.locator(`input[name="Answers[${i}].Content"]`);
    }
    private optionDescription(i: number): Locator {
        return this.page.locator(`textarea[name="Answers[${i}].Description"]`);
    }
    private optionScore(i: number): Locator {
        return this.page.locator(`input[name="Answers[${i}].Score"]`);
    }
    private destinationRadio(i: number, value: 'Question' | 'ExternalLink' | 'InternalPage' | '0'): Locator {
        return this.page.locator(`input[name="Answers[${i}].DestinationType"][value="${value}"]`);
    }
    private externalLinkInput(i: number): Locator {
        // Any visible text/url input inside the ExternalLink conditional container
        return this.page.locator(`#conditional-e-answer-${i} input[type="text"], #conditional-e-answer-${i} input[type="url"]`).first();
    }
    private removeButtonFor(i: number): Locator {
        // Button[name=remove] inside the nth option item
        return this.page.locator(`ul.app-select-options__list > li.app-select-options__item >> nth=${i}`)
            .locator(`button[name="remove"]`);
    }

    // ---- Global controls ----
    private addAnotherOptionBtn(): Locator {
        return this.page.locator(`button.govuk-button.govuk-button--secondary[name="AddEmptyAnswerOption"]`);
    }
    private saveAndContinueBtn(): Locator {
        // Text is reliable on GOV.UK buttons
        return this.page.getByRole('button', { name: 'Save and continue' });
    }
    private bulkOptionsLink(): Locator {
        return this.page.locator(`a.govuk-link[href$="/bulk-options"]`);
    }

    // ---- Actions ----
    async expectOnPage() {
        await expect(this.page.getByRole('heading', { name: 'Create a list of answer options' })).toBeVisible();
    }

    async setOptionContent(i: number, text: string) {
        await this.optionContent(i).fill(text);
    }

    async setOptionDescription(i: number, text: string) {
        await this.optionDescription(i).fill(text);
    }

    async setOptionScore(i: number, score: number) {
        await this.optionScore(i).fill(String(score));
    }

    /** value: "Question" | "ExternalLink" | "InternalPage" | "0" (None) */
    async chooseDestination(i: number, value: 'Question' | 'ExternalLink' | 'InternalPage' | '0') {
        await this.destinationRadio(i, value).check();
    }

    async setExternalLink(i: number, url: string) {
        await this.chooseDestination(i, 'ExternalLink');
        await this.externalLinkInput(i).fill(url);
    }

    /** Clicks per-option Remove (only present for existing options). */
    async removeOption(i: number) {
        await this.removeButtonFor(i).click();
    }

    /** Adds a new option section (covers the initial “Add” / “Add another option” button). */
    async addAnotherOption() {
        await this.addAnotherOptionBtn().click();
    }

    async saveAndContinue() {
        await this.saveAndContinueBtn().click();
    }

    async openBulkOptions() {
        await this.bulkOptionsLink().click();
    }
}