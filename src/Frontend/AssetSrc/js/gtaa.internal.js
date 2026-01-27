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
    
    const h = document.documentElement.offsetHeight;
    parent?.postMessage({ type: 'HEIGHT', h }, '*');
})
