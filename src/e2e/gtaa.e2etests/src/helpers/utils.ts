export function formatTimestampToLongDate(timestamp: string): string {
    const date = new Date(timestamp);

    if (isNaN(date.getTime())) {
        throw new Error(`Invalid timestamp: ${timestamp}`);
    }

    return new Intl.DateTimeFormat("en-GB", {
        day: "2-digit",
        month: "long",
        year: "numeric",
    }).format(date);
}
