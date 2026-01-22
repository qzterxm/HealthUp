# --- ЕТАП 1: БУДІВНИЦТВО (Build) ---
# Завантажуємо "важкий" образ з усіма інструментами (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Копіюємо файли проектів (це робиться окремо для швидкості)
# Зверни увагу: я використовую назви з твоїх логів (CentralAPI та DataAccess)
COPY ["CentralAPI/CentralAPI.csproj", "CentralAPI/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]

# 2. Завантажуємо всі бібліотеки (NuGet пакети)
RUN dotnet restore "CentralAPI/CentralAPI.csproj"

# 3. Копіюємо ВЕСЬ інший код
COPY . .

# 4. Збираємо проект
WORKDIR "/src/CentralAPI"
RUN dotnet build "CentralAPI.csproj" -c Release -o /app/build

# 5. Публікуємо (створюємо фінальні .dll файли)
FROM build AS publish
RUN dotnet publish "CentralAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- ЕТАП 2: ЗАПУСК (Run) ---
# Завантажуємо "легкий" образ тільки для запуску (економить місце)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Відкриваємо порт 8080 (стандарт для Render)
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Копіюємо готові файли з етапу "publish" сюди
COPY --from=publish /app/publish .

# Запускаємо програму
ENTRYPOINT ["dotnet", "CentralAPI.dll"]