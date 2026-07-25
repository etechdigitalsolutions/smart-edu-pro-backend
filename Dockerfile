########################################
# Stage 1: Build
########################################
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only csproj files first (layer caching for faster rebuilds)
COPY ["src/SmartEduPro.WebApi/SmartEduPro.WebApi.csproj", "src/SmartEduPro.WebApi/"]
COPY ["src/SmartEduPro.Application/SmartEduPro.Application.csproj", "src/SmartEduPro.Application/"]
COPY ["src/SmartEduPro.Domain/SmartEduPro.Domain.csproj", "src/SmartEduPro.Domain/"]
COPY ["src/SmartEduPro.Infrastructure/SmartEduPro.Infrastructure.csproj", "src/SmartEduPro.Infrastructure/"]

RUN dotnet restore "src/SmartEduPro.WebApi/SmartEduPro.WebApi.csproj"

# Copy the rest of the source code
COPY . .

WORKDIR /src/src/SmartEduPro.WebApi
RUN dotnet publish "SmartEduPro.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

########################################
# Stage 2: Runtime
########################################
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway provides the PORT env var at runtime; Kestrel must bind to it
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartEduPro.WebApi.dll"]