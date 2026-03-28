FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ECommerce.sln", "."]
COPY ["src/ECommerce.Core/ECommerce.Core.csproj", "src/ECommerce.Core/"]
COPY ["src/ECommerce.UseCases/ECommerce.UseCases.csproj", "src/ECommerce.UseCases/"]
COPY ["src/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj", "src/ECommerce.Infrastructure/"]
COPY ["src/ECommerce.WebApi/ECommerce.WebApi.csproj", "src/ECommerce.WebApi/"]
RUN dotnet restore "src/ECommerce.WebApi/ECommerce.WebApi.csproj"

COPY . .
RUN dotnet publish "src/ECommerce.WebApi/ECommerce.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.WebApi.dll"]
