FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_URLS=http://+:7105
WORKDIR /app
EXPOSE 7105

# Base for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS buildbase
WORKDIR /source

COPY ./API ./API
RUN dotnet restore "API/API.csproj" 

## Run migrations
FROM buildbase as migrations

RUN dotnet tool install -g dotnet-ef --version 8.*
ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT dotnet ef database update -p API