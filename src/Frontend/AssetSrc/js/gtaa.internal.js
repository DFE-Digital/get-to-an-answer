(function(c,l,a,r,i,t,y){
    c[a]=c[a]||function(){(c[a].q=c[a].q||[]).push(arguments)};
    t=l.createElement(r);t.async=1;t.src="https://www.clarity.ms/tag/"+i;
    y=l.getElementsByTagName(r)[0];y.parentNode.insertBefore(t,y);
})(window, document, "clarity", "script", "ueriupdhuw");

// Define dataLayer and the gtag function.
window.dataLayer = window.dataLayer || [];
function gtag(){dataLayer.push(arguments);}

// Set default consent to 'denied' as a placeholder
// Determine actual values based on your own requirements
gtag('consent', 'default', {
    'ad_storage': 'denied',
    'ad_user_data': 'denied',
    'ad_personalization': 'denied',
    'analytics_storage': 'denied',
});

<!-- Google Tag Manager -->
(function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({'gtm.start': new Date().getTime(),event:'gtm.js'});
    var f=d.getElementsByTagName(s)[0],
    j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';j.async=true;
    j.src='https://www.googletagmanager.com/gtm.js?id='+i+dl;
    f.parentNode.insertBefore(j,f);
})(window,document,'script','dataLayer','@scriptConfig.GTM');

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
