import { expect, Locator, Page } from '@playwright/test';
import { BasePage } from '../BasePage';
import { Timeouts } from '../../constants/timeouts';
import path from "path";

export class QuestionnaireStylingPage extends BasePage {
    // ===== Locators =====
    private readonly form: Locator;
    private readonly backLink: Locator;

    // Colour inputs
    private readonly textColorInput: Locator;
    private readonly backgroundColorInput: Locator;
    private readonly primaryButtonColorInput: Locator;
    private readonly secondaryButtonColorInput: Locator;
    private readonly stateColorInput: Locator;
    private readonly errorMessageColorInput: Locator;

    // Decorative image
    private readonly decorativeImageInput: Locator;
    private readonly removeImageButton: Locator;

    // Reset styling
    private readonly resetStylingButton: Locator;

    // Accessibility agreement
    private readonly accessibilityAgreementCheckbox: Locator;

    // Save
    private readonly saveAndContinueButton: Locator;

    // Notifications
    private readonly stylingUpdatedBannerText: Locator;

    // Error summary links (selectors align with the input IDs used in the .cshtml)
    private readonly errorSummaryTextColor: Locator;
    private readonly errorSummaryBackgroundColor: Locator;
    private readonly errorSummaryPrimaryButtonColor: Locator;
    private readonly errorSummarySecondaryButtonColor: Locator;
    private readonly errorSummaryStateColor: Locator;
    private readonly errorSummaryErrorMessageColor: Locator;
    private readonly errorSummaryAccessibilityAgreement: Locator;

    // Inline error messages (error-id values from the .cshtml)
    private readonly inlineErrorTextColor: Locator;
    private readonly inlineErrorBackgroundColor: Locator;
    private readonly inlineErrorPrimaryButtonColor: Locator;
    private readonly inlineErrorSecondaryButtonColor: Locator;
    private readonly inlineErrorStateColor: Locator;
    private readonly inlineErrorErrorMessageColor: Locator;
    private readonly inlineErrorAccessibilityAgreement: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.form = this.page.locator('form[method="post"]');
        this.backLink = this.page.locator('a.govuk-back-link');

        // Colour inputs (IDs taken from QuestionnaireCustomStyling.cshtml)
        this.textColorInput = this.form.locator('#text-color');
        this.backgroundColorInput = this.form.locator('#background-color');
        this.primaryButtonColorInput = this.form.locator('#primary-button-color');
        this.secondaryButtonColorInput = this.form.locator('#secondary-button-color');
        this.stateColorInput = this.form.locator('#state-color');
        this.errorMessageColorInput = this.form.locator('#error-color');

        // Decorative image
        this.decorativeImageInput = this.form.locator('#decorative-image-upload-input');
        this.removeImageButton = this.form.locator('button[name="RemoveImage"]');

        // Reset styling
        this.resetStylingButton = this.form.locator('#reset-styling');

        // Accessibility agreement checkbox
        this.accessibilityAgreementCheckbox = this.form.locator('#UpdateRequest-IsAccessibilityAgreementAccepted');

        // Save & continue (primary submit button in the form)
        this.saveAndContinueButton = this.form.locator('#save-and-continue');

        // Notification banner when styling is updated
        this.stylingUpdatedBannerText = this.page.locator('#just-updated-styling-banner-text');

        // Error summary links
        this.errorSummaryTextColor = this.errorSummaryLink('#UpdateRequest-TextColor');
        this.errorSummaryBackgroundColor = this.errorSummaryLink('#background-color');
        this.errorSummaryPrimaryButtonColor = this.errorSummaryLink('#primary-button-color');
        this.errorSummarySecondaryButtonColor = this.errorSummaryLink('#secondary-button-color');
        this.errorSummaryStateColor = this.errorSummaryLink('#state-color');
        this.errorSummaryErrorMessageColor = this.errorSummaryLink('#error-color');
        this.errorSummaryAccessibilityAgreement = this.errorSummaryLink(
            '#UpdateRequest-IsAccessibilityAgreementAccepted'
        );

        // Inline error messages (use the error-id values from the .cshtml)
        this.inlineErrorTextColor = this.inlineErrorLink('text-color-error');
        this.inlineErrorBackgroundColor = this.inlineErrorLink('background-color-error');
        this.inlineErrorPrimaryButtonColor = this.inlineErrorLink('primary-button-color-error');
        this.inlineErrorSecondaryButtonColor = this.inlineErrorLink('secondary-button-color-error');
        this.inlineErrorStateColor = this.inlineErrorLink('state-color-error');
        this.inlineErrorErrorMessageColor = this.inlineErrorLink('error-color-error');
        this.inlineErrorAccessibilityAgreement = this.inlineErrorLink(
            'forms-name-input-name-field-error'
        );
    }

    // ===== Validation helpers =====

    async expectOnStylingPage(): Promise<void> {
        await this.form.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        await expect(this.textColorInput).toBeVisible();
        await expect(this.backgroundColorInput).toBeVisible();
        await expect(this.primaryButtonColorInput).toBeVisible();
        await expect(this.secondaryButtonColorInput).toBeVisible();
        await expect(this.stateColorInput).toBeVisible();
        await expect(this.errorMessageColorInput).toBeVisible();
        await expect(this.saveAndContinueButton).toBeVisible();
    }

    async expectStylingUpdatedBannerVisible(): Promise<void> {
        await expect(this.stylingUpdatedBannerText).toBeVisible();
    }

    async expectErrorSummaryForTextColor(message: string): Promise<void> {
        await expect(this.errorSummaryTextColor).toHaveText(message);
    }

    async expectInlineErrorForTextColor(message: string): Promise<void> {
        await expect(this.inlineErrorTextColor).toContainText(message);
    }

    // Similar helpers can be used for the other fields when needed:
    async expectErrorSummaryForAccessibilityAgreement(message: string): Promise<void> {
        await expect(this.errorSummaryAccessibilityAgreement).toHaveText(message);
    }

    async expectInlineErrorForAccessibilityAgreement(message: string): Promise<void> {
        await expect(this.inlineErrorAccessibilityAgreement).toContainText(message);
    }

    // ===== Actions =====

    async clickBackLink(): Promise<void> {
        await Promise.all([
            this.backLink.click(),
            this.waitForPageLoad()
        ]);
    }

    async setTextColor(value: string): Promise<void> {
        await this.textColorInput.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        await this.textColorInput.clear();
        await this.textColorInput.fill(value);
    }

    async setBackgroundColor(value: string): Promise<void> {
        await this.backgroundColorInput.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        await this.backgroundColorInput.clear();
        await this.backgroundColorInput.fill(value);
    }

    async setPrimaryButtonColor(value: string): Promise<void> {
        await this.primaryButtonColorInput.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        await this.primaryButtonColorInput.clear();
        await this.primaryButtonColorInput.fill(value);
    }

    async setSecondaryButtonColor(value: string): Promise<void> {
        await this.secondaryButtonColorInput.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        await this.secondaryButtonColorInput.clear();
        await this.secondaryButtonColorInput.fill(value);
    }

    async setStateColor(value: string): Promise<void> {
        await this.stateColorInput.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        await this.stateColorInput.clear();
        await this.stateColorInput.fill(value);
    }

    async setErrorMessageColor(value: string): Promise<void> {
        await this.errorMessageColorInput.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        await this.errorMessageColorInput.clear();
        await this.errorMessageColorInput.fill(value);
    }

    async getTextColorValue(): Promise<string> {
        return await this.textColorInput.inputValue();
    }

    async getBackgroundColorValue(): Promise<string> {
        return await this.backgroundColorInput.inputValue();
    }

    async getPrimaryButtonColorValue(): Promise<string> {
        return await this.primaryButtonColorInput.inputValue();
    }

    async getSecondaryButtonColorValue(): Promise<string> {
        return await this.secondaryButtonColorInput.inputValue();
    }

    async getStateColorValue(): Promise<string> {
        return await this.stateColorInput.inputValue();
    }

    async getErrorMessageColorValue(): Promise<string> {
        return await this.errorMessageColorInput.inputValue();
    }


    async uploadDecorativeImage(filePath: string): Promise<void> {
        await this.decorativeImageInput.setInputFiles((path.join(__dirname, filePath)));
    }

    async clickRemoveImage(): Promise<void> {
        await this.removeImageButton.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        await this.removeImageButton.click();
        await this.waitForPageLoad();
    }

    async clickResetStyling(): Promise<void> {
        await this.resetStylingButton.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        await this.resetStylingButton.click();
        await this.waitForPageLoad();
    }

    async acceptAccessibilityAgreement(): Promise<void> {
        await this.accessibilityAgreementCheckbox.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        const isChecked = await this.accessibilityAgreementCheckbox.isChecked();
        if (!isChecked) {
            await this.accessibilityAgreementCheckbox.check();
        }
    }

    async saveAndContinue(): Promise<void> {
        await this.saveAndContinueButton.waitFor({ state: 'visible', timeout: Timeouts.LONG });
        await this.saveAndContinueButton.click();
        await this.waitForPageLoad();
    }

    async fillValidStylingAndSubmit(options: {
        textColor: string;
        backgroundColor: string;
        primaryButtonColor: string;
        secondaryButtonColor: string;
        stateColor: string;
        errorMessageColor: string;
    }): Promise<void> {
        await this.setTextColor(options.textColor);
        await this.setBackgroundColor(options.backgroundColor);
        await this.setPrimaryButtonColor(options.primaryButtonColor);
        await this.setSecondaryButtonColor(options.secondaryButtonColor);
        await this.setStateColor(options.stateColor);
        await this.setErrorMessageColor(options.errorMessageColor);

        await this.acceptAccessibilityAgreement();
        await this.saveAndContinue();
    }
}