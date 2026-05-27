FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY InvestmentWebApp.csproj ./
RUN dotnet restore "InvestmentWebApp.csproj"

COPY . ./
RUN dotnet publish "InvestmentWebApp.csproj" -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "InvestmentWebApp.dll"]
