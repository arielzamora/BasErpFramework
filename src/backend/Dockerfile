FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copiar toda la solución y restaurar
COPY . .
RUN dotnet restore "src/backend/BasErpFramework.slnx"

# Moverse a la carpeta de Web API y compilar
WORKDIR "/src/src/backend/BasErpFramework.Services.WebApi"
RUN dotnet build "BasErpFramework.Services.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasErpFramework.Services.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BasErpFramework.Services.WebApi.dll"]
