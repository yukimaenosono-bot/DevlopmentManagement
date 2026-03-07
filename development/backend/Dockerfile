FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Synapse.API/Synapse.API.csproj", "src/Synapse.API/"]
COPY ["src/Synapse.Application/Synapse.Application.csproj", "src/Synapse.Application/"]
COPY ["src/Synapse.Domain/Synapse.Domain.csproj", "src/Synapse.Domain/"]
COPY ["src/Synapse.Infrastructure/Synapse.Infrastructure.csproj", "src/Synapse.Infrastructure/"]
RUN dotnet restore "src/Synapse.API/Synapse.API.csproj"
COPY . .
RUN dotnet build "src/Synapse.API/Synapse.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Synapse.API/Synapse.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Synapse.API.dll"]
