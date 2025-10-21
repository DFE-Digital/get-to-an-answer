FROM mcr.microsoft.com/playwright:v1.41.1-jammy
WORKDIR /tests
COPY package.json yarn.lock ./
RUN yarn install
COPY . .
ENV API_URL=http://web-api:8080
CMD ["yarn", "playwright", "test", "-c", "playwright.config.ts"]