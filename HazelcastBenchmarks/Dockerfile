FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HazelcastBenchmarks/HazelcastBenchmarks.csproj", "HazelcastBenchmarks/"]
RUN dotnet restore "HazelcastBenchmarks/HazelcastBenchmarks.csproj"
COPY . .
WORKDIR "/src/HazelcastBenchmarks"
RUN dotnet build "HazelcastBenchmarks.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HazelcastBenchmarks.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HazelcastBenchmarks.dll"]
