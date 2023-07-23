#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CarReservationApi/CarReservationApi.csproj", "CarReservationApi/"]
RUN dotnet restore "CarReservationApi/CarReservationApi.csproj"
COPY . .
WORKDIR "/src/CarReservationApi"
RUN dotnet build "CarReservationApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CarReservationApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarReservationApi.dll"]