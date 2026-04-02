# dachord Chord Application

## Running tests

### unit tests
`dotnet test backend/tests/UnitTests/UnitTests.csproj`

### integration tests (ensure Docker daemon is running)
`dotnet test backend/tests/Integration/IntegrationTests.csproj`

## Running locally

### configuration
`dotnet user-secrets set "Jwt:Key" "your_jwt_key"`
`dotnet user-secrets set "Spotify:ClientId" "your_actual_client_id"`
`dotnet user-secrets set "Spotify:ClientSecret" "your_actual_client_secret"`

### infrastructure
`docker compose up`

### backend
`cd backend && dotnet run`

### frontend
`cd frontend && npm run dev`
