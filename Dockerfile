# 1. 使用 .NET SDK 編譯
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 2. 這是關鍵：因為你的程式碼在 GuestBookApp 資料夾裡，我們要進去
COPY ["GuestBookApp/GuestbookApp.csproj", "GuestBookApp/"]
RUN dotnet restore "GuestBookApp/GuestbookApp.csproj"

COPY . .
WORKDIR "/src/GuestBookApp"
RUN dotnet build "GuestbookApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GuestbookApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. 運行網頁
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GuestbookApp.dll"]