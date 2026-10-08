FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
# The .NET 8 images listen on port 8080 by default
EXPOSE 8080
ENTRYPOINT ["dotnet", "aps-simple-viewer-dotnet.dll"]
