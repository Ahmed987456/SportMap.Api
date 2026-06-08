FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SportMap.API/SportMap.API.csproj", "SportMap.API/"]
COPY ["SportMap.Application/SportMap.Application.csproj", "SportMap.Application/"]
COPY ["SportMap.Domain/SportMap.Domain.csproj", "SportMap.Domain/"]
COPY ["SportMap.Infrastructure/SportMap.Infrastructure.csproj", "SportMap.Infrastructure/"]

RUN dotnet restore "SportMap.API/SportMap.API.csproj"

COPY . .

WORKDIR "/src/SportMap.API"
RUN dotnet build "SportMap.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SportMap.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SportMap.API.dll"]