FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY HikePlanner.fsproj .

RUN dotnet restore --no-cache

COPY . .

RUN dotnet publish -c Release -o /app/publish --no-restore --self-contained=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HikePlanner.dll"]
