FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Dosyaları kopyala ve yayınla
COPY . ./
RUN dotnet restore
RUN dotnet publish "BirikenAPI.csproj" -c Release -o out

# Çalıştırma aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Render port ayarı
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "BirikenAPI.dll"]