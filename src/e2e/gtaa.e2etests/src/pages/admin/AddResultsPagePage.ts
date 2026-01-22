import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ErrorMessages} from "../../constants/test-data-constants";
import {EditAnswerTable} from "./components/EditAnswerTable";
import {Timeouts} from '../../constants/timeouts'

export class AddResultsPagePage extends BasePage {

    // ===== Locators =====
    private readonly form: Locator;
    private readonly addEditResultsPageHeading: Locator;
    private readonly backLink: Locator;
    private readonly resultsPageTitleInput: Locator;
    private readonly resultsPageDetailsText: Locator;
    private readonly resultsPageReferenceName: Locator;
    private readonly questionnaireTitleCaption: Locator;
    private readonly saveAndContinueButton: Locator;

    private readonly errorSummary: Locator;
    private readonly errorList: Locator;
    private readonly errorLinks: Locator;
    private readonly errorSummaryContentTitle: Locator;
    private readonly errorSummaryContentValue: Locator;
    private readonly errorSummaryReferenceName: Locator;
    
    private readonly errorInlineContentTitle: Locator;
    private readonly errorInlineContentValue: Locator;
    private readonly errorInlineReferenceName: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.form = this.page.locator('form[method="post"]');
        this.addEditResultsPageHeading = this.page.locator('#questionnaire-results-page-heading');
        this.questionnaireTitleCaption = page.locator('#current-questionnaire-title');
        this.backLink = this.page.locator('a.govuk-back-link');

        this.resultsPageTitleInput = this.form.locator('#ContentTitle');
        this.resultsPageDetailsText = this.form.locator('#ContentValue');
        this.resultsPageReferenceName = this.form.locator('#ContentRefName');

        // Save button
        this.saveAndContinueButton = page.locator('#save-and-continue');
        
        // Error summary
        this.errorSummary = this.page.locator('.govuk-error-summary[role="alert"][tabindex="-1"]');
        this.errorList = this.errorSummary.locator('ul.govuk-error-summary__list');
        this.errorLinks = this.errorList.locator('a');
        this.errorSummaryContentTitle = this.errorSummaryLink('#ContentTitle');
        this.errorSummaryContentValue = this.errorSummaryLink('#ContentValue');
        this.errorSummaryReferenceName = this.errorSummaryLink('#ContentRefName');
        
        // Inline error messages
        this.errorInlineContentTitle = this.inlineErrorLink('contenttitle-error');
        this.errorInlineContentValue = this.inlineErrorLink('contentvalue-error');
        this.errorInlineReferenceName = this.inlineErrorLink('ContentRefName-error');
    }
    
    // ===== Validation =====

    async expectAddResultsPageHeadingOnPage(expectedText?: string): Promise<void> {
        await this.addEditResultsPageHeading.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.addEditResultsPageHeading, '❌ Add results-page heading not visible').toBeVisible();

        if (expectedText) {
            await expect(
                this.addEditResultsPageHeading,
                `❌ Add results-page heading text mismatch: expected "${expectedText}"`
            ).toContainText(expectedText);
        }
    }

    async assertPageElements() {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();
        await this.verifyBackLinkPresent();
        await this.VerifyTitleDetailsAndRefName();
        await expect(this.form, '❌ Form not visible').toBeVisible();
    }

    async verifyBackLinkPresent(): Promise<void> {
        await expect(this.backLink, '❌ Back link not visible').toBeVisible();
    }

    async validateMissingAllFieldsErrorMessageSummary(browserName: string) {

        await this.errorSummary.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});
        await this.errorList.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});

        await this.page.waitForTimeout(200);  // Increase to 200ms for Firefox
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        await this.clickAllLinksAndValidateFocus(browserName);
    }

    async validateMissingTitleErrorMessageSummary(browserName: string) {
        await this.errorSummary.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});
        await this.errorList.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_RESULTS_PAGE_TITLE);

        await this.page.waitForTimeout(200);
        await this.clickErrorLinkAndValidateFocus(this.errorSummaryContentTitle, browserName);
    }

    async validateMissingDetailsErrorMessageSummary(browserName: string) {
        await this.errorSummary.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});
        await this.errorList.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_MISSING_RESULTS_PAGE_DETAILS);

        await this.page.waitForTimeout(200);
        await this.clickErrorLinkAndValidateFocus(this.errorSummaryContentValue, browserName);
    }

    async VerifyTitleDetailsAndRefName(): Promise<void> {
        await expect(this.resultsPageTitleInput, '❌ Result page title input not visible').toBeVisible();
        await expect(this.resultsPageDetailsText, '❌ Results page details textarea not visible').toBeVisible();
        await expect(this.resultsPageReferenceName, '❌ Results page reference name input not visible').toBeVisible();
    }

    async validateInlineTitleError(): Promise<void> {
        await this.errorInlineContentTitle.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});
        await expect(this.errorInlineContentTitle, '❌ Inline results page title error not visible').toBeVisible();
    }

    async validateInlineDetailsError(): Promise<void> {
        await this.errorInlineContentValue.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});
        await expect(this.errorInlineContentValue, '❌ Inline results page details error not visible').toBeVisible();
    }

    async validateInlineReferenceNameError(): Promise<void> {
        await this.errorInlineReferenceName.waitFor({state: 'visible', timeout: Timeouts.MEDIUM});
        await expect(this.errorInlineReferenceName, '❌ Inline results page ref name error not visible').toBeVisible();
    }
    
    async assertStartPageFieldsEmpty(): Promise<void> {
        await this.resultsPageTitleInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.resultsPageTitleInput).toHaveValue('');
        await this.resultsPageDetailsText.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.resultsPageDetailsText).toHaveValue('');
    }

    async assertStartPageFields(expectedContentTitle: string, expectedContentValue: string): Promise<void> {
        await this.resultsPageTitleInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.resultsPageTitleInput).toHaveValue(expectedContentTitle);
        await this.resultsPageDetailsText.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.resultsPageDetailsText).toHaveValue(expectedContentValue);
    }

    async assertErrorSummaryLinkForContentTitle(): Promise<void> {
        await this.errorSummaryContentTitle.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.errorSummaryContentTitle).toHaveText(ErrorMessages.ERROR_MESSAGE_DISPLAY_TITLE_REQUIRED);
    }

    async assertErrorSummaryLinkForContentValue(): Promise<void> {
        await this.errorSummaryContentValue.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.errorSummaryContentValue).toHaveText(ErrorMessages.ERROR_MESSAGE_QUESTIONNAIRE_DESCRIPTION_REQUIRED);
    }
    
    async assertErrorSummaryLinkForReferenceName(): Promise<void> {
        await this.errorSummaryReferenceName.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.errorSummaryReferenceName).toHaveText(ErrorMessages.ERROR_MESSAGE_QUESTIONNAIRE_DESCRIPTION_REQUIRED);
    } 

    async assertInlineErrorForContentTitle(): Promise<void> {
        await this.errorInlineContentTitle.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.errorInlineContentTitle).toContainText(ErrorMessages.ERROR_MESSAGE_DISPLAY_TITLE_REQUIRED);
    }

    async assertInlineErrorForContentValue(): Promise<void> {
        await this.errorInlineContentValue.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.errorInlineContentValue).toContainText(ErrorMessages.ERROR_MESSAGE_QUESTIONNAIRE_DESCRIPTION_REQUIRED);
    }

    async assertInlineErrorForReferenceName(): Promise<void> {
        await this.errorInlineReferenceName.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await expect(this.errorInlineReferenceName).toContainText(ErrorMessages.ERROR_MESSAGE_QUESTIONNAIRE_DESCRIPTION_REQUIRED);
    }
    
    // ===== Actions =====

    async clickBackLink(): Promise<void> {
        await Promise.all([
            this.backLink.click(),
            await this.waitForPageLoad(),
        ]);
    }

    async enterResultsPageTitleInput(text: string): Promise<void> {
        await this.resultsPageTitleInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.resultsPageTitleInput.clear();
        await this.resultsPageTitleInput.fill(text);
    }

    async clearResultsPageTitleInput(): Promise<void> {
        await this.resultsPageTitleInput.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.resultsPageTitleInput.clear();
    }

    async enterResultsPageDetailsText(text: string): Promise<void> {
        await this.resultsPageDetailsText.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.resultsPageDetailsText.clear();
        await this.resultsPageDetailsText.fill(text);
    }

    async clearResultsPageDetailsText(): Promise<void> {
        await this.resultsPageDetailsText.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.resultsPageDetailsText.clear();
    }

    async enterResultsPageRefNameInput(text: string): Promise<void> {
        await this.resultsPageReferenceName.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.resultsPageReferenceName.clear();
        await this.resultsPageReferenceName.fill(text);
    }

    async clearResultsPageRefNameInput(): Promise<void> {
        await this.resultsPageReferenceName.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.resultsPageReferenceName.clear();
    }

    async enterInvalidResultsPageTitle(): Promise<void> {
        await this.resultsPageTitleInput.clear();
        await this.resultsPageTitleInput.fill(`${' '.repeat(10)}`);
    }

    async clickSaveAndContinue(): Promise<void> {
        await this.saveAndContinueButton.waitFor({state: 'visible', timeout: Timeouts.LONG});
        await this.saveAndContinueButton.click();
        await this.waitForPageLoad();
    }
    
    async clickAllLinksAndValidateFocus(browserName: string): Promise<void> {
        const linkCount = await this.errorLinks.count();

        for (let i = 0; i < linkCount; i++) {
            const link = this.errorLinks.nth(i);
            const href = await link.getAttribute('href');
            const targetId = href?.replace('#', '');

            await link.click();

            if (browserName !== 'webkit' && targetId) {
                const targetInput = this.page.locator(`#${targetId}`);
                await expect(targetInput, `❌ Target input for link ${i + 1} not focused`).toBeFocused();
            }
        }
    }



    async clickErrorLinkAndValidateFocus(link: Locator, browserName: string): Promise<void> {
        await expect(this.errorSummary, '❌ Error summary missing').toBeVisible();
        await expect(this.errorSummary, '❌ Attribute role is missing').toHaveAttribute('role', 'alert');
        await expect(this.errorSummary, '❌ Attribute tabIndex is missing').toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary, '❌ Error summary not focused').toBeFocused();

        const href = await link.getAttribute('href');
        const targetId = href?.replace('#', '');

        await link.click();

        if (browserName !== 'webkit' && targetId) {
            const targetInput = this.page.locator(`#${targetId}`);
            await expect(targetInput, '❌ Target input not focused').toBeFocused();
        }
    }
}