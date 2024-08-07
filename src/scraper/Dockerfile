#
# Build
#
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /App

# Restore as distinct layers
COPY Scraper.csproj ./
RUN dotnet restore

# Build and publish a release
COPY . ./
RUN dotnet publish -c Release -o out

#
# Run time
# 
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /App

COPY --from=build /App/out .

ENTRYPOINT ["dotnet", "Scraper.dll"]
