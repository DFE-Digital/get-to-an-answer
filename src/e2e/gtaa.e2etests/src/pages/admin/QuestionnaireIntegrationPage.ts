import {Page, expect, Locator} from '@playwright/test';
import {BasePage} from "../BasePage";
import {JwtHelper} from "../../helpers/JwtHelper";

export class QuestionnaireIntegrationPage extends BasePage {

    // ===== Locators =====
    readonly iframeCodeContainer: Locator;
    readonly iframeCopyButton: Locator;
    readonly iframeCopyBlock: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);

        this.iframeCodeContainer = page.locator('#embed-iframe-code-container');
        this.iframeCopyButton = page.locator('#embed-iframe-code-copy-btn');
        this.iframeCopyBlock = page.locator('#embed-iframe-code');
    }

    // ===== Actions =====
    async copyIframeCode() {
        await this.iframeCopyButton.click();
        // get code from clipboard
        return await this.page.evaluate(async () => {
            return await navigator.clipboard.readText();
        });
    }
}