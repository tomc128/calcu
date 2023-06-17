FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Calcu/Calcu.csproj", "Calcu/"]
RUN dotnet restore "Calcu/Calcu.csproj"
COPY . .
WORKDIR "/src/Calcu"
RUN dotnet build "Calcu.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Calcu.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Calcu.dll"]
