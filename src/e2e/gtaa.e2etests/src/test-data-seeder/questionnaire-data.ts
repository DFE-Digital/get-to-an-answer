import {QuestionnaireBuilder} from '../builders/QuestionnaireBuilder';
import {ClaimTypes, JwtHelper, SimpleDate} from '../helpers/JwtHelper';
import {APIRequestContext, APIResponse} from "@playwright/test";
import {parseBody} from "../helpers/ParseBody";
import { EnvConfig } from '../config/environment-config';

const BASE_URL = EnvConfig.API_URL;

export async function createQuestionnaire(
    request: APIRequestContext,
    bearerToken?: string,
    title?: string,
    description?: string,
    slug?: string
) {
    const payload = new QuestionnaireBuilder()
        .withTitle(title)
        .withDescription(description)
        .withSlug(slug)
        .build();
    
    const response = await request.post(`${BASE_URL}/api/questionnaires`, {
        data: payload,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await parseBody(response);

    return {
        questionnairePostResponse: response,
        questionnaire: responseBody,
        payload
    }
}

export async function getQuestionnaire(
    request: APIRequestContext,
    questionnaireId: number,
    bearerToken?: string,
) {
    const response = await request.get(`${BASE_URL}/api/questionnaires/${questionnaireId}`, {
        headers: {
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await parseBody(response);

    return {
        questionnaireGetResponse: response,
        questionnaireGetBody: responseBody
    }
}

export async function listQuestionnaires(
    request: APIRequestContext,
    bearerToken?: string
) {
    const response = await request.get(`${BASE_URL}/api/questionnaires`, {
        headers: {
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await parseBody(response);

    return {
        questionnaireGetResponse: response,
        questionnaireGetBody: responseBody
    }
}

export async function updateQuestionnaire(
    request: APIRequestContext,
    questionnaireId: number | string,
    data: any,
    bearerToken?: string
) {
    const response = await request.put(`${BASE_URL}/api/questionnaires/${questionnaireId}`, {
        data,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await parseBody(response);

    return {
        updatedQuestionnairePostResponse: response,
        updatedQuestionnaire: responseBody,
    }
}

export async function publishQuestionnaire(
    request: APIRequestContext,
    questionnaireId: number | string,
    bearerToken?: string
) {
    const response = await request.patch(`${BASE_URL}/api/questionnaires/${questionnaireId}?action=Publish`, {
        data: {},
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });
    
    return { response };
}

export async function unpublishQuestionnaire(
    request: APIRequestContext,
    questionnaireId: number | string,
    bearerToken?: string
) {
    const response = await request.put(`${BASE_URL}/api/questionnaires/${questionnaireId}/unpublish`, {
        data: {},
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    return { response };
}

export async function cloneQuestionnaire(
    request: APIRequestContext,
    questionnaireId: number | string,
    data: any,
    bearerToken?: string
) {
    const response = await request.post(`${BASE_URL}/api/questionnaires/${questionnaireId}/clones`, {
        data,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });
    if (!response.ok()) {
        throw new Error(`❌ Failed to clone questionnaire: ${response.status()}`);
    }
    return await response.json();
}

export async function deleteQuestionnaire(
    request: APIRequestContext,
    questionnaireId: string,
    bearerToken?: string
) {
    const response = await request.delete(`${BASE_URL}/api/questionnaires/${questionnaireId}`, {
        headers: {
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await parseBody(response);

    return {
        deleteQuestionnaireResponse: response,
        deleteQuestionnaireBody: responseBody,
    }
}

// Versions

export async function listQuestionnaireVersions(
    request: APIRequestContext,
    questionnaireId: number | string,
    bearerToken?: string
) {
    const response = await request.get(`${BASE_URL}/api/questionnaires/${questionnaireId}/versions`, {
        headers: {'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`}
    });
    
    let versions = []
    
    if (response.ok()) {
        versions = await response.json();
    }
    return { response, versions};
}

// Runner

export async function getInitialQuestion(
    request: APIRequestContext,
    questionnaireId: number | string,
    bearerToken?: string
) {
    const response = await request.get(`${BASE_URL}/api/questionnaires/${questionnaireId}/initial`, {
        headers: {'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`}
    });
    if (!response.ok()) {
        throw new Error(`❌ Failed to get initial question: ${response.status()}`);
    }
    return await response.json();
}

export async function getNextState(
    request: APIRequestContext,
    questionnaireId: number | string,
    data: any,
    bearerToken?: string
) {
    const response = await request.post(`${BASE_URL}/api/questionnaires/${questionnaireId}/next`, {
        data,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });
    if (!response.ok()) {
        throw new Error(`❌ Failed to get next state: ${response.status()}`);
    }
    return await response.json();
}

// Contributors

export async function addSelfToQuestionnaireContributors(
    request: APIRequestContext,
    questionnaireId: number | string,
    bearerToken?: string
) {
    const response = await request.put(`${BASE_URL}/api/questionnaires/${questionnaireId}/contributors/self`, {
        headers: {
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });
    if (!response.ok()) {
        throw new Error(`❌ Failed to add self to contributors: ${response.status()}`);
    }
    return await response.json();
}