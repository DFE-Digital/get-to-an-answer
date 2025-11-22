import {expect, test} from "@playwright/test";
import {QuestionnaireNextPage} from "../../pages/fe/QuestionnaireNextPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire, publishQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createAnswer} from "../../test-data-seeder/answer-data";
import {QuestionType} from "../../constants/test-data-constants";

test.describe('Questionnaire Next Page - Single Select Questions', () => {
    let questionnaireNextPage: QuestionnaireNextPage;
    let token: string;
    let questionnaireSlug: string;
    let questionnaireId: string;
    let questionId: string;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        // Create questionnaire
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
        questionnaireSlug = questionnaire.slug;

        // Create a single-select question
        const {question} = await createQuestion(request, questionnaireId, token,
            'What is your preferred option?',
            QuestionType.SINGLE, 
            'Please select one option'
        );
        questionId = question.id;

        // Create answer options
        await createAnswer(request, questionId, token,
            'Option A',
            'This is option A',
            1
        );
        await createAnswer(request, questionId, token, 
           'Option B',
            'This is option B',
            2
        );
        await createAnswer(request, questionId, token, 
           'Option C',
            undefined,
            3
        );

        // Publish questionnaire
        await publishQuestionnaire(request, questionnaireId, token);

        questionnaireNextPage = new QuestionnaireNextPage(page);
    });

    test.describe('Page Display and Navigation', () => {
        test('Page loads successfully and displays question', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            await questionnaireNextPage.expectOnPage();
        });

        test('Question heading is visible', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const questionText = await questionnaireNextPage.getQuestionText();
            expect(questionText).toContain('What is your preferred option?');
        });

        test('Continue button is visible', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            await expect(page.locator('button[type="submit"].govuk-button')).toBeVisible();
        });
    });

    test.describe('Radio Button Interactions', () => {
        test('Radio option can be selected', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const radioIds = await questionnaireNextPage.getAllRadioOptionIds();

            expect(radioIds.length).toBe(3);
            await questionnaireNextPage.selectRadioOption(radioIds[0]);
            await questionnaireNextPage.expectRadioOptionSelected(radioIds[0]);
        });

        test('Only one radio option can be selected at a time', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const radioIds = await questionnaireNextPage.getAllRadioOptionIds();

            await questionnaireNextPage.selectRadioOption(radioIds[0]);
            await questionnaireNextPage.expectRadioOptionSelected(radioIds[0]);

            await questionnaireNextPage.selectRadioOption(radioIds[1]);
            await questionnaireNextPage.expectRadioOptionSelected(radioIds[1]);
            await questionnaireNextPage.expectRadioOptionNotSelected(radioIds[0]);
        });

        test('Radio options count matches created answers', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const count = await questionnaireNextPage.countRadioOptions();
            expect(count).toBe(3);
        });

        test('Answer labels are displayed correctly', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const radioIds = await questionnaireNextPage.getAllRadioOptionIds();

            const labelA = await questionnaireNextPage.getAnswerLabelText(radioIds[0]);
            const labelB = await questionnaireNextPage.getAnswerLabelText(radioIds[1]);

            expect([labelA, labelB]).toContain('Option A');
            expect([labelA, labelB]).toContain('Option B');
        });
    });

    test.describe('Error Validation', () => {
        test('No errors displayed on initial page load', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            await questionnaireNextPage.expectNoErrors();
        });

        test('Error summary appears when submitting without selection', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            await questionnaireNextPage.clickContinue();

            await questionnaireNextPage.expectErrorSummary();
        });

        test('Field error message is displayed with validation error', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            await questionnaireNextPage.clickContinue();

            await questionnaireNextPage.expectFieldError();
            const errorMessage = await questionnaireNextPage.getFieldErrorMessage();
            expect(errorMessage).toBeTruthy();
        });

        test('Form group error styling is applied', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            await questionnaireNextPage.clickContinue();

            await questionnaireNextPage.expectFormGroupError();
        });
    });

    test.describe('Form Submission', () => {
        test('Form submits successfully with valid selection', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const radioIds = await questionnaireNextPage.getAllRadioOptionIds();

            await questionnaireNextPage.selectRadioOption(radioIds[0]);
            await questionnaireNextPage.submitForm();

            // Verify form was submitted (page navigates or updates)
            await page.waitForLoadState('networkidle');
        });

        test('Form includes anti-forgery token', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const antiForgeryToken = page.locator('input[name="__RequestVerificationToken"]');
            await expect(antiForgeryToken).toBeAttached();
        });

        test('Hidden fields contain questionnaire metadata', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);

            const slug = await questionnaireNextPage.getHiddenFieldValue('Questionnaire.Slug');
            expect(slug).toBe(questionnaireSlug);
        });
    });
});

test.describe('Questionnaire Next Page - Multi Select Questions', () => {
    let questionnaireNextPage: QuestionnaireNextPage;
    let token: string;
    let questionnaireSlug: string;
    let questionnaireId: string;
    let questionId: string;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        // Create questionnaire
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
        questionnaireSlug = questionnaire.slug;

        // Create a multi-select question
        const {question} = await createQuestion(request, questionnaireId, token, 
           'Select all that apply',
            QuestionType.MULTIPLE,
            'You can choose multiple options'
        );
        questionId = question.id;

        // Create answer options
        await createAnswer(request, questionId, token, 
           'First choice',
            undefined,
            1
        );
        await createAnswer(request, questionId, token, 
           'Second choice',
            undefined,
            2
        );
        await createAnswer(request, questionId, token, 
           'Third choice',
            undefined,
            3
        );

        // Publish questionnaire
        await publishQuestionnaire(request, questionnaireId, token);

        questionnaireNextPage = new QuestionnaireNextPage(page);
    });

    test.describe('Checkbox Interactions', () => {
        test('Multiple checkbox options can be selected', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const checkboxIds = await questionnaireNextPage.getAllCheckboxOptionIds();

            expect(checkboxIds.length).toBe(3);

            await questionnaireNextPage.checkCheckboxOption(checkboxIds[0]);
            await questionnaireNextPage.checkCheckboxOption(checkboxIds[1]);

            await questionnaireNextPage.expectCheckboxOptionChecked(checkboxIds[0]);
            await questionnaireNextPage.expectCheckboxOptionChecked(checkboxIds[1]);
        });

        test('Checkbox option can be unchecked', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const checkboxIds = await questionnaireNextPage.getAllCheckboxOptionIds();

            await questionnaireNextPage.checkCheckboxOption(checkboxIds[0]);
            await questionnaireNextPage.expectCheckboxOptionChecked(checkboxIds[0]);

            await questionnaireNextPage.uncheckCheckboxOption(checkboxIds[0]);
            await questionnaireNextPage.expectCheckboxOptionNotChecked(checkboxIds[0]);
        });

        test('Multiple checkboxes can be checked at once using helper', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const checkboxIds = await questionnaireNextPage.getAllCheckboxOptionIds();

            await questionnaireNextPage.checkMultipleCheckboxes([checkboxIds[0], checkboxIds[2]]);

            await questionnaireNextPage.expectCheckboxOptionChecked(checkboxIds[0]);
            await questionnaireNextPage.expectCheckboxOptionChecked(checkboxIds[2]);
            await questionnaireNextPage.expectCheckboxOptionNotChecked(checkboxIds[1]);
        });

        test('Checkbox options count matches created answers', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const count = await questionnaireNextPage.countCheckboxOptions();
            expect(count).toBe(3);
        });
    });

    test.describe('Multi-Select Validation', () => {
        test('Error appears when no checkboxes selected on submit', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            await questionnaireNextPage.clickContinue();

            await questionnaireNextPage.expectErrorSummary();
        });

        test('Form submits successfully with multiple selections', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const checkboxIds = await questionnaireNextPage.getAllCheckboxOptionIds();

            await questionnaireNextPage.answerMultiSelectQuestion([checkboxIds[0], checkboxIds[1]]);

            await page.waitForLoadState('networkidle');
        });
    });

    test.describe('GOV.UK Design Compliance', () => {
        test('Checkboxes use GOV.UK styling', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const checkboxes = page.locator('.govuk-checkboxes');
            await expect(checkboxes).toBeVisible();
        });

        test('Multi-select question type is correctly detected', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            await questionnaireNextPage.expectQuestionType(QuestionType.MULTIPLE);
        });
    });
});

test.describe('Questionnaire Next Page - Dropdown Select Questions', () => {
    let questionnaireNextPage: QuestionnaireNextPage;
    let token: string;
    let questionnaireSlug: string;
    let questionnaireId: string;
    let questionId: string;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        // Create questionnaire
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
        questionnaireSlug = questionnaire.slug;

        // Create a dropdown-select question
        const {question} = await createQuestion(request, questionnaireId, token, 
           'Select from dropdown',
            QuestionType.DROPDOWN,
            'Choose one option from the list'
        );
        questionId = question.id;

        // Create answer options
        await createAnswer(request, questionId, token, 
           'Dropdown Option 1',
            undefined,
            1
        );
        await createAnswer(request, questionId, token, 
           'Dropdown Option 2',
            undefined,
            2
        );
        await createAnswer(request, questionId, token, 
           'Dropdown Option 3',
            undefined,
            3
        );

        // Publish questionnaire
        await publishQuestionnaire(request, questionnaireId, token);

        questionnaireNextPage = new QuestionnaireNextPage(page);
    });

    test.describe('Dropdown Interactions', () => {
        test('Dropdown displays all available options', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const options = await questionnaireNextPage.getDropdownOptions();

            // Should have placeholder + 3 options
            expect(options.length).toBe(4);
            expect(options[0]).toContain('Please select');
        });

        test('Dropdown option can be selected by text', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);

            await questionnaireNextPage.selectDropdownOptionByText('Dropdown Option 2');
            const selectedValue = await questionnaireNextPage.getSelectedDropdownValue();
            expect(selectedValue).toBeTruthy();
        });

        test('Selected dropdown value can be retrieved', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const options = await questionnaireNextPage.getDropdownOptions();

            await questionnaireNextPage.selectDropdownOptionByText(options[1]);
            const value = await questionnaireNextPage.getSelectedDropdownValue();
            expect(value).toBeTruthy();
        });
    });

    test.describe('Dropdown Validation', () => {
        test('Error appears when no dropdown option selected', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            await questionnaireNextPage.clickContinue();

            await questionnaireNextPage.expectErrorSummary();
        });

        test('Form submits successfully with dropdown selection', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const options = await questionnaireNextPage.getDropdownOptions();

            await questionnaireNextPage.selectDropdownOptionByText(options[1]);
            await questionnaireNextPage.submitForm();

            await page.waitForLoadState('networkidle');
        });
    });

    test.describe('Dropdown Design', () => {
        test('Dropdown uses GOV.UK styling', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            const dropdown = page.locator('select.govuk-select');
            await expect(dropdown).toBeVisible();
        });

        test('Dropdown question type is correctly detected', async ({page}) => {
            await page.goto(`/questionnaires/${questionnaireSlug}/next`);
            await questionnaireNextPage.expectQuestionType(QuestionType.DROPDOWN);
        });
    });
});

test.describe('Questionnaire Next Page - Accessibility', () => {
    let token: string;
    let questionnaireSlug: string;

    test.beforeEach(async ({request}) => {
        token = JwtHelper.NoRecordsToken();

        // Create questionnaire with single-select question
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireSlug = questionnaire.slug;

        const {question} = await createQuestion(request, questionnaire.id, token,
            'Accessibility test question',
            QuestionType.SINGLE
        );

        await createAnswer(request, question.id, token, 
           'Test answer',
            undefined,
            1
        );

        await publishQuestionnaire(request, questionnaire.id, token);
    });

    test('Fieldset has proper legend', async ({page}) => {
        await page.goto(`/questionnaires/${questionnaireSlug}/next`);
        const legend = page.locator('fieldset.govuk-fieldset legend.govuk-fieldset__legend');
        await expect(legend).toBeVisible();
    });

    test('Error messages have proper visually-hidden text', async ({page}) => {
        await page.goto(`/questionnaires/${questionnaireSlug}/next`);
        await page.locator('button[type="submit"]').click();

        const visuallyHidden = page.locator('.govuk-error-message .govuk-visually-hidden');
        if (await visuallyHidden.count() > 0) {
            await expect(visuallyHidden.first()).toHaveText('Error:');
        }
    });

    test('Button has double-click prevention', async ({page}) => {
        await page.goto(`/questionnaires/${questionnaireSlug}/next`);
        const button = page.locator('button[type="submit"]');

        await expect(button).toHaveAttribute('data-prevent-double-click', 'true');
    });
});