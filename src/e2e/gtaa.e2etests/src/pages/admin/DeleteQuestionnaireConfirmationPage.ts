// import { expect, Locator, Page, APIResponse } from '@playwright/test';
//
// export class DeleteQuestionnaireConfirmationPage {
//     readonly page: Page;
//
//     // ----- Core UI -----
//     readonly backLink: Locator;                
//     readonly form: Locator;                    
//     readonly continueButton: Locator;          
//     readonly radiosFormGroup: Locator;         
//     readonly radios: Locator;                  
//
//     // ----- Validation UI -----
//     readonly errorSummary: Locator;            
//     readonly errorSummaryLinks: Locator;       
//     readonly inlineError: Locator;            
//
//     constructor(page: Page) {
//         this.page = page;
//
//         this.backLink = page.locator('a.govuk-back-link');
//         this.form = page.locator('main form[method="post"]');
//
//         this.continueButton = this.form.locator('button.govuk-button[type="submit"]');
//
//         // Radios group 
//         this.radiosFormGroup = this.form.locator('.govuk-form-group:has(.govuk-radios)');
//         this.radios = this.form.locator('.govuk-radios input[type="radio"]');
//
//         // Validation 
//         this.errorSummary = page.locator('.govuk-error-summary[role="alert"][tabindex="-1"]');
//         this.errorSummaryLinks = this.errorSummary.locator('.govuk-error-summary__list a');
//         this.inlineError = this.radiosFormGroup.locator('.govuk-error-message');
//     }
//
//     // ===== Actions =====
//     async clickBackLink() {
//         await this.backLink.click();
//     }
//
//     async chooseYes() { await this.radios.nth(0).check(); }
//     async chooseNo()  { await this.radios.nth(1).check(); }
//
//     async clickContinue() {
//         await this.continueButton.click();
//     }
//
//     // ===== Validations =====
//     async expectBackLinkContainsTitle(title: string) {
//         await expect(this.backLink, '❌ Back link not visible').toBeVisible();
//         const text = await this.backLink.innerText();
//         expect(text, '❌ Back link does not include questionnaire title')
//             .toContain(title);
//     }
//
//     async expectRequiredChoiceValidation() {
//         // submit without selecting a radio
//         await this.clickContinue();
//
//         // Error summary appears with role="alert" and tabindex="-1"
//         await expect(this.errorSummary, '❌ Error summary not visible').toBeVisible();
//
//         // summary contains at least one link that points back to the radios group
//         await expect(this.errorSummaryLinks, '❌ Error summary does not contain links')
//             .toHaveCount(1);
//
//         // Inline error above radios
//         await expect(this.inlineError, '❌ Inline error not visible above radios').toBeVisible();
//
//         // form-group has error class
//         const classes = await this.radiosFormGroup.getAttribute('class');
//         expect(classes ?? '', '❌ Radios form-group missing .govuk-form-group--error')
//             .toMatch(/\bgovuk-form-group--error\b/);
//     }
//
//     async chooseYesAndSubmitWaitForDelete(endpointSubstring: string) {
//         await this.chooseYes();
//         const [request] = await Promise.all([
//             this.page.waitForRequest(
//                 req => req.method() === 'POST' && req.url().includes(endpointSubstring)
//             ),
//             this.clickContinue(),
//         ]);
//         expect(request.url(), '❌ Form did not post to expected delete endpoint')
//             .toContain(endpointSubstring);
//     }
//
//     async chooseNoAndSubmitReturnToTrack(trackUrlSubstring?: string) {
//         await this.chooseNo();
//         if (trackUrlSubstring) {
//             await Promise.all([
//                 this.page.waitForURL((url) => url.toString().includes(trackUrlSubstring)),
//                 this.clickContinue(),
//             ]);
//         } else {
//             await this.clickContinue();
//         }
//     }
//
//     async submitAndReturnSelectedIndex(): Promise<number> {
//         const selectedIndex =
//             (await this.radios.nth(0).isChecked()) ? 0 :
//                 (await this.radios.nth(1).isChecked()) ? 1 : -1;
//
//         await this.clickContinue();
//         return selectedIndex;
//     }
//
//     async expectTwoRadiosPresent() {
//         await expect(this.radiosFormGroup, '❌ Radios form-group not visible').toBeVisible();
//         await expect(this.radios, '❌ Radios count is not 2').toHaveCount(2);
//     }
// }