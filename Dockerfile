FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY .config/dotnet-tools.json .config/
COPY business_logic/business_logic.csproj business_logic/
COPY data_access/data_access.csproj data_access/
COPY transport-logistic-management-2026/fleet-management-system-2026.csproj transport-logistic-management-2026/
RUN dotnet restore transport-logistic-management-2026/fleet-management-system-2026.csproj
COPY business_logic/ business_logic/
COPY data_access/ data_access/
COPY transport-logistic-management-2026/ transport-logistic-management-2026/
RUN dotnet publish transport-logistic-management-2026/fleet-management-system-2026.csproj --configuration Release --no-restore --output /app/publish

FROM build AS migration-bundle-build
RUN dotnet tool restore \
    && dotnet ef migrations bundle \
        --project data_access \
        --startup-project transport-logistic-management-2026 \
        --configuration Release \
        --self-contained \
        --target-runtime linux-x64 \
        --output /app/efbundle

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS migrations
WORKDIR /app
COPY --from=migration-bundle-build /app/efbundle ./efbundle
COPY --from=build /app/publish/appsettings.json ./appsettings.json
USER $APP_UID
ENTRYPOINT ["./efbundle"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "fleet-management-system-2026.dll"]
