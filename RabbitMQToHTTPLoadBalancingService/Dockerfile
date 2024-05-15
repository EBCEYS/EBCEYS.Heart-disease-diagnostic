#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RabbitMQToHTTPLoadBalancingService/RabbitMQToHTTPLoadBalancingService.csproj", "RabbitMQToHTTPLoadBalancingService/"]
COPY ["HeartDiseasesDiagnosticExtentions/HeartDiseasesDiagnosticExtentions.csproj", "HeartDiseasesDiagnosticExtentions/"]
RUN dotnet restore "RabbitMQToHTTPLoadBalancingService/RabbitMQToHTTPLoadBalancingService.csproj"
COPY . .
WORKDIR "/src/RabbitMQToHTTPLoadBalancingService"
RUN dotnet build "RabbitMQToHTTPLoadBalancingService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RabbitMQToHTTPLoadBalancingService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMQToHTTPLoadBalancingService.dll"]