FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 暴力法：自動尋找任何層級下的 .csproj，完全無視資料夾名稱和大小寫！
COPY . .
RUN dotnet restore $(find . -name "*.csproj" | head -n 1)
RUN dotnet publish $(find . -name "*.csproj" | head -n 1) -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# 自動尋找編譯好的 .dll 來執行
ENTRYPOINT ["sh", "-c", "dotnet $(ls *.dll | grep -v 'deps.dll' | grep -v 'runtimeconfig.json' | head -n 1)"]