#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AlertStorageService/AlertStorageService.csproj", "AlertStorageService/"]
COPY ["DataBaseObjects/DataBaseObjects.csproj", "DataBaseObjects/"]
COPY ["HeartDiseasesDiagnosticExtentions/HeartDiseasesDiagnosticExtentions.csproj", "HeartDiseasesDiagnosticExtentions/"]
COPY ["DiagnoseDataObjects/DiagnoseDataObjects.csproj", "DiagnoseDataObjects/"]
RUN dotnet restore "AlertStorageService/AlertStorageService.csproj"
COPY . .
WORKDIR "/src/AlertStorageService"
RUN dotnet build "AlertStorageService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlertStorageService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlertStorageService.dll"]