ARG Client_ID="722815e8-ef2b-4564-9179-f0ba95afd86e"
ARG Tenant_ID="2b2a6365-d23a-4e3d-a0be-a44e8f5319b9"
ARG Secret_ID="lNR8Q~A2VdolzCe2dFdobHhc2hG4zotSStuFuaOf" #To be sent in as Secrets

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Pour le managed identity pour dev seulement.
ENV AZURE_CLIENT_ID=$Client_ID
ENV AZURE_TENANT_ID=$Tenant_ID
ENV AZURE_CLIENT_SECRET=$Secret_ID

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["API/API.csproj", "API/"]
COPY ["MVC", "MVC/"]

RUN dotnet restore "./API/API.csproj"
COPY ./API ./API
WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]