FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY GateSense.sln ./

# Copy project files
COPY GateSense.Domain/GateSense.Domain.csproj ./GateSense.Domain/
COPY GateSense.Application/GateSense.Application.csproj ./GateSense.Application/
COPY GateSense.Infrastructure/GateSense.Infrastructure.csproj ./GateSense.Infrastructure/
COPY GetSense.API/GetSense.API.csproj ./GetSense.API/

# Restore dependencies
RUN dotnet restore GateSense.sln

# Copy all source files
COPY GateSense.Domain/ ./GateSense.Domain/
COPY GateSense.Application/ ./GateSense.Application/
COPY GateSense.Infrastructure/ ./GateSense.Infrastructure/
COPY GetSense.API/ ./GetSense.API/

# Build and publish
WORKDIR /src/GetSense.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GetSense.API.dll"]
