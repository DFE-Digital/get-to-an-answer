import {expect, Page, Locator} from '@playwright/test';
import {BasePage} from '../BasePage';

type Mode = 'create' | 'edit' | 'clone';

export class EditQuestionnairePage extends BasePage {
    // ===== Locators =====
    private readonly main: Locator;
    private readonly banner: Locator;
    private readonly heading: Locator;
    private readonly backToQuestionnaireLink: Locator;
    private readonly questionnaireTitle: Locator;
    private readonly editQuestionnaireHeading: Locator;
    private readonly questionnaireStatus: Locator;
    private readonly linkEditTitle: Locator;
    private readonly linkEditSlug: Locator;
    private readonly linkAddEditQuestions: Locator;
    private readonly linkStartPage: Locator;
    private readonly linkBrandingTheme: Locator;
    private readonly linkCustomisations: Locator;
    private readonly linkAnswerContent: Locator;
    private readonly linkPrivacyPolicy: Locator;
    private readonly btnPublish: Locator;
    private readonly btnDelete: Locator;
    private readonly linkViewVersions: Locator;
    private readonly linkClone: Locator;

    constructor(page: Page, mode: Mode = 'edit') {
        super(page);
        this.main = this.page.locator('main.govuk-main-wrapper[role="main"]');
        this.banner = this.page.locator('div.govuk-notification-banner--success[role="alert"]');
        this.heading = this.banner.locator('.govuk-notification-banner__heading');
        this.questionnaireTitle = this.page.locator('span.govuk-caption-l');
        this.editQuestionnaireHeading = this.page.locator('main[role="main"] h1.govuk-heading-l');
        this.questionnaireStatus = this.page.locator('strong.govuk-tag[data-status]');
        this.linkEditTitle = this.page.locator(
            'a.govuk-task-list__link[aria-describedby="create-your-questionnaire-1-status"]'
        );
        this.backToQuestionnaireLink = this.page.locator(
            'a.govuk-back-link[href$="/admin/questionnaires/manage"]'
        );
        this.linkEditSlug = this.page.locator(
            'a.govuk-task-list__link[aria-describedby="edit-slug-status"]'
        );
        this.linkAddEditQuestions = this.page.locator(
            'a.govuk-task-list__link[href*="/questionnaires/"][href$="/questions"]'
        );
        this.linkStartPage = this.page.locator(
            'a.govuk-task-list__link[href$="/start-page/edit"]'
        );
        this.linkBrandingTheme = this.page.locator(
            'a.govuk-task-list__link[href$="/branding"]'
        );
        this.linkCustomisations = this.page.locator(
            'a.govuk-task-list__link[href$="/customizations"]'
        );
        this.linkAnswerContent = this.page.locator(
            'a.govuk-task-list__link[href$="/contents"]'
        );
        this.linkPrivacyPolicy = this.page.locator(
            'a.govuk-task-list__link[href*="/privacy-policy"]'
        );
        this.btnPublish = this.page.locator(
            'a.govuk-button.govuk-button--primary[href$="/publish/confirm"]'
        );
        this.btnDelete = this.page.locator(
            'a.govuk-button.govuk-button--warning[href$="/delete/confirm"]'
        );
        this.linkViewVersions = this.page.locator(
            'a.govuk-link[href$="/versions"]'
        );
        this.linkClone = this.page.locator(
            'a.govuk-link[href$="/clone"]'
        );
    }

    // ===== Actions =====
    async ClickBackToQuestionnaireLink(): Promise<void> {
        await Promise.all([
            this.page.waitForLoadState('networkidle'),
            this.backToQuestionnaireLink.click()
        ]);
    }

    async openEditTitle(): Promise<void> {
        await this.linkEditTitle.click();
    }

    async openEditSlug(): Promise<void> {
        await this.linkEditSlug.click();
    }

    async openQuestions(): Promise<void> {
        await this.linkAddEditQuestions.click();
    }

    async openStartPage(): Promise<void> {
        await this.linkStartPage.click();
    }

    async openBrandingTheme(): Promise<void> {
        await this.linkBrandingTheme.click();
    }

    async openCustomisations(): Promise<void> {
        await this.linkCustomisations.click();
    }

    async openAnswerContent(): Promise<void> {
        await this.linkAnswerContent.click();
    }

    async openPrivacyPolicy(): Promise<void> {
        await this.linkPrivacyPolicy.click();
    }

    async publish(): Promise<void> {
        await this.btnPublish.click();
    }

    async delete(): Promise<void> {
        await this.btnDelete.click();
    }

    async viewVersions(): Promise<void> {
        await this.linkViewVersions.click();
    }

    async clone(): Promise<void> {
        await this.linkClone.click();
    }

    // ===== Validation methods (structure only; not content) =====
    async expectSuccessBannerVisible(): Promise<void> {
        await this.banner.waitFor({state: 'attached'});
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
        await expect(this.linkEditSlug).toBeVisible();
        await expect(this.linkAddEditQuestions).toBeVisible();
    }

    async validateOptionalTasks(shouldExist = true): Promise<void> {
        if (shouldExist) {
            await expect(this.linkStartPage).toBeVisible();
            await expect(this.linkBrandingTheme).toBeVisible();
            await expect(this.linkCustomisations).toBeVisible();
        } else {
            await expect(this.linkStartPage).toBeHidden();
            await expect(this.linkBrandingTheme).toBeHidden();
            await expect(this.linkCustomisations).toBeHidden();
        }
    }

    async validateAnswerContentSection(): Promise<void> {
        await expect(this.linkAnswerContent).toBeVisible();
    }

    async validatePrivacyAndContactsSection(): Promise<void> {
        await expect(this.linkPrivacyPolicy).toBeVisible();
    }

    async validateActionsSection(): Promise<void> {
        await expect(this.btnPublish).toBeVisible();
        await expect(this.btnDelete).toBeVisible();
        await expect(this.linkViewVersions).toBeVisible();
        await expect(this.linkClone).toBeVisible();
    }

    async validateAllSections(includeOptional = true): Promise<void> {
        await this.validateHeadingAndStatus();
        await this.validateCoreLinks();
        await this.validateOptionalTasks(includeOptional);
        await this.validateAnswerContentSection();
        await this.validatePrivacyAndContactsSection();
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
        await this.validatePrivacyAndContactsSection();
        await this.validateActionsSection();
    }
}