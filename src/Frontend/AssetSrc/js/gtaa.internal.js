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
    document.addEventListener('DOMContentLoaded', function () {
        const questionForm = document.getElementById('question-form');
        if (questionForm) {
            questionForm.addEventListener('submit', function (e) {
                // Grab the question content (text only)
                let question = document.getElementById("question-content").innerText;
                let questionReference = document.getElementById("question-content").attributes.getNamedItem("data-reference")?.value;

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
                    questionReference: questionReference,
                    answers: answers
                };

                // Send this event to Google Analytics
                sendGA4EventToParent('page_submit', result);
            });
        }
    }); 

    // Check if the URL contains the query string 'embed=True' (case-insensitive)
    const urlParams = new URLSearchParams(window.location.search);
    const embedParam = urlParams.get('embed');

    // Run this logic ONLY when embed=True is present and back button is used to return to the page
    if (embedParam && embedParam.toLowerCase() === 'true') {

        // Select all form controls inside your question form (including dropdowns)
        const formElements = document.querySelectorAll('#question-form input, #question-form select, #question-form textarea');

        formElements.forEach(element => {
            // Use the element's 'name' or 'id' as a unique storage key
            const storageKey = element.name || element.id;
            if (!storageKey) return; // Skip if no identifier exists

            // 1. RESTORE STATE: Check if there's a saved answer in sessionStorage
            const savedValue = sessionStorage.getItem(storageKey);
            if (savedValue !== null) {
                if (element.tagName === 'SELECT') {
                    // Restore dropdown selected option
                    element.value = savedValue;
                } else if (element.type === 'checkbox') {
                    // Restore checkbox state
                    element.checked = (savedValue === 'true');
                } else if (element.type === 'radio') {
                    // Restore radio button state if it matches
                    if (element.value === savedValue) {
                        element.checked = true;
                    }
                } else {
                    // Restore text inputs and textareas
                    element.value = savedValue;
                }
            }

            // 2. SAVE STATE: Listen for changes and update sessionStorage
            const eventType = (element.tagName === 'SELECT' || element.type === 'checkbox' || element.type === 'radio') ? 'change' : 'input';

            element.addEventListener(eventType, (e) => {
                let valueToSave;

                if (element.tagName === 'SELECT') {
                    // Capture the selected option's value from the dropdown
                    valueToSave = element.value;
                } else if (element.type === 'checkbox') {
                    valueToSave = element.checked ? 'true' : 'false';
                } else if (element.type === 'radio') {
                    if (element.checked) {
                        valueToSave = element.value;
                    } else {
                        return; // Ignore unchecked radios in a group
                    }
                } else {
                    valueToSave = element.value;
                }

                // Store it securely in the session
                sessionStorage.setItem(storageKey, valueToSave);
            });
        });
    }
    
    const h = document.documentElement.offsetHeight;
    parent?.postMessage({ type: 'HEIGHT', h }, '*');
})

