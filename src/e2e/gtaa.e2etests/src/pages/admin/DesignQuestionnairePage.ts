import {expect, Page, Locator} from '@playwright/test';
import {BasePage} from '../BasePage';
import {Timeouts} from '../../constants/timeouts';
import {TaskStatus} from "../../constants/test-data-constants";

export class DesignQuestionnairePage extends BasePage {
    // ===== Page-level Locators =====
    private readonly main: Locator;
    private readonly banner: Locator;
    private readonly heading: Locator;
    private readonly linkBackToQuestionnaires: Locator;
    private readonly questionnaireTitle: Locator;
    private readonly editQuestionnaireHeading: Locator;
    private readonly questionnaireStatus: Locator;

    // Task list container + sections (ULs)
    private readonly taskListContainer: Locator;

    private readonly manageSection: Locator;
    private readonly editSection: Locator;
    private readonly customiseSection: Locator;
    private readonly reviewSection: Locator;
    private readonly publishSection: Locator;

    // Section-scoped link/text locators
    // Manage questionnaire
    private readonly manage_editName: Locator;
    private readonly manage_manageAccess: Locator;
    private readonly manage_makeCopy: Locator;
    private readonly manage_viewVersions: Locator;
    private readonly manage_viewVersionsText: Locator;
    private readonly manage_viewVersions_status: Locator;

    // Edit questionnaire
    private readonly edit_startPage: Locator;
    private readonly edit_questionsAnswers: Locator;
    private readonly edit_resultsPages: Locator;

    // Customise questionnaire
    private readonly customise_styling: Locator;
    private readonly customise_buttonText: Locator;

    // Review questionnaire
    private readonly review_branchingMap: Locator;
    private readonly review_preview: Locator;

    // Prepare to publish questionnaire
    private readonly publish_questionnaireId: Locator;
    private readonly publish_publish: Locator;
    private readonly publish_integrationGuide: Locator;

    // Status locators (section-scoped)
    private readonly edit_startPage_status: Locator;
    private readonly edit_questionsAnswers_status: Locator;
    private readonly edit_resultsPages_status: Locator;
    private readonly customise_styling_status: Locator;
    private readonly customise_buttonText_status: Locator;
    private readonly publish_questionnaireId_status: Locator;

    // Other general actions
    private readonly deleteQuestionnaireButton: Locator;
    private readonly linkViewVersions: Locator;
    private readonly linkPreviewQuestionnaire: Locator;
    private readonly linkClone: Locator;

    // Notification banners
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

    private readonly addStartPageLink: Locator;
    private readonly resultsPagesTaskStatus: Locator;
    
    constructor(page: Page) {
        super(page);

        // ----- Top-level Page Elements -----
        this.main = page.locator('main.govuk-main-wrapper[role="main"]');
        this.banner = page.locator('div.govuk-notification-banner--success[role="alert"]');
        this.heading = this.banner.locator('.govuk-notification-banner__title');

        this.linkBackToQuestionnaires = page.locator('#back-to-manage');
        this.questionnaireTitle = page.locator('#current-questionnaire-title');
        this.editQuestionnaireHeading = page.locator('#track-questionnaire-page-heading');
        this.questionnaireStatus = page.locator('#questionnaire-status');

        // ----- Task-list Sections -----
        this.taskListContainer = page.locator('.app-task-list');

        this.manageSection = this.taskListContainer
            .getByRole('heading', {level: 2, name: /Manage questionnaire/i})
            .locator('xpath=following-sibling::ul[1]');

        this.editSection = this.taskListContainer
            .getByRole('heading', {level: 2, name: /Edit questionnaire/i})
            .locator('xpath=following-sibling::ul[1]');

        this.customiseSection = this.taskListContainer
            .getByRole('heading', {level: 2, name: /Customise questionnaire/i})
            .locator('xpath=following-sibling::ul[1]');

        this.reviewSection = this.taskListContainer
            .getByRole('heading', {level: 2, name: /Review questionnaire/i})
            .locator('xpath=following-sibling::ul[1]');

        this.publishSection = this.taskListContainer
            .getByRole('heading', {level: 2, name: /Prepare to publish questionnaire/i})
            .locator('xpath=following-sibling::ul[1]');

        // ----- Section-scoped Locators: Manage Section -----
        this.manage_editName = this.manageSection.locator('#edit-questionnaire-name');
        this.manage_manageAccess = this.manageSection.locator('#manage-access');
        this.manage_makeCopy = this.manageSection.locator('#make-copy');
        // No id for this, so assert using text
        this.manage_viewVersions = this.manageSection.locator('#view-version-history');
        this.manage_viewVersions_status = this.manageSection.locator('#view-version-history-status');
        this.manage_viewVersionsText = this.manageSection.getByText(/View version history/i);

        // ----- Edit Section (links) -----
        this.edit_startPage = this.editSection.locator('#edit-start-page');
        this.edit_questionsAnswers = this.editSection.locator('#edit-questions');
        this.edit_resultsPages = this.editSection.locator('#edit-results-pages');

        // ----- Customise Section (links) -----
        this.customise_styling = this.customiseSection.locator('#customise-styling');
        this.customise_buttonText = this.customiseSection.locator('#edit-button-text');

        // ----- Review Section (links) -----
        this.review_branchingMap = this.reviewSection.locator('#view-branching-map');
        this.review_preview = this.reviewSection.locator('#preview-questionnaire');

        // ----- Publish Section (links) -----
        this.publish_questionnaireId = this.publishSection.locator('#edit-questionnaire-id');
        this.publish_publish = this.publishSection.locator('#publish-questionnaire');
        this.publish_integrationGuide = this.publishSection.locator('#integration-guide');

        // ----- Status locators (relative to link) -----
        this.edit_startPage_status = this.edit_startPage.locator(
            'xpath=../../div[contains(@class,"govuk-task-list__status")]'
        );
        this.edit_questionsAnswers_status = this.edit_questionsAnswers.locator(
            'xpath=../../div[contains(@class,"govuk-task-list__status")]'
        );
        this.edit_resultsPages_status = this.edit_resultsPages.locator(
            'xpath=../../div[contains(@class,"govuk-task-list__status")]'
        );
        this.customise_styling_status = this.customise_styling.locator(
            'xpath=../../div[contains(@class,"govuk-task-list__status")]'
        );
        this.customise_buttonText_status = this.customise_buttonText.locator(
            'xpath=../../div[contains(@class,"govuk-task-list__status")]'
        );
        this.publish_questionnaireId_status = this.publish_questionnaireId.locator(
            'xpath=../../div[contains(@class,"govuk-task-list__status")]'
        );

        // ----- Other buttons -----
        this.deleteQuestionnaireButton = page.locator('#delete-questionnaire');

        // ----- Banner Locators -----
        this.justCreatedBannerText = page.locator('#just-created-banner-text');
        this.justUpdatedBannerText = page.locator('#just-updated-banner-text');
        this.justClonedBannerText = page.locator('#just-cloned-banner-text');
        this.justPublishedBannerText = page.locator('#just-published-banner-text');
        this.justUnpublishedBannerText = page.locator('#just-unpublished-banner-text');
        this.justAddedStartPageBannerText = page.locator('#just-added-start-page-banner-text');
        this.justRemovedStartPageBannerText = page.locator('#just-removed-start-page-banner-text');
        this.justResetStylingBannerText = page.locator('#just-reset-styling-banner-text');
        this.justUpdatedStylingBannerText = page.locator('#just-updated-styling-banner-text');
        this.justModifiedButtonTextBannerText = page.locator('#just-modified-button-text-banner-text');
        this.justRemovedStartPageImageBannerText = page.locator('#just-removed-start-page-image-banner-text');
        this.gtaaApiErrorBannerLabel = page.locator('#gtaa-api-error-banner-label');
        this.gtaaApiErrorBannerText = page.locator('#gtaa-api-error-banner-text');
        this.linkViewVersions = this.page.locator('#view-version-history');
        this.linkPreviewQuestionnaire = this.page.locator('#preview-questionnaire');
        this.linkClone = this.page.locator('#make-copy');

        this.addStartPageLink = page.locator('#edit-start-page');
        this.resultsPagesTaskStatus = page.locator('#edit-results-pages-status');
    }

    // =====================================================
    //                     Actions
    // =====================================================

    async ClickBackToQuestionnaireLink(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.linkBackToQuestionnaires.click(),
        ]);
    }

    async openEditQuestionnaireName(): Promise<void> {
        await this.manage_editName.click();
    }

    async createQuestionnaireId(): Promise<void> {
        await this.publish_questionnaireId.click();
    }

    async openManageAccess(): Promise<void> {
        await this.manage_manageAccess.click();
    }

    async openVersionHistory(): Promise<void> {
        await this.manage_viewVersions.click();
    }
    
    async openStartPage(): Promise<void> {
        await this.edit_startPage.click();
    }

    async openAddEditQuestionsAnswers(): Promise<void> {
        await this.edit_questionsAnswers.click();
    }

    async openAddEditResultsPage(): Promise<void> {
        await this.edit_resultsPages.click();
    }

    async openEditButtonText(): Promise<void> {
        await this.customise_buttonText.click();
    }

    async openCustomiseStyling(): Promise<void> {
        await this.customise_styling.click();
    }

    async openViewVersionHistory() {
        await this.linkViewVersions.click();
    }

    async openBranchingMap(): Promise<void> {
        await this.review_branchingMap.click();
    }

    async previewQuestionnaire(): Promise<void> {
        await this.review_preview.click();
    }

    async publish(): Promise<void> {
        await this.publish_publish.click();
    }

    async publishQuestionnaire(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.publish_publish.click(),
        ]);
    }

    async openPreview() {
        const [newPage] = await Promise.all([
            this.page.context().waitForEvent('page'),
            this.linkPreviewQuestionnaire.click()
        ]);

        return newPage;
    }

    async openIntegrationGuide(): Promise<void> {
        await this.publish_integrationGuide.click();
    }

    async clickDeleteQuestionnaireButton(): Promise<void> {
        await this.deleteQuestionnaireButton.click();
    }
    
    async openAddStartPage(): Promise<void> {
        await expect(this.addStartPageLink, '❌ Add start page link missing').toBeVisible();
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.addStartPageLink.click(),
        ]);
    }


    async getQuestionnaireStatus(): Promise<string> {
        await this.questionnaireStatus.waitFor({state: 'visible'});
        return await this.questionnaireStatus.textContent() || '';
    }

    // =====================================================
    //              Generic status assertion helper
    // =====================================================

    private async assertTaskStatus(
        statusLocator: Locator,
        expectedStatus: string,
        taskName: string
    ): Promise<void> {
        await expect(
            statusLocator,
            `❌ Status container missing for task "${taskName}"`
        ).toBeVisible();

        const text = (await statusLocator.innerText()).trim();

        await expect(
            text,
            `❌ Expected status for "${taskName}" to be "${expectedStatus}" but got "${text}"`
        ).toBe(expectedStatus);
    }

    // =====================================================
    //                 Section Validation
    // =====================================================

    async validateManageSection(): Promise<void> {
        await expect(this.manageSection, '❌ "Manage questionnaire" section UL should be visible').toBeVisible();

        await expect(
            this.manage_editName,
            '❌ "Edit the name of your questionnaire" link should be visible in Manage section',
        ).toBeVisible();

        await expect(
            this.manage_manageAccess,
            '❌ "Manage access" link should be visible in Manage section',
        ).toBeVisible();

        await expect(
            this.manage_makeCopy,
            '❌ "Make a copy" link should be visible in Manage section',
        ).toBeVisible();

        await expect(
            this.manage_viewVersionsText,
            '❌ "View questionnaire versions" text should be visible in Manage section',
        ).toBeVisible();

        await expect(
            this.manage_viewVersions_status,
            '❌ Expected status "Cannot view until published" for View questionnaire versions'
        ).toContainText('Cannot view until published');
    }

    async validateEditSection(): Promise<void> {
        await expect(this.editSection, '❌ "Edit questionnaire" section UL should be visible').toBeVisible();

        await expect(
            this.edit_startPage,
            '❌ "Add and edit start page" link should be visible in Edit section',
        ).toBeVisible();

        await expect(
            this.edit_questionsAnswers,
            '❌ "Add or edit questions and answers" link should be visible in Edit section',
        ).toBeVisible();

        await expect(
            this.edit_resultsPages,
            '❌ "Add and edit results pages" link should be visible in Edit section',
        ).toBeVisible();
    }

    async validateCustomiseSection(): Promise<void> {
        await expect(this.customiseSection, '❌ "Customise questionnaire" section UL should be visible').toBeVisible();

        await expect(
            this.customise_styling,
            '❌ "Customise styling" link should be visible in Customise section',
        ).toBeVisible();

        await expect(
            this.customise_buttonText,
            '❌ "Edit button text" link should be visible in Customise section',
        ).toBeVisible();
    }

    async validateReviewSection(): Promise<void> {
        await expect(this.reviewSection, '❌ "Review questionnaire" section UL should be visible').toBeVisible();

        await expect(
            this.review_branchingMap,
            '❌ "View branching map" link should be visible in Review section',
        ).toBeVisible();

        await expect(
            this.review_preview,
            '❌ "Preview questionnaire" link should be visible in Review section',
        ).toBeVisible();
    }

    async validatePublishSection(): Promise<void> {
        await expect(
            this.publishSection,
            '❌ "Prepare to publish questionnaire" section UL should be visible',
        ).toBeVisible();

        await expect(
            this.publish_questionnaireId,
            '❌ "Add or edit questionnaire ID" link should be visible in Publish section',
        ).toBeVisible();

        await expect(
            this.publish_publish,
            '❌ "Publish questionnaire" link should be visible in Publish section',
        ).toBeVisible();

        await expect(
            this.publish_integrationGuide,
            '❌ "Add questionnaire to your service" link should be visible in Publish section',
        ).toBeVisible();
    }

    async validateAllSectionsAndRelatedLinks(): Promise<void> {
        await this.validateManageSection();
        await this.validateEditSection();
        await this.validateCustomiseSection();
        await this.validateReviewSection();
        await this.validatePublishSection();
    }

    // =====================================================
    //         Status validation: Optional & Not started
    // =====================================================

    async validateOptionalTasks(): Promise<void> {
        await this.assertTaskStatus(
            this.edit_startPage_status,
            'Optional',
            'Add and edit start page'
        );

        await this.assertTaskStatus(
            this.edit_resultsPages_status,
            'Optional',
            'Add and edit results pages'
        );

        await this.assertTaskStatus(
            this.customise_styling_status,
            'Optional',
            'Customise styling'
        );

        await this.assertTaskStatus(
            this.customise_buttonText_status,
            'Optional',
            'Edit button text'
        );
    }

    async validateNotStartedTasks(): Promise<void> {
        // "Add or edit questions and answers"
        await this.assertTaskStatus(
            this.edit_questionsAnswers_status,
            'Not started',
            'Add or edit questions and answers'
        );

        // "Add or edit questionnaire ID"
        await this.assertTaskStatus(
            this.publish_questionnaireId_status,
            'Not started',
            'Add or edit questionnaire ID'
        );
    }
    
    async taskStatusAddEditQuestionsAnswers(taskStatus: string, taskName: string): Promise<void> {
        await this.assertTaskStatus(
            this.edit_questionsAnswers_status,
            taskStatus,
            taskName
        );
    }

    async taskStatusAddEditQuestionnaireId(taskStatus: string, taskName: string): Promise<void> {
        await this.assertTaskStatus(
            this.publish_questionnaireId_status,
            taskStatus,
            taskName
        );
    }
    
    async taskStatusAddEditStartPage(taskStatus: string, taskName: string): Promise<void> {
        await this.assertTaskStatus(
            this.edit_startPage_status,
            taskStatus,
            taskName
        );
    }
    
    async taskStatusEditButtonTextPage(taskStatus: string, taskName: string): Promise<void> {
        await this.assertTaskStatus(
            this.customise_buttonText_status,
            taskStatus,
            taskName
        );
    }
    
    async taskStatusCustomiseStylingPage(taskStatus: string, taskName: string): Promise<void> {
        await this.assertTaskStatus(
            this.customise_styling_status,
            taskStatus,
            taskName
        );
    }

    async validateAnswerContentSection(): Promise<void> {
        await expect(
            this.edit_resultsPages,
            '❌ Answer content section: "Add and edit results pages" link should be visible',
        ).toBeVisible();
    }

    async validateBranchingMapSection(): Promise<void> {
        await expect(
            this.review_branchingMap,
            '❌ Branching map section: "View branching map" link should be visible',
        ).toBeVisible();
    }

    // =====================================================
    //             General Page Assertions
    // =====================================================

    async expectSuccessBannerVisible(): Promise<void> {
        await expect(this.banner, '❌ Success banner should be visible').toBeVisible();
        await expect(this.heading, '❌ Success banner heading should be visible').toBeVisible();

        const text = (await this.heading.textContent()) ?? '';
        await expect(
            text.trim().length,
            '❌ Success banner heading text should not be empty',
        ).toBeGreaterThan(0);
    }

    async validateHeading(expectedText: string): Promise<void> {
        await expect(
            this.editQuestionnaireHeading,
            '❌ Design questionnaire heading should be visible',
        ).toBeVisible();

        if (expectedText) {
            await expect(
                this.editQuestionnaireHeading,
                `❌ Design questionnaire heading text mismatch: expected "${expectedText}"`,
            ).toContainText(expectedText);
        }
    }

    async validateHeadingAndStatus(): Promise<void> {
        await this.main.waitFor({state: 'attached'});

        await expect(this.main, '❌ Main content area should be visible').toBeVisible();
        await expect(this.questionnaireTitle, '❌ Questionnaire title caption should be visible').toBeVisible();
        await expect(this.editQuestionnaireHeading, '❌ Page heading should be visible').toBeVisible();
        await expect(this.questionnaireStatus, '❌ Questionnaire status tag should be visible').toBeVisible();
    }

    async expectEditQuestionnaireHeadingOnPage(expectedText?: string): Promise<void> {
        await expect(
            this.editQuestionnaireHeading,
            '❌ Design questionnaire heading not visible on page',
        ).toBeVisible();

        if (expectedText) {
            await expect(
                this.editQuestionnaireHeading,
                `❌ Design questionnaire heading text does not match: expected "${expectedText}"`,
            ).toContainText(expectedText);
        }
    }

    async assertPageElements(): Promise<void> {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();
        await this.validateHeadingAndStatus();
    }

    // =====================================================
    //         API error banner & success banners
    // =====================================================

    async assertGtaaApiErrorBanner(expectedLabel?: string, expectedText?: string): Promise<void> {
        await this.gtaaApiErrorBannerLabel.waitFor({
            state: 'visible',
            timeout: Timeouts.LONG,
        });

        await expect(
            this.gtaaApiErrorBannerLabel,
            '❌ GTAA API error banner label should be visible',
        ).toBeVisible();

        await expect(
            this.gtaaApiErrorBannerText,
            '❌ GTAA API error banner text should be visible',
        ).toBeVisible();

        if (expectedLabel) {
            await expect(
                this.gtaaApiErrorBannerLabel,
                `❌ GTAA API error banner label text mismatch: expected "${expectedLabel}"`,
            ).toHaveText(expectedLabel);
        }

        if (expectedText) {
            await expect(
                this.gtaaApiErrorBannerText,
                `❌ GTAA API error banner body text mismatch: expected "${expectedText}"`,
            ).toHaveText(expectedText);
        }
    }

    async assertSavedStartPageSuccessBanner(): Promise<void> {
        await expect(
            this.justAddedStartPageBannerText,
            '❌ Start page saved success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justAddedStartPageBannerText,
            '❌ Start page saved success banner text mismatch',
        ).toHaveText('Your start page has been saved');
    }

    async assertQuestionnaireCreatedSuccessBanner(): Promise<void> {
        await expect(
            this.justCreatedBannerText,
            '❌ Questionnaire created success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justCreatedBannerText,
            '❌ Questionnaire created success banner text mismatch',
        ).toHaveText('Your questionnaire has been created.');
    }

    async assertQuestionnaireUpdatedSuccessBanner(): Promise<void> {
        await expect(
            this.justUpdatedBannerText,
            '❌ Questionnaire updated success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justUpdatedBannerText,
            '❌ Questionnaire updated success banner text mismatch',
        ).toHaveText('Your changes have been saved.');
    }

    async assertQuestionnaireClonedSuccessBanner(): Promise<void> {
        await expect(
            this.justClonedBannerText,
            '❌ Questionnaire cloned success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justClonedBannerText,
            '❌ Questionnaire cloned success banner text mismatch',
        ).toHaveText('Your copy questionnaire has been created.');
    }

    async assertQuestionnairePublishedSuccessBanner(): Promise<void> {
        await expect(
            this.justPublishedBannerText,
            '❌ Questionnaire published success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justPublishedBannerText,
            '❌ Questionnaire published success banner text mismatch',
        ).toHaveText('Your questionnaire has been published.');
    }

    async assertQuestionnaireUnpublishedSuccessBanner(): Promise<void> {
        await expect(
            this.justUnpublishedBannerText,
            '❌ Questionnaire unpublished success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justUnpublishedBannerText,
            '❌ Questionnaire unpublished success banner text mismatch',
        ).toHaveText('Your questionnaire has been unpublished.');
    }

    async assertRemovedStartPageSuccessBanner(): Promise<void> {
        await expect(
            this.justRemovedStartPageBannerText,
            '❌ Start page removed success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justRemovedStartPageBannerText,
            '❌ Start page removed success banner text mismatch',
        ).toHaveText('Your start page has been removed');
    }

    async assertResetStylingSuccessBanner(): Promise<void> {
        await expect(
            this.justResetStylingBannerText,
            '❌ Styling reset success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justResetStylingBannerText,
            '❌ Styling reset success banner text mismatch',
        ).toHaveText('Your customised styling has been reset');
    }

    async assertUpdatedStylingSuccessBanner(): Promise<void> {
        await expect(
            this.justUpdatedStylingBannerText,
            '❌ Styling updated success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justUpdatedStylingBannerText,
            '❌ Styling updated success banner text mismatch',
        ).toHaveText('Your customised styling has been saved');
    }

    async assertModifiedButtonTextSuccessBanner(): Promise<void> {
        await expect(
            this.justModifiedButtonTextBannerText,
            '❌ Button text modified success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justModifiedButtonTextBannerText,
            '❌ Button text modified success banner text mismatch',
        ).toHaveText('Your button text has been saved');
    }

    async assertRemovedStartPageImageSuccessBanner(): Promise<void> {
        await expect(
            this.justRemovedStartPageImageBannerText,
            '❌ Start page image removed success banner should be visible',
        ).toBeVisible();

        await expect(
            this.justRemovedStartPageImageBannerText,
            '❌ Start page image removed success banner text mismatch',
        ).toHaveText('Your start page image has been removed');
    }

    async validateQuestionnaireDefaultStatus(status: string): Promise<void> {
        expect(status.trim().toLowerCase(), '❌ Questionnaire default status is incorrect').toBe('draft');
    }

    // Assert that task list reflects that contributors and results pages have been configured
    async expectTaskStatusReflectsConfiguredContributorsAndResults(): Promise<void> {
        await this.verifyHeaderLinks();
        await this.verifyFooterLinks();

        const resultsStatusText =
            (await this.resultsPagesTaskStatus.textContent())?.trim() ?? '';

        expect(resultsStatusText.length).toBeGreaterThan(0);
    }

    async assertViewHistoryLinkDisabled() {
        // check tag type is a span element when disabled
        await expect(this.manage_viewVersions).toBeVisible();
        await expect(this.manage_viewVersions, 'View history link should be a span element when disabled').toHaveJSProperty('tagName', 'SPAN');
        
        await expect(this.manage_viewVersions_status).toBeVisible();
        await expect(this.manage_viewVersions_status, 'View history status should populated').toHaveText(TaskStatus.CANNOT_VIEW_HISTORY);
        
        // Verify disabled state attributes
        await expect(this.manage_viewVersions, 'View history link should have aria-disabled attribute').toHaveAttribute('aria-disabled', 'true');
        await expect(this.manage_viewVersions, 'View history link should have tabindex attribute').toHaveAttribute('tabindex', '-1');
    }

    async assertViewHistoryLinkEnabled() {
        // check tag type is a anchor link element when enabled
        await expect(this.manage_viewVersions).toBeVisible();
        await expect(this.manage_viewVersions, 'View history link should be a anchor link element when enabled').toHaveJSProperty('tagName', 'A');

        await expect(this.manage_viewVersions_status).not.toBeVisible();

        // Verify disabled state attributes
        await expect(this.manage_viewVersions, 'View history link should have aria-disabled attribute').not.toHaveAttribute('aria-disabled', 'true');
        await expect(this.manage_viewVersions, 'View history link should have tabindex attribute').toHaveAttribute('tabindex', '-1');
    }
}