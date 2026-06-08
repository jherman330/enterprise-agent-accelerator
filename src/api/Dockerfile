# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY EnterpriseAgentAccelerator.Api.csproj ./
RUN dotnet restore EnterpriseAgentAccelerator.Api.csproj
COPY . ./
RUN dotnet publish EnterpriseAgentAccelerator.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "EnterpriseAgentAccelerator.Api.dll"]
