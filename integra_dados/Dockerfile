#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER $APP_UID
#WORKDIR /app
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
##WORKDIR /src
#COPY ["/integra_dados/integra_dados.csproj", "integra_dados/"]
#RUN dotnet restore "integra_dados/integra_dados.csproj"
#
#COPY . .
#WORKDIR "/src/integra_dados"
#RUN dotnet build "./integra_dados.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./integra_dados.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/build .
#ENTRYPOINT ["dotnet", "integra_dados.dll"]

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