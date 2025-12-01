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