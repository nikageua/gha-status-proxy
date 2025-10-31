FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/GhaStatusProxy/*.csproj src/GhaStatusProxy/
RUN dotnet restore src/GhaStatusProxy/GhaStatusProxy.csproj

COPY src/GhaStatusProxy/ src/GhaStatusProxy/

RUN dotnet publish src/GhaStatusProxy/GhaStatusProxy.csproj -c Release -o /app \
    --no-self-contained -p:PublishSingleFile=false -p:SelfContained=false -p:PublishTrimmed=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet","/app/GhaStatusProxy.dll"]
