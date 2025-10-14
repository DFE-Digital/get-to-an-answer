module.exports = function (migration) {
    // Questionnaire
    const questionnaire = migration.createContentType('questionnaire')
        .name('Questionnaire')
        .displayField('title')
        .description('Questionnaire root entity');

    questionnaire.createField('title').name('Title').type('Symbol').required(true);
    questionnaire.createField('slug').name('Slug').type('Symbol').required(false);
    questionnaire.createField('description').name('Description').type('Text');
    questionnaire.createField('status').name('Status').type('Symbol').validations([
        { in: ['Draft', 'Published', 'Archived', 'Deleted'] }
    ]);
    questionnaire.createField('contributors').name('Contributors').type('Array').items({ type: 'Symbol' });
    
    // Question
    const question = migration.createContentType('question')
        .name('Question')
        .displayField('content');

    question.createField('questionnaire').name('Questionnaire').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['questionnaire'] }]).required(true);
    question.createField('content').name('Content').type('Text').required(true);
    question.createField('description').name('Description').type('Text');
    question.createField('type').name('Type').type('Symbol').validations([
        { in: ['SingleSelect', 'MultiSelect', 'DropdownSelect']}
    ]);
    question.createField('order').name('Order').type('Integer').required(true);
    question.createField('status').name('Status').type('Symbol').validations([
        { in: ['Draft', 'Published', 'Archived', 'Deleted'] }
    ]);
    
    // Answer
    const answer = migration.createContentType('answer')
        .name('Answer')
        .displayField('content');

    answer.createField('questionnaire').name('Questionnaire').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['questionnaire'] }]).required(true);
    answer.createField('question').name('Question').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['question'] }]).required(true);
    answer.createField('content').name('Content').type('Text').required(true);
    answer.createField('description').name('Description').type('Text');
    answer.createField('score').name('Score').type('Number');
    answer.createField('destinationType').name('Destination Type').type('Symbol').validations([
        { in: ['Question', 'ExternalLink', 'InternalPage'] }
    ]);
    answer.createField('destination').name('Destination (URL)').type('Symbol');
    answer.createField('destinationQuestion').name('Destination Question').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['question'] }]);
    answer.createField('destinationContent').name('Destination Content').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['content'] }]);
    
    // Content (internal pages)
    const content = migration.createContentType('content')
        .name('Content')
        .displayField('title');

    content.createField('questionnaire').name('Questionnaire').type('Link').linkType('Entry')
        .validations([{ linkContentType: ['questionnaire'] }]).required(true);
    content.createField('title').name('Title').type('Symbol').required(true);
    content.createField('content').name('Content Body').type('Text'); // or 'Text' if you prefer
    
     // Optional: indexes/uniqueness hints (apply manually if needed)
    // - questionnaire.slug unique per space
    // - question order unique within questionnaire (enforce in app)
    // - answers per question (enforce in app)
};