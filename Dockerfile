# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY KonferenscentrumVast/*.csproj KonferenscentrumVast/
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /src/KonferenscentrumVast
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "KonferenscentrumVast.dll"]
