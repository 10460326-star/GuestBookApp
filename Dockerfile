FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 修正重點：把 GuestBookApp.csproj 全部的 B 都改成小寫 b
COPY ["GuestBookApp/GuestbookApp.csproj", "GuestBookApp/"]
RUN dotnet restore "GuestBookApp/GuestbookApp.csproj"

COPY . .
WORKDIR "/src/GuestBookApp"
RUN dotnet build "GuestbookApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GuestbookApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GuestbookApp.dll"]