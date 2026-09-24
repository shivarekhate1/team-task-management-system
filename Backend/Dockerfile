# Multi-stage Dockerfile for ASP.NET Core Web API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["Backend/TaskManagement.API/TaskManagement.API.csproj", "Backend/TaskManagement.API/"]
COPY ["Backend/TaskManagement.Application/TaskManagement.Application.csproj", "Backend/TaskManagement.Application/"]
COPY ["Backend/TaskManagement.Domain/TaskManagement.Domain.csproj", "Backend/TaskManagement.Domain/"]
COPY ["Backend/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "Backend/TaskManagement.Infrastructure/"]

RUN dotnet restore "Backend/TaskManagement.API/TaskManagement.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/Backend/TaskManagement.API"
RUN dotnet build "TaskManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]
