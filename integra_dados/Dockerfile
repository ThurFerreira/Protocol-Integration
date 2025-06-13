# Imagem para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia a solução e restaura pacotes
COPY . .
RUN dotnet restore "integra_dados.csproj"

# Publica a aplicação
RUN dotnet publish "integra_dados.csproj" -c Release -o publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 7001

ENTRYPOINT [ "dotnet", "integra_dados.dll" ]