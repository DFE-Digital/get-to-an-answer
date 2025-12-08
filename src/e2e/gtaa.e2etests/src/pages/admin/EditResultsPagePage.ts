import {expect, Locator, Page} from '@playwright/test';
import {BasePage} from '../BasePage';
import {ErrorMessages} from "../../constants/test-data-constants";
import {EditAnswerTable} from "./components/EditAnswerTable";
import {Timeouts} from '../../constants/timeouts'
import {AddResultsPagePage} from "./AddResultsPagePage";

export class EditResultsPagePage extends AddResultsPagePage {

    private readonly removeResultsPageButton: Locator;

    // ===== Constructor =====
    constructor(page: Page) {
        super(page);
        
        this.removeResultsPageButton = page.locator('#remove-results-page');
    }
    
    // ===== Validation =====

    async clickRemoveResultsPage(): Promise<void> {
        await this.removeResultsPageButton.click();
    }
}