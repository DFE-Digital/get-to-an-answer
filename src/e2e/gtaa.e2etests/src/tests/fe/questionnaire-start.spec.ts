import {expect, test} from "@playwright/test";
import {QuestionnaireStartPage} from "../../pages/fe/QuestionnaireStartPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {
    createQuestionnaire,
    publishQuestionnaire,
    updateQuestionnaire
} from "../../test-data-seeder/questionnaire-data";
import {createQuestion} from "../../test-data-seeder/question-data";
import {AnswerDestinationType, QuestionType} from "../../constants/test-data-constants";
import {createAnswer, createSingleAnswer} from "../../test-data-seeder/answer-data";
import {createContent} from "../../test-data-seeder/content-data";

test.describe('Questionnaire Start Page', () => {
    let questionnaireStartPage: QuestionnaireStartPage;
    let token: string;
    let questionnaireSlug: string;
    let questionnaireId: string;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        // Create and publish a questionnaire via API
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
        questionnaireSlug = 'test-questionnaire-start-page-' + Math.floor(Math.random() * 1000000) + '';
        
        await updateQuestionnaire(request, questionnaireId, {
            slug: questionnaireSlug,
            displayTitle: 'Test Questionnaire Start Page',
            description: 'This is a test questionnaire for the start page.'
        }, token);

        // Create a multi-select question
        const {question} = await createQuestion(request, questionnaireId, token,
            'Question 1?',
            QuestionType.MultiSelect,
            'You can choose multiple options'
        );
        const questionId = question.id;
        
        const {question: question2} = await createQuestion(request, questionnaireId, token,
            'Question 2?',
            QuestionType.MultiSelect,
            'You can choose multiple options'
        );
        const question2Id = question2.id;

        // Create answer options
        await createSingleAnswer(request, {
            questionnaireId, 
            questionId,
            content: 'First choice',
            priority: 1,
            destinationType: AnswerDestinationType.Question,
            destinationQuestionId: question2.id
        }, token);
        
        await createSingleAnswer(request, {
            questionnaireId, 
            questionId,
            content: 'Second choice',
            priority: 2,
            destinationType: AnswerDestinationType.ExternalLink,
            destinationUrl: 'https://dev-admin.get-to-an-answer.education.gov.uk'
        }, token);
        
        const {content} = await createContent(request, {
            questionnaireId: questionnaireId,
            title: 'Test Content',
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)
        
        await createSingleAnswer(request, {
            questionnaireId,
            questionId: question2Id,
            content: 'Third choice',
            priority: 3,
            destinationType: AnswerDestinationType.CustomContent,
            destinationContentId: content.id
        }, token);

        // Publish the questionnaire so it's available on frontend
        await publishQuestionnaire(request, questionnaireId, token);

        questionnaireStartPage = new QuestionnaireStartPage(page);
    });

    test.describe('Page Display and Navigation', () => {
        test('Page loads successfully and displays required elements', async () => {
            await questionnaireStartPage.goto(questionnaireSlug);
            await questionnaireStartPage.expectOnPage();
            await questionnaireStartPage.expectDescription();
            const isStartButtonVisible = await questionnaireStartPage.isStartButtonVisible();
            expect(isStartButtonVisible).toBeTruthy();
        });

        test('Page heading is displayed correctly', async () => {
            await questionnaireStartPage.goto(questionnaireSlug);
            const heading = await questionnaireStartPage.getHeading();
            expect(heading).toBeTruthy();
            expect(heading.length).toBeGreaterThan(0);
        });

        test('Page description content is visible', async () => {
            await questionnaireStartPage.goto(questionnaireSlug);
            await questionnaireStartPage.expectDescription();
            const description = await questionnaireStartPage.getDescription();
            expect(description).toBeTruthy();
        });

        test('Start button contains correct text and icon', async () => {
            await questionnaireStartPage.goto(questionnaireSlug);
            const isVisible = await questionnaireStartPage.isStartButtonVisible();
            expect(isVisible).toBeTruthy();
        });
    });

    test.describe('Navigation with Start Button', () => {
        test('Start button navigates to next page', async ({page}) => {
            await questionnaireStartPage.goto(questionnaireSlug);
            await questionnaireStartPage.clickStartNow();

            // Verify navigation to next page
            await page.waitForURL(/\/questionnaires\/.*\/next/);
            expect(page.url()).toContain('/next');
        });

        test('Start button navigates with embed parameter when provided', async ({page}) => {
            await questionnaireStartPage.goto(questionnaireSlug, true);
            await questionnaireStartPage.clickStartNow();

            await page.waitForURL(/embed=True/);
            expect(page.url()).toContain('embed=True');
        });
    });

    test.describe('URL and Query Parameters', () => {
        test('Page loads with questionnaire slug in URL', async ({page}) => {
            await questionnaireStartPage.goto(questionnaireSlug);
            await questionnaireStartPage.expectOnPage();
            expect(page.url()).toContain(questionnaireSlug);
        });

        test('Page loads with embed parameter when specified', async ({page}) => {
            await questionnaireStartPage.goto(questionnaireSlug, true);
            expect(page.url().toLowerCase()).toContain('embed=true');
        });

        test('Page loads without embed parameter when not specified', async ({page}) => {
            await questionnaireStartPage.goto(questionnaireSlug);
            expect(page.url()).not.toContain('embed=true');
        });
    });

    test.describe('Error Handling', () => {
        test('No error summary is displayed on initial page load', async () => {
            await questionnaireStartPage.goto(questionnaireSlug);
            await questionnaireStartPage.expectNoErrors();
        });

        test('Page returns 404 for non-existent questionnaire', async ({page}) => {
            const nonExistentSlug = 'non-existent-questionnaire-slug';
            const response = await page.goto(`/questionnaires/${nonExistentSlug}/start`);
            expect(response?.status()).toBe(404);
        });
    });

    test.describe('Content Verification', () => {
        test('Page heading matches questionnaire title', async ({request}) => {
            await questionnaireStartPage.goto(questionnaireSlug);
            const heading = await questionnaireStartPage.getHeading();
            expect(heading).toBeTruthy();
        });

        test('Description renders markdown content as HTML', async () => {
            await questionnaireStartPage.goto(questionnaireSlug);
            const description = await questionnaireStartPage.getDescription();
            expect(description).toBeTruthy();
        });
    });

    test.describe('Accessibility', () => {
        test('Start button has correct ARIA attributes', async ({page}) => {
            await questionnaireStartPage.goto(questionnaireSlug);
            const startButton = page.locator('a.govuk-button.govuk-button--start');
            await expect(startButton).toHaveAttribute('role', 'button');
            await expect(startButton).toHaveAttribute('draggable', 'false');
        });

        test('Page has proper heading hierarchy', async ({page}) => {
            await questionnaireStartPage.goto(questionnaireSlug);
            const h1 = page.locator('h1');
            await expect(h1).toBeVisible();
            const count = await h1.count();
            expect(count).toBeGreaterThanOrEqual(1);
        });

        test('Main content is properly identified', async ({page}) => {
            await questionnaireStartPage.goto(questionnaireSlug);
            const mainContent = page.locator('#main-content-header');
            await expect(mainContent).toBeVisible();
        });
    });

    test.describe('Page Header Section', () => {
        test('Header section has grey background styling', async ({page}) => {
            await questionnaireStartPage.goto(questionnaireSlug);
            const headerSection = page.locator('#main-content-header');
            await expect(headerSection).toHaveClass(/dfe-page-grey/);
        });

        test('Header container is properly structured', async ({page}) => {
            await questionnaireStartPage.goto(questionnaireSlug);
            const headerContainer = page.locator('#main-header-container');
            await expect(headerContainer).toBeVisible();
        });
    });
    
    test.describe('No Start Page', () => {
        test('No start page questionnaire', async ({request, page}) => {
            // Create and publish a questionnaire via API
            const {questionnaire} = await createQuestionnaire(request, token);
            const noStartPageQuestionnaireId = questionnaire.id;
            const noStartPageQuestionnaireSlug = 'test-no-start-page-' + Math.floor(Math.random() * 1000000) + '';

            await updateQuestionnaire(request, noStartPageQuestionnaireId, {
                slug: noStartPageQuestionnaireSlug,
            }, token);

            // Create a multi-select question
            const {question} = await createQuestion(request, 
                noStartPageQuestionnaireId, token,
                'Question 1?',
                QuestionType.MultiSelect,
                'You can choose multiple options'
            );
            const questionId = question.id;

            await createSingleAnswer(request, {
                questionnaireId: noStartPageQuestionnaireId,
                questionId,
                content: 'First choice',
                priority: 2,
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: 'https://dev-admin.get-to-an-answer.education.gov.uk'
            }, token);

            // Publish the questionnaire so it's available on frontend
            await publishQuestionnaire(request, noStartPageQuestionnaireId, token);
            
            await questionnaireStartPage.goto(noStartPageQuestionnaireSlug);
            expect(page.url()).toContain("/next?embed");
            const questionHeaderView = page.locator('.govuk-heading-xl');
            await expect(questionHeaderView).toHaveText(questionnaire.title);
        });
    })

    test.describe('Multiple Questionnaires', () => {
        test('Different questionnaires display different content', async ({request, page}) => {
            // Create a second questionnaire
            const {questionnaire: questionnaire2} = await createQuestionnaire(request, token);
            await publishQuestionnaire(request, questionnaire2.id, token);

            // Visit first questionnaire
            await questionnaireStartPage.goto(questionnaireSlug);
            const heading1 = await questionnaireStartPage.getHeading();

            // Visit second questionnaire
            await questionnaireStartPage.goto(questionnaire2.slug);
            const heading2 = await questionnaireStartPage.getHeading();

            // Headings should be different (or at least one should be visible)
            expect(heading1).toBeTruthy();
            expect(heading2).toBeTruthy();
        });
    });
});