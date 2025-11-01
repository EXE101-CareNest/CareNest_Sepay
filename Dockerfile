# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Configure ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CareNest_SePay/CareNest_SePay.csproj", "CareNest_SePay/"]
COPY ["CareNest_SePay.Application/CareNest_SePay.Application.csproj", "CareNest_SePay.Application/"]
COPY ["CareNest_SePay.Domain/CareNest_SePay.Domain.csproj", "CareNest_SePay.Domain/"]
COPY ["CareNest_SePay.Infrastructure/CareNest_SePay.Infrastructure.csproj", "CareNest_SePay.Infrastructure/"]
RUN dotnet restore "./CareNest_SePay/CareNest_SePay.csproj"
COPY . .
WORKDIR "/src/CareNest_SePay"
RUN dotnet build "./CareNest_SePay.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CareNest_SePay.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Add health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CareNest_SePay.dll"]