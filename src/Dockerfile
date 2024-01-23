#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/pterodactyl-discord-bot.csproj", "src/"]
RUN dotnet restore "src/pterodactyl-discord-bot.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "pterodactyl-discord-bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "pterodactyl-discord-bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pterodactyl-discord-bot.dll"]