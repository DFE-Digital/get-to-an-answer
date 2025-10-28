import {QuestionnaireBuilder} from '../builders/QuestionnaireBuilder';
import {ClaimTypes, JwtHelper, SimpleDate} from '../helpers/JwtHelper';
import {APIRequestContext, APIResponse} from "@playwright/test";

//to parse response-body correctly - json body can be json, text or empty string
async function safeParseBody(response: APIResponse) {
    const ct = (response.headers()['content-type'] || '').toLowerCase();
    const raw = await response.text();
    if (!raw) return null;
    if (ct.includes('application/json')) {
        try {
            return JSON.parse(raw);
        } catch {
            return null;
        }
    }
    return raw;
}

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

    const response = await request.post('/api/questionnaires', {
        data: payload,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

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
    const response = await request.get(`/api/questionnaires/${questionnaireId}`, {
        headers: {
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

    return {
        questionnaireGetResponse: response,
        questionnaireGetBody: responseBody
    }
}

export async function listQuestionnaires(
    request: APIRequestContext,
    bearerToken?: string
) {
    const response = await request.get('/api/questionnaires', {
        headers: {
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });
    
    const responseBody = await safeParseBody(response);

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
    const response = await request.put(`/api/questionnaires/${questionnaireId}`, {
        data,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

    return {
        updatedQuestionnairePostResponse: response,
        updatedQuestionnaire: responseBody,
    }
}

export async function updateQuestionnaireStatus(
    request: APIRequestContext,
    questionnaireId: number | string,
    data: any,
    bearerToken?: string
) {
    const response = await request.put(`/api/questionnaires/${questionnaireId}/status`, {
        data,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });
    if (!response.ok()) {
        throw new Error(`❌ Failed to update questionnaire status: ${response.status()}`);
    }
    return await response.json();
}

export async function cloneQuestionnaire(
    request: APIRequestContext,
    questionnaireId: number | string,
    data: any,
    bearerToken?: string
) {
    const response = await request.post(`/api/questionnaires/${questionnaireId}/clones`, {
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
    const response = await request.delete(`/api/questionnaires/${questionnaireId}`, {
        headers: {
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

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
    const response = await request.get(`/api/questionnaires/${questionnaireId}/versions`, {
        headers: {'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`}
    });
    if (!response.ok()) {
        throw new Error(`❌ Failed to list questionnaire versions: ${response.status()}`);
    }
    return await response.json();
}

export async function getQuestionnaireVersion(
    request: APIRequestContext,
    questionnaireId: number | string,
    versionNumber: number,
    bearerToken?: string
) {
    const response = await request.get(`/api/questionnaires/${questionnaireId}/versions/${versionNumber}`, {
        headers: {'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`}
    });
    if (!response.ok()) {
        throw new Error(`❌ Failed to get questionnaire version: ${response.status()}`);
    }
    return await response.json();
}

export async function getLatestQuestionnaireVersion(
    request: APIRequestContext,
    questionnaireId: number | string,
    bearerToken?: string
) {
    const response = await request.get(`/api/questionnaires/${questionnaireId}/versions/current`, {
        headers: {'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`}
    });
    if (!response.ok()) {
        throw new Error(`❌ Failed to get latest questionnaire version: ${response.status()}`);
    }
    return await response.json();
}

// Runner

export async function getInitialQuestion(
    request: APIRequestContext,
    questionnaireId: number | string,
    bearerToken?: string
) {
    const response = await request.get(`/api/questionnaires/${questionnaireId}/initial`, {
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
    const response = await request.post(`/api/questionnaires/${questionnaireId}/next`, {
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
    const response = await request.put(`/api/questionnaires/${questionnaireId}/contributors`, {
        headers: {
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });
    if (!response.ok()) {
        throw new Error(`❌ Failed to add self to contributors: ${response.status()}`);
    }
    return await response.json();
}