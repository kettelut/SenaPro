FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SenaPro.API/SenaPro.API.csproj", "SenaPro.API/"]
COPY ["SenaPro.Domain/SenaPro.Domain.csproj", "SenaPro.Domain/"]
COPY ["SenaPro.Application/SenaPro.Application.csproj", "SenaPro.Application/"]
COPY ["SenaPro.Infrastructure/SenaPro.Infrastructure.csproj", "SenaPro.Infrastructure/"]
RUN dotnet restore "SenaPro.API/SenaPro.API.csproj"
COPY . .
WORKDIR "/src/SenaPro.API"
RUN dotnet build "SenaPro.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SenaPro.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SenaPro.API.dll"]
