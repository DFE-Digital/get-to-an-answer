import {AnswerBuilder} from '../builders/AnswerBuilder';
import {AnswerDestinationType} from '../constants/test-data-constants'
import {JwtHelper} from "../helpers/JwtHelper";
import {APIResponse} from "@playwright/test";

//to parse response-body correctly - json body can be json, text or empty string
//duplicate - to be fixed later
async function safeParseBody(response: APIResponse) {
    const ct = (response.headers()['content-type'] || '').toLowerCase();

    try {
        const raw = await response.text();

        // If empty body, return null
        if (!raw || raw.trim() === '') {
            return null;
        }

        // If JSON content type, try to parse
        if (ct.includes('application/json')) {
            try {
                return JSON.parse(raw);
            } catch (parseError) {
                console.error('JSON parse error:', parseError);
                console.error('Raw text:', raw);
                return null;
            }
        }

        // Return raw text for non-JSON responses
        return raw;
    } catch (error) {
        console.error('Error reading response body:', error);
        return null;
    }
}

// export async function createMultipleAnswers(
//     request: any,
//     questionId: string,
//     questionnaireId: string,
//     numberOfAnswers: number,
//     useDifferentDestinations?: boolean,
//     bearerToken?: string,
// ) {
//     const createdAnswers = [];
//     const createdPayloads = [];
//    
//     const destinationType = AnswerDestinationType.PAGE;
//     const destination = '/default-destination'
//     const score = 0.0;
//
//     for (let i = 1; i <= numberOfAnswers; i++) {
//         const content = `Auto-generated answer content - Choice ${i}`;
//         const description = `Auto-generated description - option ${i}`;
//
//         // Either same destination or /default-destination-1, /default-destination-2, etc.
//         const finalDestination = useDifferentDestinations ? `${destination}-${i}` : destination;
//
//         const payload = new AnswerBuilder(questionId, questionnaireId)
//             .withContent(content)
//             .withDescription(description)
//             .withDestinationUrl(finalDestination)
//             .withDestinationType(destinationType)
//             .withScore(score)
//             .build();
//
//         const response = await request.post('/api/answers', {
//             data: payload,
//             headers: {
//                 'Content-Type': 'application/json',
//                 'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
//             }
//         });
//
//         if (!response.ok()) {
//             throw new Error(`❌ Failed to create answer ${i}: ${response.status()}`);
//         }
//
//         const body = await response.json();
//         createdAnswers.push(body);
//         createdPayloads.push(payload);
//     }
//
//     console.log(
//         `✅ Created ${numberOfAnswers} ${useDifferentDestinations ? 'different' : 'same'} outcomes for question ${questionId}`
//     );
//     return {createdAnswers, createdPayloads};
// }

interface CreateAnswerRequest {
    questionId: string;
    questionnaireId: string;
    destinationQuestionId?: string;
    content?: string;
    description?: string;
    answerPrefix?: string;
    score?: number;
    destinationType?: AnswerDestinationType;
    destinationUrl?: string;
}

export async function createSingleAnswer(
    request: any,
    answerRequest: CreateAnswerRequest,
    bearerToken?: string,
) {
    const payload = new AnswerBuilder(answerRequest.questionId, answerRequest.questionnaireId)
        .withDestinationQuestionId(answerRequest.destinationQuestionId)
        .withContent(answerRequest.content)
        .withDescription(answerRequest.description)
        .withDestinationUrl(answerRequest.destinationUrl)
        .withDestinationType(answerRequest.destinationType)
        .withScore(answerRequest.score)
        .build();

    const response = await request.post('/api/answers', {
        data: payload,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);
    
    return {
        answerPostResponse: response,
        answer: responseBody,
        payload
    }
}

export async function getAnswer(
    request: any,
    answerId: string,
    bearerToken?: string,
) {
    const response = await request.get(`/api/answers/${answerId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

    return {
        answerGetResponse: response,
        answer: responseBody
    }
}

export async function listAnswers(
    request: any,
    questionId: string,
    bearerToken?: string,
) {
    const response = await request.get(`/api/questions/${questionId}/answers`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

    return {
        answersGetResponse: response,
        answers: responseBody
    }
}

export async function updateAnswer(
    request: any,
    answerId: string,
    data: any,
    bearerToken?: string,
) {
    const response = await request.put(`/api/answers/${answerId}`, {
        data,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

    return {
        updatedAnswerPostResponse: response,
        updatedAnswer: responseBody
    }
}

export async function deleteAnswer(
    request: any,
    answerId: number,
    bearerToken?: string,
) {
    const response = await request.delete(`/api/answers/${answerId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to delete answer: ${response.status()}`);
    }

    return await response.json();
}