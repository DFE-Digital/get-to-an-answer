// typescript
import {expect, Page} from '@playwright/test';
import {BasePage} from '../BasePage';

export class EditQuestionnairePage extends BasePage {

    // ===== Locators =====
    private heading = this.page.getByRole('heading', {name: 'Edit your questionnaire'});
    private successBanner = this.page.getByText('Your questionnaire has been created', {exact: true});
    private draftBadge = this.page.getByText('Draft', {exact: true});

    private linkEditTitle = this.page.getByRole('link', {name: 'Edit the title of your questionnaire'});
    private linkEditSlug = this.page.getByRole('link', {name: 'Edit the slug of your questionnaire'});
    private linkAddQuestions = this.page.getByRole('link', {name: 'Add and edit your questions'});

    private linkAddStartPage = this.page.getByRole('link', {name: 'Add a start page'});
    private linkAddContributors = this.page.getByRole('link', {name: 'Add or remove contributors'});

    private linkBrandingTheme = this.page.getByRole('link', {name: 'Edit the branding and theme of your questionnaire'});
    private linkCustomiseButtons = this.page.getByRole('link', {name: 'Customise the button text, error messages, etc. of your questionnaire'});

    private linkAnswerContent = this.page.getByRole('link', {name: 'Add and edit answer content of your questionnaire'});

    private linkPrivacy = this.page.getByRole('link', {name: 'Provide a link to privacy information for this questionnaire'});
    private linkContactDetails = this.page.getByRole('link', {name: 'Provide contact details for support'});

    private btnPublish = this.page.getByRole('button', {name: 'Publish questionnaire'});
    private btnDelete = this.page.getByRole('button', {name: 'Delete questionnaire'});
    private linkViewVersions = this.page.getByRole('link', {name: 'View questionnaire versions'});
    private linkClone = this.page.getByRole('link', {name: 'Clone this questionnaire'});

    constructor(page: Page) {
        super(page);
    }
    
    // ===== Actions =====
    async openEditTitle() {
        await this.linkEditTitle.click();
    }

    async openEditSlug() {
        await this.linkEditSlug.click();
    }

    async openAddQuestions() {
        await this.linkAddQuestions.click();
    }

    async openAddStartPage() {
        await this.linkAddStartPage.click();
    }

    async openContributors() {
        await this.linkAddContributors.click();
    }

    async openBrandingTheme() {
        await this.linkBrandingTheme.click();
    }

    async openCustomiseButtons() {
        await this.linkCustomiseButtons.click();
    }

    async openAnswerContent() {
        await this.linkAnswerContent.click();
    }

    async openPrivacy() {
        await this.linkPrivacy.click();
    }

    async openContactDetails() {
        await this.linkContactDetails.click();
    }

    async publish() {
        await this.btnPublish.click();
    }

    async delete() {
        await this.btnDelete.click();
    }

    async viewVersions() {
        await this.linkViewVersions.click();
    }

    async clone() {
        await this.linkClone.click();
    }

    // ===== Validation methods =====

    async validateHeadingAndStatus() {
        await expect(this.heading).toBeVisible();
        await expect(this.draftBadge).toBeVisible();
        if (await this.successBanner.count() > 0) {
            await expect(this.successBanner).toBeVisible();
        }
    }

    async validateCoreLinks() {
        await expect(this.linkEditTitle).toBeVisible();
        await expect(this.linkEditSlug).toBeVisible();
        await expect(this.linkAddQuestions).toBeVisible();
    }

    /**
     * Validate optional tasks. Pass `shouldExist = false` to assert they are hidden/absent.
     */
    async validateOptionalTasks(shouldExist = true) {
        if (shouldExist) {
            await expect(this.linkAddStartPage).toBeVisible();
            await expect(this.linkAddContributors).toBeVisible();
        } else {
            await expect(this.linkAddStartPage).toBeHidden();
            await expect(this.linkAddContributors).toBeHidden();
        }
    }

    async validateCustomisationsAndBranding() {
        await expect(this.linkBrandingTheme).toBeVisible();
        await expect(this.linkCustomiseButtons).toBeVisible();
    }

    async validateAnswerContentSection() {
        await expect(this.linkAnswerContent).toBeVisible();
    }

    async validatePrivacyAndContactSection() {
        await expect(this.linkPrivacy).toBeVisible();
        await expect(this.linkContactDetails).toBeVisible();
    }

    async validateActionsSection() {
        await expect(this.btnPublish).toBeVisible();
        await expect(this.btnDelete).toBeVisible();
        await expect(this.linkViewVersions).toBeVisible();
        await expect(this.linkClone).toBeVisible();
    }

    /**
     * Convenience method to validate every section. Pass `includeOptional` as false if you expect optional tasks to be absent.
     */
    async validateAllSections(includeOptional = true) {
        await this.validateHeadingAndStatus();
        await this.validateCoreLinks();
        await this.validateOptionalTasks(includeOptional);
        await this.validateCustomisationsAndBranding();
        await this.validateAnswerContentSection();
        await this.validatePrivacyAndContactSection();
        await this.validateActionsSection();
    }
}
    
