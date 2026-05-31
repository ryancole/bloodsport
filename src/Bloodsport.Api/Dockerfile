FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Bloodsport.Api/Bloodsport.Api.csproj", "src/Bloodsport.Api/"]
COPY ["src/Bloodsport.Core/Bloodsport.Core.csproj", "src/Bloodsport.Core/"]
COPY ["src/Bloodsport.Data/Bloodsport.Data.csproj", "src/Bloodsport.Data/"]
RUN dotnet restore "src/Bloodsport.Api/Bloodsport.Api.csproj"
COPY . .
RUN dotnet publish "src/Bloodsport.Api/Bloodsport.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bloodsport.Api.dll"]
