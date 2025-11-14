import {AnswerBuilder} from '../builders/AnswerBuilder';
import {AnswerDestinationType} from '../constants/test-data-constants'
import {JwtHelper} from "../helpers/JwtHelper";
import {APIResponse} from "@playwright/test";
import {parseBody} from "../helpers/ParseBody";
import { EnvConfig } from '../config/environment-config';

const BASE_URL = EnvConfig.API_URL;

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

interface CreateAnswerRequest {
    questionId: string;
    questionnaireId: string;
    destinationQuestionId?: string;
    content?: string;
    description?: string;
    answerPrefix?: string;
    priority?: number;
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
        .withScore(answerRequest.priority)
        .build();

    const response = await request.post(`${BASE_URL}/api/answers`, {
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

export async function createAnswer(
    request: any,
    questionnaireId: string,
    questionId: string,
    content?: string,
    description?: string,
    priority?: number,
    destinationType?: AnswerDestinationType,
    destinationQuestionId?: string,
    destinationUrl?: string,
) {
    return await createSingleAnswer(request, {
        questionId,
        questionnaireId,
        destinationQuestionId,
        content,
        description,
        priority,
        destinationType,
        destinationUrl
    })
}

export async function getAnswer(
    request: any,
    answerId: string,
    bearerToken?: string,
) {
    const response = await request.get(`${BASE_URL}/api/answers/${answerId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

    return {
        response,
        answer: responseBody
    }
}

export async function listAnswers(
    request: any,
    questionId: string,
    bearerToken?: string,
) {
    const response = await request.get(`${BASE_URL}/api/questions/${questionId}/answers`, {
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
    const response = await request.put(`${BASE_URL}/api/answers/${answerId}`, {
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
    answerId: string,
    bearerToken?: string,
) {
    const response = await request.delete(`${BASE_URL}/api/answers/${answerId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await parseBody(response);

    return {
        deleteAnswerResponse: response,
        deleteAnswerBody: responseBody,
    }
}