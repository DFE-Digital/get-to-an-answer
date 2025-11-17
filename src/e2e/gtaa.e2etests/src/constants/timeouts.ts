export const Timeouts = {
    SHORT: 2000,      // Quick DOM updates, simple elements
    MEDIUM: 6000,     // Standard waits, form validation
    LONG: 10000,      // Complex operations, network requests
    EXTRA_LONG: 15000 // Slow operations, mobile webkit
} as const;
