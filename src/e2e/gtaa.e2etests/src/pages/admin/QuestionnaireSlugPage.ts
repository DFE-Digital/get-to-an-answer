// import {expect, Page} from '@playwright/test';
// import {BasePage} from '../BasePage';
//
// type Mode = 'create' | 'edit';
//
// export class QuestionnaireSlugPage extends BasePage {
//     // ===== Locators =====
//     private readonly form = this.page.locator(
//         'form[action*="/questionnaires/"][method="post"]'
//     );
//
//     private readonly slugInput = this.page.locator(
//         'input#forms-name-input-name-field[name="slug"][type="text"]'
//     );
//
//
//     private readonly saveAndContinueButton = this.form.locator(
//         'button.govuk-button[type="submit"]'
//     );
//
//
//     private readonly slugLabel = this.page.locator(
//         'label[for="forms-name-input-name-field"]'
//     );
//     private readonly slugHint = this.page.locator(
//         '#forms-name-input-name-hint'
//     );
//
//     // ===== Constructor =====
//     constructor(page: Page, mode: Mode = 'create') {
//         super(page);
//     }
//
//     // ----- Actions -----
//     async enterSlug(slug: string): Promise<void> {
//         await this.slugInput.fill(slug);
//     }
//
//     async clickSaveAndContinue(): Promise<void> {
//         await this.saveAndContinueButton.click();
//     }
//
//     async createSlug(slug: string): Promise<void> {
//         await this.enterSlug(slug);
//         await this.clickSaveAndContinue();
//     }
//
//     // ----- Validations (structure-only) -----
//     async verifyOnQuestionnaireSlugPage(): Promise<void> {
//
//         // URL structure; no wording validation
//         await expect(this.page).toHaveURL(/\/questionnaires\/[^/]+\/edit/);
//
//         await expect(this.form).toBeVisible();
//         await expect(this.slugInput).toBeVisible();
//         await expect(this.saveAndContinueButton).toBeVisible();
//     }
//
//     async verifyLabelAndHintPresent(): Promise<void> {
//         await expect(this.slugLabel).toBeVisible();
//         await expect(this.slugHint).toBeVisible();
//     }
// }