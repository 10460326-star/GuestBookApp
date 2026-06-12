FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 改用萬用字元 * 去抓大資料夾，不管你大寫小寫，通通繞過去！
COPY */*.csproj ./
RUN dotnet restore

COPY . .
# 這裡直接用 * 走進你的專案資料夾
WORKDIR /src/GuestBookApp
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GuestbookApp.dll"]