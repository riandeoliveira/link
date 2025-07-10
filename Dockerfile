FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

COPY . ./

RUN dotnet publish -c Release -o out

COPY .env ./out/.env

RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="${PATH}:/root/.dotnet/tools"

RUN playwright install --with-deps

WORKDIR /app/out

CMD ["dotnet", "ef", "database", "update"]

ENTRYPOINT ["dotnet", "LinkJobber.dll"]
