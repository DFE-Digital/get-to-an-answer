import {expect, Page} from '@playwright/test';
import {BasePage} from '../BasePage';

type Mode = 'create' | 'edit' | 'clone';

export class EditQuestionnairePage extends BasePage {
    private static readonly EDIT_URL: RegExp =
        /\/admin\/questionnaires\/[0-9a-f-]+\/track\/?$/i;

    // ===== Locators =====
    private readonly banner = this.page.locator('div.govuk-notification-banner--success[role="alert"]');
    private readonly heading = this.banner.locator('.govuk-notification-banner__heading');
    private readonly main = this.page.locator('main.govuk-main-wrapper[role="main"]');

    // Status badges (draft etc.)
    private readonly draftBadge = this.page.locator('[data-status="Draft"]');

    private readonly linkEditTitle = this.page.locator(
        'a.govuk-task-list__link[aria-describedby="create-your-questionnaire-1-status"]'
    );

    private readonly linkEditSlug = this.page.locator(
        'a.govuk-task-list__link[aria-describedby="edit-slug-status"]'
    );

    private readonly linkAddEditQuestions = this.page.locator(
        'a.govuk-task-list__link[href*="/questionnaires/"][href$="/questions"]'
    );
    
    private readonly linkStartPage = this.page.locator(
        'a.govuk-task-list__link[href$="/start-page/edit"]'
    );

    private readonly linkBrandingTheme = this.page.locator(
        'a.govuk-task-list__link[href$="/branding"]'
    );
    
    private readonly linkCustomisations = this.page.locator(
        'a.govuk-task-list__link[href$="/customizations"]'
    );
    
    private readonly linkAnswerContent = this.page.locator(
        'a.govuk-task-list__link[href$="/contents"]'
    );

    private readonly linkPrivacyPolicy = this.page.locator(
        'a.govuk-task-list__link[href*="/privacy-policy"]'
    );


    private readonly btnPublish = this.page.locator(
        'a.govuk-button.govuk-button--primary[href$="/publish/confirm"]'
    );

    private readonly btnDelete = this.page.locator(
        'a.govuk-button.govuk-button--warning[href$="/delete/confirm"]'
    );

    private readonly linkViewVersions = this.page.locator(
        'a.govuk-link[href$="/versions"]'
    );

    private readonly linkClone = this.page.locator(
        'a.govuk-link[href$="/clone"]'
    );

    // ===== Constructor =====
    constructor(page: Page, mode: Mode = 'create') {
        super(page);
    }

    // ===== Actions =====
    async expectSuccessBannerVisible(): Promise<void> {
        await expect(this.banner).toBeVisible();
        await expect(this.heading).toBeVisible();

        const text = await this.heading.textContent();
        expect(text?.trim().length).toBeGreaterThan(0);
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
    async expectUrlOnPage(): Promise<void> {
        await this.validateUrlMatches(EditQuestionnairePage.EDIT_URL);
    }

    async validateHeadingAndStatus(): Promise<void> {
        await expect(this.main).toBeVisible();
        await expect(this.draftBadge).toBeVisible();
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
        await this.expectUrlOnPage();
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