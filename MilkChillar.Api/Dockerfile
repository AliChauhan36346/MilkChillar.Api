# Use official .NET 8 SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["MilkChillar.Api/MilkChillar.Api.csproj", "MilkChillar.Api/"]
COPY ["MilkChillar.Application/MilkChillar.Application.csproj", "MilkChillar.Application/"]
COPY ["MilkChillar.Domain/MilkChillar.Domain.csproj", "MilkChillar.Domain/"]
COPY ["MilkChillar.Infrastructure/MilkChillar.Infrastructure.csproj", "MilkChillar.Infrastructure/"]
COPY ["MilkChillar.Persistence/MilkChillar.Persistence.csproj", "MilkChillar.Persistence/"]
RUN dotnet restore "MilkChillar.Api/MilkChillar.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MilkChillar.Api"
RUN dotnet publish -c Release -o /app/publish

# Use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MilkChillar.Api.dll"]
