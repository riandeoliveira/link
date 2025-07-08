PROJECT_PATH = src/LinkJoBot.csproj

add-migration:
	@read -p "Migration name: " migration; \
	dotnet ef migrations add $$migration --project $(PROJECT_PATH)

build:
	@dotnet build

migrate:
	@dotnet ef database update $(PROJECT_PATH)

run:
	@docker compose up -d
	@dotnet run --project $(PROJECT_PATH)
