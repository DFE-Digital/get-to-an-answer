import {QuestionBuilder} from '../builders/QuestionBuilder';
import {QuestionType} from '../constants/test-data-constants';
import { JwtHelper } from "../helpers/JwtHelper";

export async function createQuestion(
    request: any,
    questionnaireId: string,
    content?: string,
    description?: string,
    type?: QuestionType,
    questionPrefix?: string,
    bearerToken?: string
) {

    const payload = new QuestionBuilder(questionnaireId)
        .withContent(content)
        .withContentPrefix(questionPrefix)
        .withDescription(description)
        .withType(type)
        .build();
    
    const response = await request.post('/api/questions', {
        data: payload,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to create question: ${response.status()}`);
    }

    const questionPostResponse = await response.json();
    return {questionPostResponse, payload}
}

export async function getQuestion(
    request: any,
    questionId: string,
    bearerToken?: string
) {
    const response = await request.get(`/api/questions/${questionId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to fetch required question: ${response.status()}`);
    }

    return await response.json();
}

export async function listQuestions(
    request: any,
    questionnaireId: string,
    bearerToken?: string
) {
    const response = await request.get(`/api/questionnaires/${questionnaireId}/questions`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to list questions: ${response.status()}`);
    }

    return await response.json();
}

export async function updateQuestion(
    request: any,
    questionId: string,
    data: any,
    bearerToken?: string
) {
    const response = await request.put(`/api/questions/${questionId}`, {
        data,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to update question: ${response.status()}`);
    }

    return await response.json();
}

export async function deleteQuestion(
    request: any,
    questionId: string,
    bearerToken?: string
) {
    const response = await request.delete(`/api/questions/${questionId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    if (!response.ok()) {
        throw new Error(`❌ Failed to delete question: ${response.status()}`);
    }

    return response.status();
}

export async function moveQuestionDownOne(
    request: any,
    questionnaireId: string,
    questionId: string,
    bearerToken?: string,
) {
    const response = await request.patch(`/api/questionnaires/${questionnaireId}/questions/${questionId}/move-down`, {
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
    const response = await request.patch(`/api/questionnaires/${questionnaireId}/questions/${questionId}/move-up`, {
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
