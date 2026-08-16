FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/AbsurdCourt.Api/AbsurdCourt.Api.csproj src/AbsurdCourt.Api/
COPY src/AbsurdCourt.Application/AbsurdCourt.Application.csproj src/AbsurdCourt.Application/
COPY src/AbsurdCourt.Domain/AbsurdCourt.Domain.csproj src/AbsurdCourt.Domain/
COPY src/AbsurdCourt.Infrastructure/AbsurdCourt.Infrastructure.csproj src/AbsurdCourt.Infrastructure/
RUN dotnet restore src/AbsurdCourt.Api/AbsurdCourt.Api.csproj

COPY src/ src/
RUN dotnet publish src/AbsurdCourt.Api/AbsurdCourt.Api.csproj --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AbsurdCourt.Api.dll"]
