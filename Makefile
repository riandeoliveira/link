PROJECT_PATH = src/LinkJobber.csproj

add-migration:
	@read -p "Migration name: " migration; \
	dotnet ef migrations add $$migration --project $(PROJECT_PATH)

build:
	@dotnet build

migrate:
	@dotnet ef database update $(PROJECT_PATH)

run:
	@dotnet run --project $(PROJECT_PATH)

up:
	@docker compose up -d
