# Сборка приложения
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ISUMPK2.Web/*.csproj ISUMPK2.Web/
RUN dotnet restore ISUMPK2.Web/ISUMPK2.Web.csproj

COPY . .
RUN dotnet publish ISUMPK2.Web/ISUMPK2.Web.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "ISUMPK2.Web.dll"]