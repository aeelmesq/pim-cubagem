FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CubagemCaminhao.Blazor.csproj", "./"]
RUN dotnet restore "CubagemCaminhao.Blazor.csproj"

COPY . .
RUN dotnet publish "CubagemCaminhao.Blazor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CubagemCaminhao.Blazor.dll"]
