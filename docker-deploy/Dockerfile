# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 5000

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY UsuarioAPI.sln ./
COPY src/Usuario.API/Usuario.API.csproj ./src/Usuario.API/

RUN dotnet restore "./src/Usuario.API/Usuario.API.csproj"

COPY src/Usuario.API/ ./src/Usuario.API/

RUN dotnet build "./src/Usuario.API/Usuario.API.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./src/Usuario.API/Usuario.API.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/publish \
  --no-restore

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5035
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Usuario.API.dll"]
