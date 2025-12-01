import {test} from '@playwright/test';
import {QuestionnaireSeeder} from "../../helpers/fedataseeder/QuetionnaireSeeder";
import {generateToken} from "../../helpers/JwtExamples";

test.describe('Get-to-an-answer - care leavers migration data seed', () => {
    test('seed questionnaire from JSON file for front-end', async ({request}) => {
        const token = generateToken();
        await QuestionnaireSeeder.seed(request, "check-your-care-leaver-support.json", token);
        console.log('Questionnaire seeded successfully');
    });
});