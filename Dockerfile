FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ExpenseTracker.API/ExpenseTracker.API.csproj", "ExpenseTracker.API/"]
RUN dotnet restore "ExpenseTracker.API/ExpenseTracker.API.csproj"
COPY . .
WORKDIR "/src/ExpenseTracker.API"
RUN dotnet build "ExpenseTracker.API.csproj" -c Release --no-restore -o /app/build

FROM build AS publish
RUN dotnet publish "ExpenseTracker.API.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "ExpenseTracker.API.dll"]
