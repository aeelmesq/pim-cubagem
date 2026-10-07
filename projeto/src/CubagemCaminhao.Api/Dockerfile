FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/CubagemCaminhao.Api/CubagemCaminhao.Api.csproj", "src/CubagemCaminhao.Api/"]
COPY ["src/CubagemCaminhao.Domain/CubagemCaminhao.Domain.csproj", "src/CubagemCaminhao.Domain/"]
COPY ["src/CubagemCaminhao.Infrastructure/CubagemCaminhao.Infrastructure.csproj", "src/CubagemCaminhao.Infrastructure/"]
RUN dotnet restore "src/CubagemCaminhao.Api/CubagemCaminhao.Api.csproj"

COPY . .
RUN dotnet publish "src/CubagemCaminhao.Api/CubagemCaminhao.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CubagemCaminhao.Api.dll"]
