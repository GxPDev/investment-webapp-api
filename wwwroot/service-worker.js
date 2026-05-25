self.addEventListener('install', event => {
    console.log('Service Worker Installed');
});

self.addEventListener('fetch', event => {
    // For now we don't intercept fetch — placeholder for future caching strategies
});
