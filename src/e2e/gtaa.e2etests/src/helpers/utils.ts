import {Locator} from "@playwright/test";

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

export function convertColorToHex(color: string): string {
    if (!color) return '';

    // Handle RGB/RGBA format
    if (color.startsWith('rgb')) {
        const values = color.match(/\d+/g);
        if (!values || values.length < 3) return color;

        const r = parseInt(values[0]);
        const g = parseInt(values[1]);
        const b = parseInt(values[2]);

        return `#${((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1)}`;
    }

    // Handle hex format
    if (color.startsWith('#')) {
        return color.length === 4
            ? `#${color[1]}${color[1]}${color[2]}${color[2]}${color[3]}${color[3]}`
            : color;
    }

    return color;
}

export async function getElementInfo(element: Locator) {
    return await element.
    evaluate((el) => ({
        tagName: el.tagName.toLowerCase(),
        id: el.id || '',
        classes: el.className || '',
        selector: el.tagName.toLowerCase() +
            (el.id ? `#${el.id}` : '') +
            (el.className ? `.${el.className.split(' ').join('.')}` : '')
    }));
}