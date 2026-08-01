# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# curl is needed for Docker Compose's HEALTHCHECK to probe /health. libgssapi-krb5-2 silences
# a benign "Cannot load library libgssapi_krb5.so.2" warning Npgsql logs on every connection
# when probing for optional Kerberos/GSSAPI auth support, which this project doesn't use. Both
# must run as root, before USER switches to the unprivileged app account.
RUN apt-get update && apt-get install -y --no-install-recommends curl libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ui/Retrosharp.UI.Api/Retrosharp.UI.Api.csproj", "ui/Retrosharp.UI.Api/"]
RUN dotnet restore "./ui/Retrosharp.UI.Api/Retrosharp.UI.Api.csproj"
COPY . .
WORKDIR "/src/ui/Retrosharp.UI.Api"
RUN dotnet build "./Retrosharp.UI.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Retrosharp.UI.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Retrosharp.UI.Api.dll"]