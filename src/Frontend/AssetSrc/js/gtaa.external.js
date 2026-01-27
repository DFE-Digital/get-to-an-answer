const frame = document.getElementById('gtaaFrame');

frame.addEventListener('load', () => {
    frame.contentWindow.postMessage({ type: 'RESIZE' }, '*');
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
        // Send to parent GA4
        gtag('event', e.data.eventName, e.data.eventParams);
    }
});