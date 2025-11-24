# =========================
# 建置階段（Build stage）
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 先複製 sln & 各專案 csproj（利用 Docker cache，加快 restore）
COPY ["StockPriceCalculator.sln", "./"]
COPY ["src/StockPriceCalculator.Api/StockPriceCalculator.Api.csproj", "src/StockPriceCalculator.Api/"]
COPY ["src/StockPriceCalculator.Application/StockPriceCalculator.Application.csproj", "src/StockPriceCalculator.Application/"]
COPY ["src/StockPriceCalculator.Infrastructure/StockPriceCalculator.Infrastructure.csproj", "src/StockPriceCalculator.Infrastructure/"]
COPY ["src/StockPriceCalculator.Domain/StockPriceCalculator.Domain.csproj", "src/StockPriceCalculator.Domain/"]

# 還原 NuGet 套件
RUN dotnet restore src/StockPriceCalculator.Api/StockPriceCalculator.Api.csproj

# 複製整個 solution 原始碼
COPY . .

# 切到 API 專案資料夾並 Publish
WORKDIR "/src/src/StockPriceCalculator.Api"
RUN dotnet publish "StockPriceCalculator.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false


# =========================
# 執行階段（Runtime stage）
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# 從 build 階段帶入 publish 出來的檔案
COPY --from=build /app/publish .

# Cloud Run 會注入 PORT 環境變數，這裡讓 ASP.NET 監聽該埠
ENV PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
EXPOSE 8080

ENTRYPOINT ["dotnet", "StockPriceCalculator.Api.dll"]