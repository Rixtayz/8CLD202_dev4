# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl pour le HealthCheck
USER root
RUN apt-get update && apt-get install -y
RUN apt-get install curl -y

# Install ping tool for diagnostic
USER root
RUN apt-get update && apt-get install -y iputils-ping

# Pour le managed identity en Dev seulement.
ENV AZURE_CLIENT_ID="71cfba22-690f-49c1-96ad-1956705fcd85"
ENV AZURE_TENANT_ID="2b2a6365-d23a-4e3d-a0be-a44e8f5319b9"
ENV AZURE_CLIENT_SECRET="3Or8Q~QpOy6Uuwc3E5pwq01J~ioIzjc8DZH34bCc"

# Pour forcer le redirect URL
ENV ASPNETCORE_HTTP_PORTS=http://+:80
ENV ASPNETCORE_HTTPS_PORTS=https://+:443

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MVC/MVC.csproj", "MVC/"]
RUN dotnet restore "./MVC/MVC.csproj"
COPY . .
WORKDIR "/src/MVC"
RUN dotnet build "MVC.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MVC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MVC.dll"]
