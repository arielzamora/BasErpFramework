# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BasErpFramework.Services.WebApi/BasErpFramework.Services.WebApi.csproj", "BasErpFramework.Services.WebApi/"]
COPY ["BasErpFramework.Application.Dto/BasErpFramework.Application.Dto.csproj", "BasErpFramework.Application.Dto/"]
COPY ["BasErpFramework.Application.Interface/BasErpFramework.Application.Interface.csproj", "BasErpFramework.Application.Interface/"]
COPY ["BasErpFramework.Transversal.Common/BasErpFramework.Transversal.Common.csproj", "BasErpFramework.Transversal.Common/"]
COPY ["BasErpFramework.Domain.Entity/BasErpFramework.Domain.Entity.csproj", "BasErpFramework.Domain.Entity/"]
COPY ["BasErpFramework.Application.Main/BasErpFramework.Application.Main.csproj", "BasErpFramework.Application.Main/"]
COPY ["BasErpFramework.Domain.Interface/BasErpFramework.Domain.Interface.csproj", "BasErpFramework.Domain.Interface/"]
COPY ["BasErpFramework.Transversal.Logging/BasErpFramework.Transversal.Logging.csproj", "BasErpFramework.Transversal.Logging/"]
COPY ["BasErpFramework.Domain.Core/BasErpFramework.Domain.Core.csproj", "BasErpFramework.Domain.Core/"]
COPY ["BasErpFramework.Infrastructure.Interface/BasErpFramework.Infrastructure.Interface.csproj", "BasErpFramework.Infrastructure.Interface/"]
COPY ["BasErpFramework.Infrastructure.Repository/BasErpFramework.Infrastructure.Repository.csproj", "BasErpFramework.Infrastructure.Repository/"]
COPY ["BasErpFramework.Infrastructure.Data/BasErpFramework.Infrastructure.Data.csproj", "BasErpFramework.Infrastructure.Data/"]
RUN dotnet restore "./BasErpFramework.Services.WebApi/BasErpFramework.Services.WebApi.csproj"
COPY . .
WORKDIR "/src/BasErpFramework.Services.WebApi"
RUN dotnet build "./BasErpFramework.Services.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BasErpFramework.Services.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BasErpFramework.Services.WebApi.dll"]