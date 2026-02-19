# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["S2O1.API/S2O1.API.csproj", "S2O1.API/"]
COPY ["S2O1.CLI/S2O1.CLI.csproj", "S2O1.CLI/"]
COPY ["S2O1.Business/S2O1.Business.csproj", "S2O1.Business/"]
COPY ["S2O1.Core/S2O1.Core.csproj", "S2O1.Core/"]
COPY ["S2O1.DataAccess/S2O1.DataAccess.csproj", "S2O1.DataAccess/"]
COPY ["S2O1.Domain/S2O1.Domain.csproj", "S2O1.Domain/"]
RUN dotnet restore "S2O1.API/S2O1.API.csproj"
RUN dotnet restore "S2O1.CLI/S2O1.CLI.csproj"

# Copy everything else and build
COPY . .

# Build API
WORKDIR "/src/S2O1.API"
RUN dotnet build "S2O1.API.csproj" -c Release -o /app/build_api

# Build CLI
WORKDIR "/src/S2O1.CLI"
RUN dotnet build "S2O1.CLI.csproj" -c Release -o /app/build_cli

# Publish Stage
FROM build AS publish
WORKDIR "/src/S2O1.API"
RUN dotnet publish "S2O1.API.csproj" -c Release -o /app/publish_api /p:UseAppHost=false

WORKDIR "/src/S2O1.CLI"
RUN dotnet publish "S2O1.CLI.csproj" -c Release -o /app/publish_cli /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish_api .
COPY --from=publish /app/publish_cli ./cli

# Create a pre-configured dbconfig.txt for CLI inside Docker
RUN echo "Server=db;Database=2S1O;User Id=sa;Password=SqlPassword123!;TrustServerCertificate=True;" > ./cli/dbconfig.txt
# Create the installation flag for Linux so CLI doesn't ask for setup
RUN mkdir -p /etc/2s1o && echo "Installed=true" > /etc/2s1o/installed.flag

# Copy the static web files into the app directory
COPY web ./web

# Expose port and start
EXPOSE 5267
ENV ASPNETCORE_URLS=http://+:5267
ENTRYPOINT ["dotnet", "S2O1.API.dll"]

