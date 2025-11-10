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
    }
});