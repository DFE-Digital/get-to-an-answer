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

interface CreateContentRequest {
    questionnaireId: string;
    title?: string;
    content?: string;
    referenceName?: string;
}

export async function createContent(
    request: any,
    contentRequest: CreateContentRequest,
    bearerToken?: string,
) {

    const response = await request.post(`${BASE_URL}/api/contents`, {
        data: {
            questionnaireId: contentRequest.questionnaireId,
            title: contentRequest.title,
            content: contentRequest.content,
            referenceName: contentRequest.referenceName,
        },
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);
    
    return {
        response,
        content: responseBody
    }
}

export async function getContent(
    request: any,
    contentId: string,
    bearerToken?: string,
) {
    const response = await request.get(`${BASE_URL}/api/contents/${contentId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

    return {
        response,
        content: responseBody
    }
}

export async function listContents(
    request: any,
    questionnaireId: string,
    bearerToken?: string,
) {
    const response = await request.get(`${BASE_URL}/api/questionnaires/${questionnaireId}/contents`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

    return {
        contentsGetResponse: response,
        contents: responseBody
    }
}

export async function updateContent(
    request: any,
    contentId: string,
    data: any,
    bearerToken?: string,
) {
    const response = await request.put(`${BASE_URL}/api/contents/${contentId}`, {
        data,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await safeParseBody(response);

    return {
        updatedContentPostResponse: response,
        updatedContent: responseBody
    }
}

export async function deleteContent(
    request: any,
    contentId: string,
    bearerToken?: string,
) {
    const response = await request.delete(`${BASE_URL}/api/contents/${contentId}`, {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearerToken ?? JwtHelper.ValidToken}`
        }
    });

    const responseBody = await parseBody(response);

    return {
        deleteContentResponse: response,
        deleteContentBody: responseBody,
    }
}