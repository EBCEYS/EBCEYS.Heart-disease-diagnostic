#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["UsersDBAdapter/UsersDBAdapter.csproj", "UsersDBAdapter/"]
COPY ["DataBaseObjects/DataBaseObjects.csproj", "DataBaseObjects/"]
COPY ["HeartDiseasesDiagnosticExtentions/HeartDiseasesDiagnosticExtentions.csproj", "HeartDiseasesDiagnosticExtentions/"]
RUN dotnet restore "UsersDBAdapter/UsersDBAdapter.csproj"
COPY . .
WORKDIR "/src/UsersDBAdapter"
RUN dotnet build "UsersDBAdapter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UsersDBAdapter.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UsersDBAdapter.dll"]