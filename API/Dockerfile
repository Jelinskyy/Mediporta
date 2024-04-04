# Building Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ./API ./API
RUN dotnet restore "./API/API.csproj" 
RUN dotnet publish "./API/API.csproj" -o /out

# Serve Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 
WORKDIR /app
COPY --from=build /out ./

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000

ENTRYPOINT [ "dotnet", "API.dll" ]


