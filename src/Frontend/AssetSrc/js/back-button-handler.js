function getPathFromUrl(url) {
    const protocolEnd = url.indexOf('://') + 3;
    const pathStart = url.indexOf('/', protocolEnd);
    const fullPath = url.substring(pathStart);

    const parts = fullPath.split('/');
    return '/' + parts.slice(parts.length - 2).join('/');
}

// Handle Back button click for both iframe and non-iframe contexts
document.addEventListener('DOMContentLoaded', function() {
    const backButton = document.getElementById('back-button');
    if (!backButton) return;
   
    const apiBaseUrl = backButton.getAttribute('data-api-baseurl') || '';   
    const path = getPathFromUrl(window.location.href);

    backButton.addEventListener('click', function(e) {
        e.preventDefault();

        const form = document.getElementById('question-form');
        if (!form) return; 
        form.action = apiBaseUrl + path + '&handler=Back';        
        form.submit();
    });
});
