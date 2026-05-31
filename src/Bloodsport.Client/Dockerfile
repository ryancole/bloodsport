FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Bloodsport.Client/Bloodsport.Client.csproj", "src/Bloodsport.Client/"]
COPY ["src/Bloodsport.Core/Bloodsport.Core.csproj", "src/Bloodsport.Core/"]
RUN dotnet restore "src/Bloodsport.Client/Bloodsport.Client.csproj"
COPY . .
RUN dotnet publish "src/Bloodsport.Client/Bloodsport.Client.csproj" -c Release -o /app/publish

FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/Bloodsport.Client/nginx.conf /etc/nginx/nginx.conf
