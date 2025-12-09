import {expect, Page, Locator} from '@playwright/test';
import {BasePage} from '../BasePage';
import {Timeouts} from "../../constants/timeouts";

export class DesignQuestionnairePage extends BasePage {
    // ===== Locators =====
    private readonly main: Locator;
    private readonly banner: Locator;
    private readonly heading: Locator;
    private readonly linkBackToQuestionnaires: Locator;
    private readonly questionnaireTitle: Locator;
    private readonly editQuestionnaireHeading: Locator;
    private readonly questionnaireStatus: Locator;
    private readonly linkEditTitle: Locator;
    private readonly linkEditQuestionnaireId: Locator;
    private readonly linkManageAccess: Locator;
    private readonly linkAddEditQuestionsAnswers: Locator;
    private readonly linkAnswerContent: Locator;
    private readonly linkStartPage: Locator;
    private readonly linkEditButtonText: Locator;
    private readonly linkCustomiseStyling: Locator;
    private readonly linkBranchingMap: Locator;
    private readonly btnPublish: Locator;
    private readonly btnUnpublish: Locator;
    private readonly deleteQuestionnaireButton: Locator;
    private readonly linkViewVersions: Locator;
    private readonly linkClone: Locator;

    // Notification banner text locators
    private readonly justCreatedBannerText: Locator;
    private readonly justUpdatedBannerText: Locator;
    private readonly justClonedBannerText: Locator;
    private readonly justPublishedBannerText: Locator;
    private readonly justUnpublishedBannerText: Locator;
    private readonly justAddedStartPageBannerText: Locator;
    private readonly justRemovedStartPageBannerText: Locator;
    private readonly justResetStylingBannerText: Locator;
    private readonly justUpdatedStylingBannerText: Locator;
    private readonly justModifiedButtonTextBannerText: Locator;
    private readonly justRemovedStartPageImageBannerText: Locator;
    private readonly gtaaApiErrorBannerLabel: Locator;
    private readonly gtaaApiErrorBannerText: Locator;
    
    constructor(page: Page) {
        super(page);
        this.main = this.page.locator('main.govuk-main-wrapper[role="main"]');
        this.banner = this.page.locator('div.govuk-notification-banner--success[role="alert"]');
        this.heading = this.banner.locator('.govuk-notification-banner__title');
        this.questionnaireTitle = this.page.locator('#current-questionnaire-title');
        this.editQuestionnaireHeading = this.page.locator('#track-questionnaire-page-heading');
        this.questionnaireStatus = this.page.locator('#questionnaire-status');
        
        this.linkEditTitle = this.page.locator('#edit-questionnaire-name');
        this.linkBackToQuestionnaires = this.page.locator('#back-to-manage');
        this.linkEditQuestionnaireId = this.page.locator('#edit-questionnaire-id');
        this.linkManageAccess = this.page.locator('#manage-access');
        this.linkStartPage = this.page.locator('#edit-start-page');
        this.linkAddEditQuestionsAnswers = page.locator('#edit-questions');
        this.linkEditButtonText = this.page.locator('#edit-button-text');
        this.linkCustomiseStyling = this.page.locator('#customise-styling');
        this.linkBranchingMap = this.page.locator('#view-branching-map');
        this.btnPublish = this.page.locator('#publish-questionnaire');
        this.btnUnpublish = this.page.locator('#unpublish-questionnaire');
        this.deleteQuestionnaireButton = page.locator('#delete-questionnaire');
        this.linkViewVersions = this.page.locator('#view-version-history');
        this.linkClone = this.page.locator('#make-copy');
        this.linkAnswerContent = page.locator('#edit-results-pages');
        
        // Notification banner text locators
        this.justCreatedBannerText = this.page.locator('#just-created-banner-text');
        this.justUpdatedBannerText = this.page.locator('#just-updated-banner-text');
        this.justClonedBannerText = this.page.locator('#just-cloned-banner-text');
        this.justPublishedBannerText = this.page.locator('#just-published-banner-text');
        this.justUnpublishedBannerText = this.page.locator('#just-unpublished-banner-text');
        this.justAddedStartPageBannerText = this.page.locator('#just-added-start-page-banner-text');
        this.justRemovedStartPageBannerText = this.page.locator('#just-removed-start-page-banner-text');
        this.justResetStylingBannerText = this.page.locator('#just-reset-styling-banner-text');
        this.justUpdatedStylingBannerText = this.page.locator('#just-updated-styling-banner-text');
        this.justModifiedButtonTextBannerText = this.page.locator('#just-modified-button-text-banner-text');
        this.justRemovedStartPageImageBannerText = this.page.locator('#just-removed-start-page-image-banner-text');
        this.gtaaApiErrorBannerLabel = this.page.locator('#gtaa-api-error-banner-label');
        this.gtaaApiErrorBannerText = this.page.locator('#gtaa-api-error-banner-text');
    }

    // ===== Actions =====
    async ClickBackToQuestionnaireLink(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.linkBackToQuestionnaires.click()
        ]);
    }

    async openEditTitle(): Promise<void> {
        await this.linkEditTitle.click();
    }

    async createQuestionnaireId(): Promise<void> {
        await this.linkEditQuestionnaireId.click();
    }

    async openQuestions(): Promise<void> {
        await this.linkManageAccess.click();
    }

    async openStartPage(): Promise<void> {
        await this.linkStartPage.click();
    }

    async openAddEditQuestionsAnswers(): Promise<void> {
        await this.linkAddEditQuestionsAnswers.click();
    }

    async openBrandingTheme(): Promise<void> {
        await this.linkEditButtonText.click();
    }

    async openCustomisations(): Promise<void> {
        await this.linkCustomiseStyling.click();
    }

    async openPrivacyPolicy(): Promise<void> {
        await this.linkBranchingMap.click();
    }

    async publish(): Promise<void> {
        await this.btnPublish.click();
    }

    async deleteQuestionnaire(): Promise<void> {
        await this.deleteQuestionnaireButton.click();
    }

    async viewVersions(): Promise<void> {
        await this.linkViewVersions.click();
    }

    async clone(): Promise<void> {
        await this.linkClone.click();
    }

    async publishQuestionnaire() {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.btnPublish.click()
        ]);
    }

    // ===== Validation methods (structure only; not content) =====
    async expectSuccessBannerVisible(): Promise<void> {
        await expect(this.banner).toBeVisible();
        await expect(this.heading).toBeVisible();

        const text = await this.heading.textContent();
        expect(text?.trim().length).toBeGreaterThan(0);
    }

    async validateHeading(expectedText: string): Promise<void> {
        await expect(this.editQuestionnaireHeading).toBeVisible();

        if (expectedText) {
            await expect(
                this.editQuestionnaireHeading,
                `❌ Edit questionnaire heading text mismatch: expected "${expectedText}"`
            ).toContainText(expectedText);
        }
    }

    async validateHeadingAndStatus(): Promise<void> {
        await this.main.waitFor({state: 'attached'});
        await expect(this.main).toBeVisible();
        await expect(this.questionnaireTitle).toBeVisible();
        await expect(this.editQuestionnaireHeading).toBeVisible();
        await expect(this.questionnaireStatus).toBeVisible();
    }

    async validateCoreLinks(): Promise<void> {
        await expect(this.linkEditTitle).toBeVisible();
        await expect(this.linkEditQuestionnaireId).toBeVisible();
        await expect(this.linkManageAccess).toBeVisible();
    }

    async validateOptionalTasks(shouldExist = true): Promise<void> {
        if (shouldExist) {
            await expect(this.linkStartPage).toBeVisible();
            await expect(this.linkEditButtonText).toBeVisible();
            await expect(this.linkCustomiseStyling).toBeVisible();
        } else {
            await expect(this.linkStartPage).toBeHidden();
            await expect(this.linkEditButtonText).toBeHidden();
            await expect(this.linkCustomiseStyling).toBeHidden();
        }
    }

    async validateAnswerContentSection(): Promise<void> {
        await expect(this.linkAnswerContent).toBeVisible();
    }

    async validateBranchingMapSection(): Promise<void> {
        await expect(this.linkBranchingMap).toBeVisible();
    }

    async validateActionsSection(): Promise<void> {
        await expect(this.btnPublish).toBeVisible();
        await expect(this.deleteQuestionnaireButton).toBeVisible();
        await expect(this.linkViewVersions).toBeVisible();
        await expect(this.linkClone).toBeVisible();
    }

    async validateAllSections(includeOptional = true): Promise<void> {
        await this.validateHeadingAndStatus();
        await this.validateCoreLinks();
        await this.validateOptionalTasks(includeOptional);
        await this.validateAnswerContentSection();
        await this.validateBranchingMapSection();
        await this.validateActionsSection();
    }

    async assertPageElements() {
        await this.validateHeadingAndStatus();
        await this.verifyHeaderLinks()
        await this.verifyFooterLinks();
        await this.validateAllSections();
        await this.validateCoreLinks();
        await this.validateOptionalTasks();
        await this.validateAnswerContentSection();
        await this.validateBranchingMapSection();
        await this.validateActionsSection();
    }
    
    async assertGtaaApiErrorBanner(expectedLabel?: string, expectedText?: string) {
        await this.gtaaApiErrorBannerLabel.waitFor({state: 'visible', timeout: Timeouts.LONG});
        
        await expect(this.gtaaApiErrorBannerLabel).toBeVisible();
        await expect(this.gtaaApiErrorBannerText).toBeVisible();
        
        if (expectedLabel) {
            await expect(this.gtaaApiErrorBannerLabel).toHaveText(expectedLabel);
        }
        
        if (expectedText) {
            await expect(this.gtaaApiErrorBannerText).toHaveText(expectedText);
        }
    }

    async expectEditQuestionnaireHeadingOnPage(expectedText?: string): Promise<void> {
        await expect(this.editQuestionnaireHeading, '❌ View question page heading not visible').toBeVisible();

        if (expectedText) {
            await expect(
                this.editQuestionnaireHeading,
                `❌ View question page heading text does not match: expected "${expectedText}"`
            ).toContainText(expectedText);
        }
    }

    async assertSavedStartPageSuccessBanner() {
        await expect(this.justAddedStartPageBannerText).toBeVisible();
        await expect(this.justAddedStartPageBannerText).toHaveText("Your start page has been saved");
    }

    async assertQuestionnaireCreatedSuccessBanner() {
        await expect(this.justCreatedBannerText).toBeVisible();
        await expect(this.justCreatedBannerText).toHaveText("Your questionnaire has been created.");
    }

    async assertQuestionnaireUpdatedSuccessBanner() {
        await expect(this.justUpdatedBannerText).toBeVisible();
        await expect(this.justUpdatedBannerText).toHaveText("Your changes have been saved.");
    }

    async assertQuestionnaireClonedSuccessBanner() {
        await expect(this.justClonedBannerText).toBeVisible();
        await expect(this.justClonedBannerText).toHaveText("Your copy questionnaire has been created.");
    }

    async assertQuestionnairePublishedSuccessBanner() {
        await expect(this.justPublishedBannerText).toBeVisible();
        await expect(this.justPublishedBannerText).toHaveText("Your questionnaire has been published.");
    }

    async assertQuestionnaireUnpublishedSuccessBanner() {
        await expect(this.justUnpublishedBannerText).toBeVisible();
        await expect(this.justUnpublishedBannerText).toHaveText("Your questionnaire has been unpublished.");
    }

    async assertRemovedStartPageSuccessBanner() {
        await expect(this.justRemovedStartPageBannerText).toBeVisible();
        await expect(this.justRemovedStartPageBannerText).toHaveText("Your start page has been removed");
    }

    async assertResetStylingSuccessBanner() {
        await expect(this.justResetStylingBannerText).toBeVisible();
        await expect(this.justResetStylingBannerText).toHaveText("Your customised styling has been reset");
    }

    async assertUpdatedStylingSuccessBanner() {
        await expect(this.justUpdatedStylingBannerText).toBeVisible();
        await expect(this.justUpdatedStylingBannerText).toHaveText("Your customised styling has been saved");
    }

    async assertModifiedButtonTextSuccessBanner() {
        await expect(this.justModifiedButtonTextBannerText).toBeVisible();
        await expect(this.justModifiedButtonTextBannerText).toHaveText("Your button text has been saved");
    }

    async assertRemovedStartPageImageSuccessBanner() {
        await expect(this.justRemovedStartPageImageBannerText).toBeVisible();
        await expect(this.justRemovedStartPageImageBannerText).toHaveText("Your start page image has been removed");
    }
}