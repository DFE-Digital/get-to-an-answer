// Handle Back button click for both iframe and non-iframe contexts
document.addEventListener('DOMContentLoaded', function() {
    const backButton = document.getElementById('back-button');
    if (!backButton) return;

    backButton.addEventListener('click', function(e) {
        e.preventDefault();

        const form = document.getElementById('question-form');
        if (!form) return;

        // Parse the current form action to preserve embed parameter
        const currentAction = form.action || '';
        const url = new URL(currentAction, window.location.href);
        const embed = url.searchParams.get('embed') || 'false';

        // Build the back action URL for admin preview
        // Extract base URL without query string
        const baseAction = currentAction.split('?')[0];
        form.action = baseAction + '?handler=Back&embed=' + embed;

        // Submit the form
        form.submit();
    });
});
