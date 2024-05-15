#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DiagnoseReceiver/DiagnoseReceiver.csproj", "DiagnoseReceiver/"]
COPY ["DiagnoseDataObjects/DiagnoseDataObjects.csproj", "DiagnoseDataObjects/"]
COPY ["HeartDiseasesDiagnosticExtentions/HeartDiseasesDiagnosticExtentions.csproj", "HeartDiseasesDiagnosticExtentions/"]
COPY ["JWTExtensions/JWTExtensions.csproj", "JWTExtensions/"]
COPY ["DataBaseObjects/DataBaseObjects.csproj", "DataBaseObjects/"]
COPY ["UsersCache/UsersCache.csproj", "UsersCache/"]
RUN dotnet restore "DiagnoseReceiver/DiagnoseReceiver.csproj"
COPY . .
WORKDIR "/src/DiagnoseReceiver"
RUN dotnet build "DiagnoseReceiver.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiagnoseReceiver.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiagnoseReceiver.dll"]