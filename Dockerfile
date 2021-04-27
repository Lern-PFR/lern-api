#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Lern-API/Lern-API.csproj", "Lern-API/"]
RUN dotnet restore "Lern-API/Lern-API.csproj"
COPY . .
WORKDIR "/src/Lern-API"
RUN dotnet build "Lern-API.csproj" -c Release -o /app/build --no-restore

FROM build AS tests
WORKDIR /src
COPY ["Lern-API.Tests/Lern-API.Tests.csproj", "Lern-API.Tests/"]
COPY ["Lern-API.Tests/", "Lern-API.Tests/"]
WORKDIR "/src"
COPY ["Lern-API.sln", "./"]
RUN dotnet restore "Lern-API.Tests/Lern-API.Tests.csproj"
RUN dotnet test --no-restore -v n

FROM build AS publish
RUN dotnet publish "Lern-API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lern-API.dll"]
