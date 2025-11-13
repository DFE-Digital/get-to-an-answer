import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from "../BasePage";
import {ErrorMessages} from "../../constants/test-data-constants";

export class TermsOfUsePage extends BasePage {
    readonly heading: Locator;
    readonly agreeCheckbox: Locator;
    readonly checkboxLabel: Locator;
    readonly checkboxHint: Locator;
    readonly checkboxContainer: Locator;
    readonly saveAndContinueButton: Locator;
    readonly fieldset: Locator;
    readonly legend: Locator;
    readonly sectionHeadings: Locator;
    readonly securitySection: Locator;
    readonly dataProtectionSection: Locator;
    readonly makeFormsSection: Locator;
    readonly serviceStandardsSection: Locator;
    readonly changesSection: Locator;
    readonly errorSummary: Locator;
    readonly errorSummaryTitle: Locator;
    readonly errorList: Locator;
    readonly errorLinks: Locator;
    readonly acceptedErrorLink: Locator;
    //readonly inlineError: Locator;

    constructor(page: Page) {
        super(page);

        this.heading = page.locator('h1.govuk-heading-l');

        this.agreeCheckbox = page.locator('input.govuk-checkboxes__input#Accepted');
        this.checkboxContainer = page.locator('div.govuk-checkboxes__item');
        this.checkboxLabel = page.locator('label.govuk-label.govuk-checkboxes__label[for="Accepted"]');
        this.checkboxHint = page.locator('div.govuk-hint.govuk-checkboxes__hint');

        this.fieldset = page.locator('fieldset.govuk-fieldset');
        this.legend = this.fieldset.locator('legend.govuk-fieldset__legend');

        this.sectionHeadings = page.locator('main#main-content h2.govuk-heading-m');
        this.securitySection = this.sectionHeadings.nth(0);
        this.dataProtectionSection = this.sectionHeadings.nth(1);
        this.makeFormsSection = this.sectionHeadings.nth(2);
        this.serviceStandardsSection = this.sectionHeadings.nth(3);
        this.changesSection = this.sectionHeadings.nth(4);

        this.errorSummary = page.locator(
            'div.govuk-error-summary[role="alert"][data-module="govuk-error-summary"]'
        );
        this.errorSummaryTitle = this.errorSummary.locator(
            'h2.govuk-error-summary__title#error-summary-title'
        );

        this.errorList = this.errorSummary.locator('ul.govuk-error-summary__list');
        this.errorLinks = this.errorList.locator('a');
        this.acceptedErrorLink = this.errorList.locator('a[href="#Accepted"]');

        //this.inlineError = page.locator('.govuk-error-message'); // TBC

        this.saveAndContinueButton = page.locator('button.govuk-button');
    }

    // ===== Validations =====
    async verifyHeading() {
        await expect(this.heading, '❌ Heading not visible').toBeVisible();
    }

    async verifyTermsSections() {
        await expect(this.securitySection, '❌ Security section not visible').toBeVisible();
        await expect(this.dataProtectionSection, '❌ Data protection section not visible').toBeVisible();
        await expect(this.makeFormsSection, '❌ Make forms section not visible').toBeVisible();
        await expect(this.serviceStandardsSection, '❌ Service standards section not visible').toBeVisible();
        await expect(this.changesSection, '❌ Changes section not visible').toBeVisible();
    }

    async verifyFieldsetVisible() {
        await expect(this.fieldset, '❌ Fieldset not visible').toBeVisible();
        await expect(this.legend, '❌ Fieldset legend not visible').toBeVisible();
        await expect(this.agreeCheckbox, '❌ Checkbox not visible').toBeVisible();
        await expect(this.checkboxLabel, '❌ Checkbox label not visible').toBeVisible();
    }

    async validateErrorMessage() {
        await expect(this.errorSummaryTitle).toBeVisible();
        await expect(this.errorSummary).toBeVisible();
        await expect(this.errorSummary).toHaveAttribute('role', 'alert');
        await expect(this.errorSummary).toHaveAttribute('tabindex', '-1');
        await expect(this.errorSummary).toBeFocused();

        await expect(this.errorList).toContainText(ErrorMessages.ERROR_MESSAGE_TERMS_OF_USE);

        await this.acceptedErrorLink.click();
        await expect(this.agreeCheckbox).toBeFocused();
    }

    // TBC
    // async verifyInlineError() {
    //     await expect(this.inlineError).toBeVisible();
    // }

    async assertPageElements() {
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await this.verifyTermsSections();
        await expect(this.heading, '❌ Main heading not visible').toBeVisible();
        await expect(this.agreeCheckbox, '❌ Agree checkbox not visible').toBeVisible();
        await expect(this.saveAndContinueButton, '❌ Continue button not visible').toBeVisible();
    }

    // ===== Actions =====
    async acceptTerms() {
        await this.agreeCheckbox.setChecked(true);
        await expect(this.agreeCheckbox).toBeChecked();
    }

    async submit() {
        await this.saveAndContinueButton.click();
    }

    async submitWithoutAccepting() {
        await this.saveAndContinueButton.click();
    }

    async agreeToTermsOfUse() {
        await this.verifyHeading();
        await this.acceptTerms();
        await this.submit();
    }
}