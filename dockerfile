# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + project files first for better layer caching
COPY ASP.NET_MVC_Chat_Test.sln ./
COPY MVC_SSL_Chat/MVC_SSL_Chat.csproj MVC_SSL_Chat/
COPY Application/Application.csproj Application/
COPY Contracts/Contracts.csproj Contracts/
COPY DomainModels/DomainModels.csproj DomainModels/
COPY Storage/Storage.csproj Storage/
COPY Email/Email.csproj Email/
COPY SecuritySupplements/SecuritySupplements.csproj SecuritySupplements/

RUN dotnet restore ASP.NET_MVC_Chat_Test.sln

# Copy the rest of the source
COPY . .

# Publish the web app
RUN dotnet publish MVC_SSL_Chat/MVC_SSL_Chat.csproj -c Release -o /app/publish /p:UseAppHost=false

# Generate a self-signed cert for HTTPS (demo only)
RUN apt-get update && apt-get install -y --no-install-recommends openssl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /https \
    && openssl req -x509 -nodes -newkey rsa:2048 -days 365 \
      -subj "/CN=localhost" \
      -keyout /https/aspnetapp.key -out /https/aspnetapp.crt \
    && openssl pkcs12 -export -out /https/aspnetapp.pfx \
      -inkey /https/aspnetapp.key -in /https/aspnetapp.crt \
      -password pass:devpassword \
    && rm /https/aspnetapp.key /https/aspnetapp.crt

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./
COPY --from=build /https /https

ENV ASPNETCORE_URLS="https://+:8443" \
    ASPNETCORE_HTTPS_PORT="8443" \
    ASPNETCORE_Kestrel__Certificates__Default__Path="/https/aspnetapp.pfx" \
    ASPNETCORE_Kestrel__Certificates__Default__Password="devpassword"

EXPOSE 8443

ENTRYPOINT ["dotnet", "MVC_SSL_Chat.dll"]
