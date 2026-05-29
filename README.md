# dachord

## Local development

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/) (for LocalStack)

### 1. Backend secrets

Copy `backend/src/WebApi/secrets.json.example` to `backend/src/WebApi/secrets.json` and fill in your values:

```json
{
  "Spotify": {
    "ClientId": "...",
    "ClientSecret": "..."
  },
  "Jwt": {
    "Key": "any-string-at-least-32-chars-long"
  },
  "DevApiKey": "any-string"
}
```

`secrets.json` is gitignored. The `AWS` block is not needed locally — `appsettings.Development.json` already points to LocalStack at `http://localhost:4566` and uses table prefix `dachord-local-`.

### 2. Frontend env

Create `frontend-web/.env.local`:

```
VITE_API_BASE=https://localhost:7266
VITE_DEV_KEY=<same value as DevApiKey in secrets.json>
```

`.env.local` is gitignored by Vite.

### 3. Start local infrastructure

```bash
docker compose up
```

This starts LocalStack and creates the `dachord-local-tracks` and `dachord-local-users` DynamoDB tables automatically.

### 4. Run the backend

```bash
cd backend/src/WebApi && dotnet run
```

### 5. Run the frontend

```bash
cd frontend-web && npm install && npm run dev
```

---

## Tests

```bash
# Unit tests
dotnet test backend/tests/UnitTests/UnitTests.csproj

# Integration tests (requires Docker)
dotnet test backend/tests/Integration/IntegrationTests.csproj
```

---

## AWS deployment

### Prerequisites
- [AWS CLI](https://aws.amazon.com/cli/) configured with your account (`aws configure`)
- [SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html)
- Docker (SAM uses it to build the Lambda package)

### 1. Spotify credentials

Register an app at [developer.spotify.com](https://developer.spotify.com) to get a client ID and secret.

### 2. SSM parameters

Store secrets in AWS Systems Manager Parameter Store. Replace values with your own:

```bash
aws ssm put-parameter --name /dachord/dev/Jwt__Key \
  --value "your-jwt-secret-min-32-chars" \
  --type SecureString --region eu-central-1

aws ssm put-parameter --name /dachord/dev/Spotify__ClientId \
  --value "your-spotify-client-id" \
  --type SecureString --region eu-central-1

aws ssm put-parameter --name /dachord/dev/Spotify__ClientSecret \
  --value "your-spotify-client-secret" \
  --type SecureString --region eu-central-1

aws ssm put-parameter --name /dachord/dev/DevApiKey \
  --value "your-dev-api-key" \
  --type SecureString --region eu-central-1
```

### 3. DynamoDB tables

Table prefix is `dachord-dev-` for the deployed dev environment (`dachord-local-` is only used by LocalStack).

```bash
aws dynamodb create-table \
  --table-name dachord-dev-users \
  --attribute-definitions AttributeName=pk,AttributeType=S AttributeName=Email,AttributeType=S \
  --key-schema AttributeName=pk,KeyType=HASH \
  --global-secondary-indexes '[{"IndexName":"EmailIndex","KeySchema":[{"AttributeName":"Email","KeyType":"HASH"}],"Projection":{"ProjectionType":"ALL"}}]' \
  --billing-mode PAY_PER_REQUEST \
  --region eu-central-1

aws dynamodb create-table \
  --table-name dachord-dev-tracks \
  --attribute-definitions AttributeName=pk,AttributeType=S AttributeName=sk,AttributeType=S \
  --key-schema AttributeName=pk,KeyType=HASH AttributeName=sk,KeyType=RANGE \
  --billing-mode PAY_PER_REQUEST \
  --region eu-central-1
```

### 4. Deploy

```bash
cd backend
sam build --config-env dev
sam deploy --config-env dev
```

The first deploy will prompt for confirmation. The API URL is printed in the stack outputs.
