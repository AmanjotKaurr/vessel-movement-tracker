# Use the official .NET 8.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV DOTNET_SYSTEM_NET_DISABLEIPV6=true 

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VesselMovementAPI.csproj", "./"]
RUN dotnet restore "./VesselMovementAPI.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "VesselMovementAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VesselMovementAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VesselMovementAPI.dll"]