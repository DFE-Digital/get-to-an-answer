export function formatTimestampToLongDate(timestamp: string): string {
    const date = new Date(timestamp);

    if (isNaN(date.getTime())) {
        throw new Error(`Invalid timestamp: ${timestamp}`);
    }

    const formatted = new Intl.DateTimeFormat("en-GB", {
        day: "numeric",
        month: "long",
        year: "numeric",
    }).format(date);

    return formatted;
}

export function normalizeText(text: string): string {
    return text
        .replace(/\s+/g, '')        // remove ALL whitespace
        .replace(/[^\w]/g, '')      // remove all non-alphanumeric characters
        .toLowerCase();             // make it case-insensitive
}