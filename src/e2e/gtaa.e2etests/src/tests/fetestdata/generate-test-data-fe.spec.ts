import {test} from '@playwright/test';
import fs from 'fs/promises';
import path from 'path';

import {
    createQuestionnaire,
    getQuestionnaire,
    publishQuestionnaire,
    updateQuestionnaire
} from '../../test-data-seeder/questionnaire-data';
import {createQuestion} from '../../test-data-seeder/question-data';
import {createAnswer, createSingleAnswer} from '../../test-data-seeder/answer-data';
import {JwtHelper} from "../../helpers/JwtHelper";
import {createContent} from "../../test-data-seeder/content-data";
import {AnswerDestinationType, QuestionType} from "../../constants/test-data-constants";
import {expect200HttpStatusCode} from "../../helpers/api-assertions-helper";

test.describe('Get to an answer - fe test data generation', () => {
    let token: string;
    let data: any;
    let apiQuestionnaireResponse: any;
    let questionnaireId: any;
    let apiContentResponse: any;

    test.beforeEach(async ({request}) => {
        token = JwtHelper.NoRecordsToken();
        
        // Read JSON using absolute path
        const jsonFilePath = path.join(__dirname, '../../helpers/fedataseeder/questionnaire.json');
        const jsonText = await fs.readFile(jsonFilePath, 'utf8');
        data = JSON.parse(jsonText);

        // Create questionnaire
        apiQuestionnaireResponse = await createQuestionnaire(request, token, data.title, data.description);
        questionnaireId = apiQuestionnaireResponse.questionnaire.id;

        const newSlug = `updated-questionnaire-slug-${Math.floor(Math.random() * 1000000000)}`;
        const {updatedQuestionnairePostResponse} = await updateQuestionnaire(
            request,
            questionnaireId,
            {
                slug: newSlug
            }, token
        );

        const {questionnaireGetBody} = await getQuestionnaire(request, questionnaireId, token);
        console.log('Questionnaire updated slug:', questionnaireGetBody.slug);

        await publishQuestionnaire(request, questionnaireId);

        apiContentResponse = await createContent(request, {
            questionnaireId: questionnaireId,
            title: 'Test Content',
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)
    });

    test('seed questionnaire from JSON file for front-end', async ({request}) => {
        let questionId: string | undefined;
        let destinationType: AnswerDestinationType | undefined;
        let destinationUrl: string | undefined;
        let destinationContentId: string | undefined;
        let destinationQuestionId: string | undefined;

        // Create all questions first
        const questionIds: Record<string, string> = {};

        for (const q of data.questions) {
            const {question} = await createQuestion(request, questionnaireId,token, q.text, q.type);
            questionIds[q.key] = question.id;
        }

        // Get the SINGLE internal content ID once
        const internalContentId = apiContentResponse.content.id;

        // Create answers for every question
        for (const q of data.questions) {
            questionId = questionIds[q.key];
            let priority = 1;

            for (const answer of q.answers) {
                const nav = answer.navigation;

                if (nav) {
                    if (nav.type === 'next-question') {
                        destinationType = AnswerDestinationType.Question;
                        destinationQuestionId = questionIds[nav.targetQuestionKey];
                        await createSingleAnswer(request, {
                            questionId,
                            questionnaireId,
                            destinationQuestionId,
                            destinationContentId,
                            content: answer.text,
                            description: undefined,
                            priority,
                            destinationType,
                            destinationUrl
                        }, token);

                    }

                    if (nav.type === 'external-link') {
                        destinationType = AnswerDestinationType.ExternalLink;
                        destinationUrl = nav.url;
                        await createSingleAnswer(request,
                            {
                                questionId,
                                questionnaireId,
                                content: answer.text,
                                description: undefined,
                                priority,
                                destinationType,
                                destinationUrl,
                            }, token);

                    }

                    if (nav.type === 'internal-link') {
                        destinationType = AnswerDestinationType.CustomContent;

                        await createSingleAnswer(request, {
                            questionId,
                            questionnaireId,
                            content: answer.text,
                            description: undefined,
                            priority,
                            destinationType: AnswerDestinationType.CustomContent,
                            destinationContentId: internalContentId
                        }, token);
                    }
                }
                priority++;
            }
        }
        console.log('Seeding completed for questionnaire:', questionnaireId);

        await publishQuestionnaire(request, questionnaireId, token);
    });
});