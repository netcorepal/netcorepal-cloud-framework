#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["NuGet.config", "."]
COPY ["test/NetCorePal.Web/NetCorePal.Web.csproj", "test/NetCorePal.Web/"]
RUN dotnet restore "test/NetCorePal.Web/NetCorePal.Web.csproj"
COPY . .
WORKDIR "/src/test/NetCorePal.Web"
RUN dotnet build "NetCorePal.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetCorePal.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCorePal.Web.dll"]