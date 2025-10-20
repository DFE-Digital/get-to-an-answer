import {QuestionBuilder} from '../builders/QuestionBuilder';
import {QuestionType} from '../constants/test-data-constants';

export async function createQuestion(
    request: any,
    questionnaireId: string,
    content?: string,
    description?: string,
    type?: QuestionType,
    questionPrefix?: string
) {

    const payload = new QuestionBuilder(questionnaireId)
        .withContent(content)
        .withContentPrefix(questionPrefix)
        .withDescription(description)
        .withType(type)
        .build();
    
    const response = await request.post('/questions', {data: payload});

    if (!response.ok()) {
        throw new Error(`❌ Failed to create question: ${response.status()}`);
    }

    const questionPostResponse = await response.json();
    return {questionPostResponse, payload}
}

export async function getQuestion(
    request: any,
    questionId: number
) {
    const response = await request.get(`/questions/${questionId}`);

    if (!response.ok()) {
        throw new Error(`❌ Failed to fetch required question: ${response.status()}`);
    }

    return await response.json();
}