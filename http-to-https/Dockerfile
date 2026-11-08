FROM mcr.microsoft.com/dotnet/sdk:11.0-alpine AS build

WORKDIR /source

# copy csproj and restore as distinct layers
COPY http-to-https/*.csproj .

RUN dotnet restore -r linux-musl-x64

# copy and publish app and libraries
COPY http-to-https/ .

RUN dotnet publish -c release -o /app -r linux-musl-x64 --no-restore

# final stage/image
# for ioxide
#FROM mcr.microsoft.com/dotnet/runtime:11.0-alpine
# for kestrel
FROM mcr.microsoft.com/dotnet/aspnet:11.0-alpine

ENV DOTNET_EnableDiagnostics=0 \
    DOTNET_gcServer=1 \
    DOTNET_TieredPGO=1 \
    DOTNET_ReadyToRun=1 \
    PORT=8080

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "http-to-https.dll"]

EXPOSE 8080