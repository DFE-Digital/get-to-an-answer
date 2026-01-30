const frame = document.getElementById('gtaaFrame');

frame.addEventListener('load', () => {
    frame.contentWindow.postMessage({ type: 'RESIZE' }, '*');

    // Get the GA4 Measure ID
    const ga4Ids = Object.keys(window.google_tag_manager || {})
        .filter(key => key.startsWith('G-'))
    
    if (ga4Ids.length && typeof gtag === 'function') {
        
        // Set page view as manual, 
        // so the questionnaire can send several page view requests
        gtag('config', ga4Ids[0], { 
            'send_page_view': false,
            'update': true
        });
    }
})
window.addEventListener('message', (e) => {
    const frame = document.getElementById('gtaaFrame');
    
    if (!frame) 
        return;
    
    if (e.data?.type === 'HEIGHT') {
        frame.style.height = `${e.data.h}px`;
    } else if (e.data?.type === 'DEST-REDIRECT') {
        window.location.href = e.data.externalLinkDest;
    } else if (e.data?.type === 'GA4_GTAA_IFRAME_EVENT' && typeof gtag === 'function') {
        // Send iframe page_view event to the GA4 dataLayer
        gtag('event', e.data.eventName, e.data.eventParams);
    }
});