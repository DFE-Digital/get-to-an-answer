// import {test, expect} from '@playwright/test';
// import {createQuestionnaire} from '../../test-data-seeder/questionnaire-data';
// import {createQuestion} from '../../test-data-seeder/question-data';
// import {createSingleAnswer} from '../../test-data-seeder/answer-data';
// import {QuestionType, QuestionPrefix, AnswerDestinationType, GUID_REGEX} from '../../constants/test-data-constants';
//
// test.describe('POST answers api tests', () => {
//     test('POST an answer for a single question', async ({request}) => {
//
//         const {questionnaire} = await createQuestionnaire(request);
//         const questionnaireId = await questionnaire.id;
//
//         const {questionPostResponse} = await createQuestion(request, questionnaireId)
//         const qId = await questionPostResponse.id;
//
//         const {answerPostResponse, payload} = await createSingleAnswer(
//             request,
//             {
//                 questionId: qId,
//                 content: undefined,
//                 description: undefined,
//                 answerPrefix: 'Option 1',
//                 weight: 0.0,
//                 destinationType: AnswerDestinationType.PAGE,
//                 destination: '/page-destination-url'
//             }
//         );
//
//         // --- HTTP-level checks ---
//         expect(answerPostResponse.ok()).toBeTruthy();
//         expect(answerPostResponse.status()).toBe(201);
//
//         // --- Schema-level checks ---
//         expect(answerPostResponse).toHaveProperty('questionId');
//         expect(answerPostResponse).toHaveProperty('content');
//         expect(answerPostResponse).toHaveProperty('description');
//         expect(answerPostResponse).toHaveProperty('destination');
//         expect(answerPostResponse).toHaveProperty('destinationType');
//         expect(answerPostResponse).toHaveProperty('weight');
//
//         // --- Type sanity checks ---
//         expect(typeof questionPostResponse.questionnaireId).toMatch(GUID_REGEX);
//         expect(typeof questionPostResponse.content).toBe('string');
//         expect(typeof questionPostResponse.description).toBe('string');
//         expect(typeof questionPostResponse.type).toBe(QuestionType);
//
//         // --- Basic content sanity ---
//         expect(questionPostResponse.content.trim().length).toBeGreaterThan(0);
//         expect(questionPostResponse.description.trim().length).toBeGreaterThan(0);
//         expect(questionPostResponse.type.trim().length).toBeGreaterThan(0);
//     });
// });