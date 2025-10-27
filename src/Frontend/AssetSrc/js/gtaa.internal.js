window.addEventListener('message', function(e) {
    if (e.data.type === 'RESIZE') {
        const h = document.documentElement.scrollHeight;
        parent?.postMessage({ type: 'HEIGHT', h }, '*');
    }
})

window.addEventListener('load', function (e) {
    const h = document.documentElement.scrollHeight;
    parent?.postMessage({ type: 'HEIGHT', h }, '*');
})
