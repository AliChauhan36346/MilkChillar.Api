# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["MilkChillar.Api.sln", "./"]

# Copy project files
COPY ["MilkChillar.Api/MilkChillar.Api.csproj", "MilkChillar.Api/"]
COPY ["MilkChillar.Application/MilkChillar.Application.csproj", "MilkChillar.Application/"]
COPY ["MilkChillar.Domain/MilkChillar.Domain.csproj", "MilkChillar.Domain/"]
COPY ["MilkChillar.Infrastructure/MilkChillar.Infrastructure.csproj", "MilkChillar.Infrastructure/"]
COPY ["MilkChillar.Persistence/MilkChillar.Persistence.csproj", "MilkChillar.Persistence/"]

# Restore dependencies
RUN dotnet restore "MilkChillar.Api.sln"

# Copy the rest of the source code
COPY . .

# Build the app
WORKDIR "/src/MilkChillar.Api"
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MilkChillar.Api.dll"]
