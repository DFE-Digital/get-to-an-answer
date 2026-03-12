// Send GA4 event data to parent
function sendGA4EventToParent(eventName, eventParams) {
    parent.postMessage({
        type: 'GA4_GTAA_IFRAME_EVENT',
        eventName: eventName,
        eventParams: eventParams
    }, '*');
}


window.addEventListener('message', function(e) {
    if (e.data.type === 'RESIZE') {
        const h = document.documentElement.offsetHeight;
        parent?.postMessage({ type: 'HEIGHT', h }, '*');
    }
})

window.addEventListener('load', function (e) {
    const externalLinkDest = document.getElementById('external-link-dest');
    if (externalLinkDest) {
        parent?.postMessage({ type: 'DEST-REDIRECT', externalLinkDest: externalLinkDest.value }, '*');
    }
    
    // https://developers.google.com/analytics/devguides/collection/ga4/views?client_type=gtag
    sendGA4EventToParent('page_view', {
        page_title: document.title,
        page_location: window.location.href,
        page_path: window.location.pathname,
        redirect_to: externalLinkDest?.value
    });

    // Add an event listener for when the form is posted
    document.getElementById('question-form').addEventListener('submit', function(e) {
        // Grab the question content (text only)
        let question = document.getElementById("question-content").innerText;

        // Get the labels for selected checkboxes
        let selectedCheckboxes = Array.from(document.querySelectorAll('input[type="checkbox"]:checked')).map(x => x.labels[0].textContent.trim());

        // Get the labels for the selected radio
        let selectedRadios = Array.from(document.querySelectorAll('input[type="radio"]:checked')).map(x => x.labels[0].textContent.trim());

        // Save these to an answers array
        let answers = [...selectedCheckboxes, ...selectedRadios];

        // Add the dropdown value to the array, if we've got one
        let selectedDropdown = document.querySelector('select[name="NextStateRequest.SelectedAnswerIds"]');
        if (selectedDropdown && selectedDropdown.selectedIndex > 0)
            answers.push(selectedDropdown.options[selectedDropdown.selectedIndex].text.trim());

        let result = {
            question: question,
            answers: answers
        };

        // Send this event to Google Analytics
        sendGA4EventToParent('page_submit', result);

    })
    
    const h = document.documentElement.offsetHeight;
    parent?.postMessage({ type: 'HEIGHT', h }, '*');
})

