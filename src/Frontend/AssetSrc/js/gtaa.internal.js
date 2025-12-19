(function(c,l,a,r,i,t,y){
    c[a]=c[a]||function(){(c[a].q=c[a].q||[]).push(arguments)};
    t=l.createElement(r);t.async=1;t.src="https://www.clarity.ms/tag/"+i;
    y=l.getElementsByTagName(r)[0];y.parentNode.insertBefore(t,y);
})(window, document, "clarity", "script", "ueriupdhuw");

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
    
    const h = document.documentElement.offsetHeight;
    parent?.postMessage({ type: 'HEIGHT', h }, '*');
})
