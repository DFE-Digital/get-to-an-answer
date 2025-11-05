import { expect, Page } from '@playwright/test';
import {BasePage} from '../BasePage';

export class EditQuestionnairePage extends BasePage {
    // ===== Root / structural =====
    private readonly main = this.page.locator('main.govuk-main-wrapper[role="main"]');

    // Status badges (draft etc.)
    private readonly draftBadge = this.page.locator('[data-status="Draft"]');

    // --- Task links (each item has a stable aria-describedby or href fragment) ---
    private readonly linkEditTitle = this.page.locator(
        'a.govuk-task-list__link[aria-describedby="create-your-questionnaire-1-status"]'
    );

    private readonly linkEditSlug = this.page.locator(
        'a.govuk-task-list__link[aria-describedby="edit-slug-status"]'
    );

    private readonly linkAddEditQuestions = this.page.locator(
        'a.govuk-task-list__link[href*="/questionnaires/"][href$="/questions"]'
    );

    // Optional tasks shown in your DOM
    private readonly linkStartPage = this.page.locator(
        'a.govuk-task-list__link[href$="/start-page/edit"]'
    );

    private readonly linkBrandingTheme = this.page.locator(
        'a.govuk-task-list__link[href$="/branding"]'
    );

    // “Customisations” section
    private readonly linkCustomisations = this.page.locator(
        'a.govuk-task-list__link[href$="/customizations"]'
    );

    // “Answer content” section
    private readonly linkAnswerContent = this.page.locator(
        'a.govuk-task-list__link[href$="/contents"]'
    );

    // Privacy / contact details (note: different base path: /forms/{id}/privacy-policy)
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

    constructor(page: Page) {
        super(page);
    }

    // ===== Actions =====
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

    // ===== Validation methods (structure only; no wording) =====
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
}