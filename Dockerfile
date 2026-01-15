# Steg 1: Build-miljö
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Kopiera lösningsfilen och projektfiler för att återställa paket
COPY *.sln .
COPY CI_CD_Group_5/*.csproj ./CI_CD_Group_5/
COPY CI_CD_Group_5.Tests/*.csproj ./CI_CD_Group_5.Tests/
RUN dotnet restore

# Kopiera resten av koden och bygg applikationen
COPY . .
WORKDIR /app/CI_CD_Group_5
RUN dotnet publish -c Release -o /app/publish

# Steg 2: Runtime-miljö
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Starta applikationen
ENTRYPOINT ["dotnet", "CI_CD_Group_5.dll"]