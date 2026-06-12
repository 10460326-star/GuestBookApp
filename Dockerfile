# 1. 升級使用微軟官方最新的 .NET 10.0 SDK 進行編譯
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 2. 精準對齊你的真實資料夾名稱與專案檔（皆為 GuestbookApp）
COPY ["GuestbookApp/GuestbookApp.csproj", "GuestbookApp/"]
RUN dotnet restore "GuestbookApp/GuestbookApp.csproj"

COPY . .
WORKDIR "/src/GuestbookApp"
RUN dotnet build "GuestbookApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GuestbookApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. 升級使用 .NET 10.0 執行環境
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GuestbookApp.dll"]