import dotenv from 'dotenv';
dotenv.config();

export type EnvType = 'local' | 'test';

export function loadEnvConfig(envName: EnvType) {
    const prefix = envName.toUpperCase();

    return {
        name: envName,
        API_URL: process.env[`${prefix}_API_URL`]!,
        ADMIN_URL: process.env[`${prefix}_ADMIN_URL`]!,
        FE_URL: process.env[`${prefix}_FE_URL`]!,
        //secrets or user test data can also go here if needed
    };
}