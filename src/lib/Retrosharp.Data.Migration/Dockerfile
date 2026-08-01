# Build context for this Dockerfile is the REPO ROOT (not src/, unlike the other two
# Dockerfiles) -- Retrosharp.Data.Migration.csproj copies docs/csv/*.csv at build time (see the
# csproj's <None Include="..\..\..\docs\csv\..."> seed-data items), which lives outside src/.
# Building this project also transitively builds Retrosharp.Engine.Console (a ProjectReference
# kept purely for NServiceBus.Persistence.Sql's script-generation side effect -- see the
# csproj), so its full source needs to be present in the context too. See
# spec/phase-1-build-plan.md Step 8.

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
# libgssapi-krb5-2 silences a benign "Cannot load library libgssapi_krb5.so.2" warning Npgsql
# logs on every connection when probing for optional Kerberos/GSSAPI auth support, which this
# project doesn't use (plain username/password auth) -- otherwise harmless, but noisy.
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo
COPY ["src/lib/Retrosharp.Data.Migration/Retrosharp.Data.Migration.csproj", "src/lib/Retrosharp.Data.Migration/"]
COPY ["src/lib/Retrosharp.Data/Retrosharp.Data.csproj", "src/lib/Retrosharp.Data/"]
COPY ["src/lib/Retrosharp.Service/Retrosharp.Service.csproj", "src/lib/Retrosharp.Service/"]
COPY ["src/lib/Retrosharp.Service.Interface/Retrosharp.Service.Interface.csproj", "src/lib/Retrosharp.Service.Interface/"]
COPY ["src/lib/Retrosharp/Retrosharp.csproj", "src/lib/Retrosharp/"]
COPY ["src/engine/Retrosharp.Engine.Console/Retrosharp.Engine.Console.csproj", "src/engine/Retrosharp.Engine.Console/"]
RUN dotnet restore "./src/lib/Retrosharp.Data.Migration/Retrosharp.Data.Migration.csproj"
COPY . .

# Built as its own explicit step, BEFORE Retrosharp.Data.Migration -- the <Content Include>
# glob below that copies the generated NServiceBus.Persistence.Sql scripts is evaluated at
# MSBuild project-evaluation time, not after Engine.Console (its ProjectReference dependency)
# finishes building within the same invocation. Locally this accidentally worked because
# Engine.Console had always already been built moments earlier in a separate command; a clean
# container has no such prior build, so the glob would silently match zero files without this
# explicit ordering. Found live verifying this Dockerfile against real Postgres.
WORKDIR "/repo/src/engine/Retrosharp.Engine.Console"
RUN dotnet build "./Retrosharp.Engine.Console.csproj" -c $BUILD_CONFIGURATION

WORKDIR "/repo/src/lib/Retrosharp.Data.Migration"
RUN dotnet build "./Retrosharp.Data.Migration.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Retrosharp.Data.Migration.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Retrosharp.Data.Migration.dll"]
