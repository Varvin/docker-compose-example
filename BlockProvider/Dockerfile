FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src/BlockProvider
COPY . /src

RUN dotnet restore 
RUN dotnet publish --configuration Release --output /bin --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=build /bin .

ENV ENVIRONMENT='Production'
ENV ASPNETCORE_URLS="http://*:80"
EXPOSE 80

ENTRYPOINT dotnet BlockProvider.dll
