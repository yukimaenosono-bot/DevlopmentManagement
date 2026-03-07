FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/ManufacturingSystem.API/ManufacturingSystem.API.csproj", "src/ManufacturingSystem.API/"]
COPY ["src/ManufacturingSystem.Application/ManufacturingSystem.Application.csproj", "src/ManufacturingSystem.Application/"]
COPY ["src/ManufacturingSystem.Domain/ManufacturingSystem.Domain.csproj", "src/ManufacturingSystem.Domain/"]
COPY ["src/ManufacturingSystem.Infrastructure/ManufacturingSystem.Infrastructure.csproj", "src/ManufacturingSystem.Infrastructure/"]
RUN dotnet restore "src/ManufacturingSystem.API/ManufacturingSystem.API.csproj"
COPY . .
RUN dotnet build "src/ManufacturingSystem.API/ManufacturingSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/ManufacturingSystem.API/ManufacturingSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ManufacturingSystem.API.dll"]
