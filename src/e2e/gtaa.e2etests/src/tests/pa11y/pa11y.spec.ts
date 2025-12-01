import {APIRequestContext, expect, test} from "@playwright/test";
import {JwtHelper} from "../../helpers/JwtHelper";
import path from "path";
import fs from "fs/promises";
import {
    createQuestionnaire,
    getQuestionnaire,
    publishQuestionnaire,
    updateQuestionnaire
} from "../../test-data-seeder/questionnaire-data";
import {createContent} from "../../test-data-seeder/content-data";
import {createQuestion} from "../../test-data-seeder/question-data";
import {AnswerDestinationType} from "../../constants/test-data-constants";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {EnvConfig} from "../../config/environment-config";
import puppeteer, { Page } from "puppeteer";
import pa11y from "pa11y";

const API_URL = EnvConfig.API_URL;
const ADMIN_URL = EnvConfig.ADMIN_URL;
const FRONTEND_URL = EnvConfig.FE_URL;
test.describe('Get to an answer Pa11y Accessibility Test', () => {
    test.describe.configure({ timeout: 5 * 60 * 1000 });

    let urlsToTest: string[] = [];
    let token = JwtHelper.NoRecordsToken();
    
    test.beforeEach(async ({request}) => {
        // Get base URL from the page context
        const entityIds: any = await loadSeeding(request, token);
        // Fetch URLs from sitemap
        urlsToTest = await fetchUrlsFromSitemap(request, `${ADMIN_URL}/sitemap.xml`, entityIds);
        console.log(`Fetched ${urlsToTest.length} URLs from sitemap`);
    });

    test('run Pa11y accessibility tests on all sitemap URLs', async () => {
        const browser = await puppeteer.launch({
            ignoreHTTPSErrors: true
        } as any);
        
        try {
            const page: Page = await browser.newPage();
            await page.goto(`${ADMIN_URL}/dev/login?jt=${token}`, {waitUntil: 'networkidle0'});

            const results: any[] = [];
            const failures: string[] = [];

            // Ensure screenshots directory exists
            const screenshotsDir = path.join(__dirname, '../../reports/pa11y-screenshots');
            await fs.mkdir(screenshotsDir, { recursive: true });

            // Pa11y configuration
            const pa11yOptions: any = {
                browser,
                standard: 'WCAG2AA', // Test against WCAG 2.1 Level AA
                timeout: 30000,
                wait: 1000, // Wait for dynamic content
                // screenCapture: undefined, // enabled
                includeNotices: false,
                includeWarnings: true,
                runners: ['axe', 'htmlcs'], // Use both Axe and HTML_CodeSniffer
            };

            // Helper to create a safe filename from URL
            const getScreenshotPathForUrl = (url: string): string => {
                const safeName = url
                    .replace(/^https?:\/\//, '')
                    .replace(/[^a-zA-Z0-9\-_.]/g, '_')
                    .slice(0, 150); // avoid overly long filenames
                return path.join(screenshotsDir, `${safeName}.png`);
            };
            
            await Promise.all(urlsToTest.map(async (url) => {
                console.log(`\nTesting ${url}...`);
                
                try {
                    const screenshotPath = getScreenshotPathForUrl(url)

                    const result = await pa11y(url, {
                        ...pa11yOptions,
                        screenCapture: screenshotPath
                    });

                    results.push({
                        url,
                        issueCount: result.issues.length,
                        issues: result.issues,
                        screenshot: screenshotPath
                    });

                    // Log issues
                    if (result.issues.length > 0) {
                        console.log(`  Found ${result.issues.length} accessibility issues:`);
                        result.issues.forEach((issue: any, index: number) => {
                            console.log(`  ${index + 1}. [${issue.type}] ${issue.message}`);
                            console.log(`     Selector: ${issue.selector}`);
                            console.log(`     Code: ${issue.code}`);
                        });
                        failures.push(url);
                    } else {
                        console.log(`  ✓ No accessibility issues found`);
                    }
                } catch (error) {
                    console.error(`  ✗ Error testing ${url}:`, error);
                    failures.push(url);
                    results.push({
                        url,
                        error: error instanceof Error ? error.message : String(error)
                    });
                }
            }))

            // Generate summary report
            console.log('\n=== Pa11y Test Summary ===');
            console.log(`Total URLs tested: ${urlsToTest.length}`);
            console.log(`URLs with issues: ${failures.length}`);
            console.log(`URLs passed: ${urlsToTest.length - failures.length}`);

            // Save detailed results to file
            const reportPath = path.join(__dirname, '../../reports/pa11y-results.json');
            await fs.mkdir(path.dirname(reportPath), {recursive: true});
            await fs.writeFile(reportPath, JSON.stringify(results, null, 2));
            console.log(`\nDetailed report saved to: ${reportPath}`);

            await generateReport(results)

            // Assert that there are no accessibility issues
            expect(failures.length, `Accessibility issues found on ${failures.length} page(s): ${failures.join(', ')}`).toBe(0);
        } catch (error) {
            //console.error('Error running Pa11y tests:', error);
            throw error;
        } finally {
            await browser.close();
        }
    });
});

// Helper function to fetch and parse sitemap
async function fetchUrlsFromSitemap(
    request: APIRequestContext,
    sitemapUrl: string, 
    entityIds: { 
        questionnaireId:any, 
        firstQuestionId:any, 
        contentId:any
    }
): Promise<string[]> {
    try {
        const response = await request.get(sitemapUrl, {
            headers: {
                'Accept': 'application/xml, text/xml'
            }
        });

        if (!response.ok) {
            throw new Error(`Failed to fetch sitemap: ${response.status} ${response.statusText}`);
        }

        const xmlText = await response.text();

        // Parse XML to extract URLs
        const urlMatches = xmlText.matchAll(/<loc>(.*?)<\/loc>/g);
        const urls: string[] = [];

        for (const match of urlMatches) {
            let url = match[1].trim();
            if (url) {
                url = url.replace("{questionnaireId}", entityIds.questionnaireId);
                url = url.replace("{questionId}", entityIds.firstQuestionId);
                url = url.replace("{contentId}", entityIds.contentId);
                urls.push(url);
            }
        }

        return urls;
    } catch (error) {
        console.error('Error fetching sitemap:', error);
        return [];
    }
}

async function generateReport(results: any[] = []): Promise<void> {
    // Generate HTML report
    const totalIssues = results.reduce((sum, r) => sum + (r.issueCount > 0 ? r.issueCount : 0), 0);
    
    // Prepare results with screenshot paths suitable for HTML
    const resultsWithScreenshotPaths = results.map((result: any) => ({
        ...result,
        screenshotForHtml: result.screenshot
            ? `./pa11y-screenshots/${path.basename(result.screenshot)}`
            : undefined
    }));

    const htmlReport = `
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Pa11y Accessibility Report</title>
        <style>
            body { font-family: Arial, sans-serif; max-width: 1200px; margin: 20px auto; padding: 0 20px; }
            h1 { color: #333; }
            .summary { background: #f5f5f5; padding: 15px; border-radius: 5px; margin-bottom: 20px; }
            .summary-stats { display: flex; gap: 20px; flex-wrap: wrap; }
            .stat { background: white; padding: 15px; border-radius: 5px; flex: 1; min-width: 150px; }
            .stat-value { font-size: 2em; font-weight: bold; color: #0288d1; }
            .url-section { margin-bottom: 30px; border: 1px solid #ddd; padding: 20px; border-radius: 5px; }
            .url-header { background: #e3f2fd; padding: 10px; margin: -20px -20px 15px -20px; border-radius: 5px 5px 0 0; }
            .issue { border: 1px solid #ddd; padding: 15px; margin-bottom: 15px; border-radius: 5px; }
            .error { border-left: 4px solid #d32f2f; }
            .warning { border-left: 4px solid #f57c00; }
            .notice { border-left: 4px solid #0288d1; }
            .issue-type { font-weight: bold; text-transform: uppercase; }
            .error .issue-type { color: #d32f2f; }
            .warning .issue-type { color: #f57c00; }
            .notice .issue-type { color: #0288d1; }
            .selector { background: #f5f5f5; padding: 5px; font-family: monospace; font-size: 0.9em; word-break: break-all; }
            .context { background: #fafafa; padding: 10px; margin-top: 10px; border-left: 3px solid #ccc; }
            .success { color: #4caf50; font-weight: bold; }
            .error-message { color: #d32f2f; font-weight: bold; }
            .screenshot-container { margin: 10px 0 20px; }
            .screenshot-container img { max-width: 100%; border: 1px solid #ccc; border-radius: 4px; }
            .screenshot-label { font-size: 0.9em; color: #555; margin-bottom: 5px; }
        </style>
    </head>
    <body>
        <h1>Pa11y Accessibility Report</h1>
        <div class="summary">
            <h2>Summary</h2>
            <p><strong>Date:</strong> ${new Date().toISOString()}</p>
            <p><strong>Standard:</strong> WCAG 2.1 Level AA</p>
            <div class="summary-stats">
                <div class="stat">
                    <div class="stat-value">${resultsWithScreenshotPaths.length}</div>
                    <div>URLs Tested</div>
                </div>
                <div class="stat">
                    <div class="stat-value">${totalIssues}</div>
                    <div>Total Issues</div>
                </div>
                <div class="stat">
                    <div class="stat-value">${resultsWithScreenshotPaths.filter(r => r.issueCount === 0).length}</div>
                    <div>Pages Passed</div>
                </div>
            </div>
        </div>
        
        ${resultsWithScreenshotPaths.map((result: any) => `
            <div class="url-section">
                <div class="url-header">
                    <h3>${result.url}</h3>
                    ${result.error ?
            `<p class="error-message">❌ Error: ${result.error}</p>` :
            result.issueCount === 0 ?
                '<p class="success">✓ No accessibility issues found!</p>' :
                `<p>Found ${result.issueCount} issue(s)</p>`}
                </div>

                ${result.screenshotForHtml ? `
                    <div class="screenshot-container">
                        <div class="screenshot-label">Screenshot</div>
                        <a href="${result.screenshotForHtml}" target="_blank" rel="noopener">
                            <img src="${result.screenshotForHtml}" alt="Screenshot for ${result.url}">
                        </a>
                    </div>
                ` : ''}
                    
                ${result.issues && result.issues.length > 0 ? result.issues.map((issue: any, index: number) => `
                    <div class="issue ${issue.type}">
                        <p><span class="issue-type">${issue.type}</span> #${index + 1}</p>
                        <p><strong>Message:</strong> ${issue.message}</p>
                        <p><strong>Code:</strong> ${issue.code}</p>
                        <p><strong>Selector:</strong> <code class="selector">${issue.selector}</code></p>
                        ${issue.context ? `<div class="context"><strong>Context:</strong><br><code>${issue.context.replace(/</g, '&lt;').replace(/>/g, '&gt;')}</code></div>` : ''}
                    </div>
                `).join('') : ''}
            </div>
        `).join('')}
    </body>
    </html>`;

    const reportPath = path.join(__dirname, '../../reports/pa11y-report.html');
    await fs.mkdir(path.dirname(reportPath), { recursive: true });
    await fs.writeFile(reportPath, htmlReport);

    console.log(`HTML report saved to: ${reportPath}`);
    console.log(`Total issues found: ${totalIssues}`);
}

async function loadSeeding(request: APIRequestContext, token: string): Promise<object> {
    const entityIds: any = {}
    
    const jsonFilePath = path.join(__dirname, '../../helpers/fedataseeder/questionnaire.json');
    const jsonText = await fs.readFile(jsonFilePath, 'utf8');
    const data = JSON.parse(jsonText);

    // Create questionnaire
    const apiQuestionnaireResponse = await createQuestionnaire(request, token, data.title, data.description);
    const questionnaireId = apiQuestionnaireResponse.questionnaire.id;

    entityIds.questionnaireId = questionnaireId;

    const newSlug = `updated-questionnaire-slug-${Math.floor(Math.random() * 1000000000)}`;
    await updateQuestionnaire(
        request,
        questionnaireId,
        {
            slug: newSlug
        }, token
    );

    const {questionnaireGetBody} = await getQuestionnaire(request, questionnaireId, token);
    const questionnaireSlug = questionnaireGetBody.slug;
    console.log('Questionnaire updated slug:', questionnaireSlug);

    await publishQuestionnaire(request, questionnaireId);

    const apiContentResponse = await createContent(request, {
        questionnaireId: questionnaireId,
        title: 'Test Content',
        content: 'This is a test content for the start page.',
        referenceName: 'test-content'
    }, token)

    // Create all questions first
    const questionIds: Record<string, string> = {};

    for (const q of data.questions) {
        const {question} = await createQuestion(request, questionnaireId, token, q.text, q.type);
        questionIds[q.key] = question.id;

        entityIds.firstQuestionId = entityIds.firstQuestionId || question.id;
    }

    // Get the SINGLE internal content ID once
    const internalContentId = apiContentResponse.content.id;

    entityIds.contentId = internalContentId;

    // Create answers for every question
    for (const q of data.questions) {
        const questionId = questionIds[q.key];

        for (const answer of q.answers) {
            const nav = answer.navigation;

            if (nav) {
                let destinationType: AnswerDestinationType | undefined;
                let destinationQuestionId: string | undefined;
                let destinationContentId: string | undefined;
                let destinationUrl: string | undefined;

                // Determine navigation type and set appropriate values
                if (nav.type === 'next-question') {
                    destinationType = AnswerDestinationType.Question;
                    destinationQuestionId = questionIds[nav.targetQuestionKey];
                } else if (nav.type === 'external-link') {
                    destinationType = AnswerDestinationType.ExternalLink;
                    destinationUrl = nav.url;
                } else if (nav.type === 'internal-link') {
                    destinationType = AnswerDestinationType.CustomContent;
                    destinationContentId = internalContentId;
                }

                await createSingleAnswer(request, {
                    questionId,
                    questionnaireId,
                    content: answer.text,
                    description: undefined,
                    priority: undefined,
                    destinationType,
                    destinationQuestionId,
                    destinationContentId,
                    destinationUrl
                }, token);
            }
        }
    }
    console.log('Seeding completed for questionnaire:', questionnaireId);
    await publishQuestionnaire(request, questionnaireId, token);
    
    return entityIds;
}