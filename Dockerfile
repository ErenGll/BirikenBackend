FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Sadece proje dosyasını kopyalayıp restore yapıyoruz
COPY *.csproj ./
RUN dotnet restore

# Kalan her şeyi kopyalayıp yayınlıyoruz
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Render portu
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "BirikenAPI.dll"]