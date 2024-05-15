#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DBAdapterService/DBAdapterService.csproj", "DBAdapterService/"]
COPY ["DataBaseObjects/DataBaseObjects.csproj", "DataBaseObjects/"]
COPY ["HeartDiseasesDiagnosticExtentions/HeartDiseasesDiagnosticExtentions.csproj", "HeartDiseasesDiagnosticExtentions/"]
RUN dotnet restore "DBAdapterService/DBAdapterService.csproj"
COPY . .
WORKDIR "/src/DBAdapterService"
RUN dotnet build "DBAdapterService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DBAdapterService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DBAdapterService.dll"]