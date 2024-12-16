FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Argus.sln", "."]
COPY ["src/Argus.Api/Argus.Api.csproj", "src/Argus.Api/"]
COPY ["src/Argus.Abstractions/Argus.Abstractions.csproj", "src/Argus.Abstractions/"]
COPY ["src/Argus.Grains/Argus.Grains.csproj", "src/Argus.Grains/"]
COPY ["src/Argus.Infrastructure/Argus.Infrastructure.csproj", "src/Argus.Infrastructure/"]
COPY ["src/Core/Core.csproj", "src/Core/"]

RUN dotnet restore
COPY . .

WORKDIR "/src/src/Argus.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Argus.Api.dll"]