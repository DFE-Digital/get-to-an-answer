import {QuestionBuilder} from '../builders/QuestionBuilder';
import {QuestionType} from '../constants/test-data-constants';
import { JwtHelper } from "../helpers/JwtHelper";
import {parseBody} from "../helpers/ParseBody";
import { EnvConfig } from '../config/environment-config';

const BASE_URL = EnvConfig.API_URL;

export async function createQuestion(
    request: any,
    questionnaireId: string,
    bearerToken?: string,
    content?: string,
    type?: QuestionType,
    description?: string,
) {

    const payload = new QuestionBuilder(questionnaireId)
        .withContent(content)
        .withDescription(description)
        .withType(type)
        .build();
    
    const response = await request.post(`${BASE_URL}/api/questions`, {
        data: payload,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });
    
    const responseBody = await parseBody(response);

    return {
        questionPostResponse: response,
        question: responseBody,
        payload
    }
}

export async function getQuestion(
    request: any,
    questionId: string,
    bearerToken?: string
) {
    const response = await request.get(`${BASE_URL}/api/questions/${questionId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await parseBody(response);

    return {
        questionGetResponse: response,
        questionGetBody: responseBody
    }
}

export async function listQuestions(
    request: any,
    questionnaireId: string,
    bearerToken?: string
) {
    const response = await request.get(`${BASE_URL}/api/questionnaires/${questionnaireId}/questions`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await parseBody(response);

    return {
        questionGetResponse: response,
        questionGetBody: responseBody
    }
}

export async function updateQuestion(
    request: any,
    questionId: string,
    data: any,
    bearerToken?: string
) {
    const response = await request.put(`${BASE_URL}/api/questions/${questionId}`, {
        data,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });
    
    const responseBody = await parseBody(response);

    return {
        updatedQuestionPostResponse: response,
        UpdatedQuestion: responseBody,
    }
}

export async function deleteQuestion(
    request: any,
    questionId: string,
    bearerToken?: string
) {
    const response = await request.delete(`${BASE_URL}/api/questions/${questionId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await parseBody(response);

    return {
        deleteQuestionResponse: response,
        deleteQuestionBody: responseBody,
    }
}

export async function moveQuestionDownOne(
    request: any,
    questionnaireId: string,
    questionId: string,
    bearerToken?: string,
) {
    const response = await request.patch(`${BASE_URL}/api/questionnaires/${questionnaireId}/questions/${questionId}/move-down`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to move question down: ${response.status()}`);
    }

    return response.status();
}

export async function moveQuestionUpOne(
    request: any,
    questionnaireId: string,
    questionId: string,
    bearerToken?: string
) {
    const response = await request.patch(`${BASE_URL}/api/questionnaires/${questionnaireId}/questions/${questionId}/move-up`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to move question up: ${response.status()}`);
    }

    return response.status();
}
