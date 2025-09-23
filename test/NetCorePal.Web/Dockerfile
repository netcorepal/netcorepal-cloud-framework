#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["NuGet.config", "."]
COPY ["test/NetCorePal.Web/NetCorePal.Web.csproj", "test/NetCorePal.Web/"]
RUN dotnet restore "test/NetCorePal.Web/NetCorePal.Web.csproj"
COPY . .
WORKDIR "/src/test/NetCorePal.Web"
RUN dotnet build "NetCorePal.Web.csproj" -c Release -f net10.0 -o /app/build

FROM build AS publish
RUN dotnet publish "NetCorePal.Web.csproj" -c Release -f net10.0 -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCorePal.Web.dll"]
