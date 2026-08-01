# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
# aspnet, not the plain runtime image: this process now hosts a minimal Kestrel /health
# listener (see Program.cs) alongside the NServiceBus endpoint, which needs the ASP.NET Core
# shared framework at runtime, not just at compile time -- found live when the container
# failed to start ("No frameworks were found... Microsoft.AspNetCore.App").
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# curl is needed for Docker Compose's HEALTHCHECK to probe the sidecar /health listener
# (see Program.cs). libgssapi-krb5-2 silences a benign "Cannot load library
# libgssapi_krb5.so.2" warning Npgsql logs on every connection when probing for optional
# Kerberos/GSSAPI auth support, which this project doesn't use. Both must run as root, before
# USER switches to the unprivileged app account.
RUN apt-get update && apt-get install -y --no-install-recommends curl libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["engine/Retrosharp.Engine.Console/Retrosharp.Engine.Console.csproj", "engine/Retrosharp.Engine.Console/"]
RUN dotnet restore "./engine/Retrosharp.Engine.Console/Retrosharp.Engine.Console.csproj"
COPY . .
WORKDIR "/src/engine/Retrosharp.Engine.Console"
RUN dotnet build "./Retrosharp.Engine.Console.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Retrosharp.Engine.Console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Retrosharp.Engine.Console.dll"]