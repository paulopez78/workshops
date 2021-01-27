# STAGE 1: PUBLISH
FROM mcr.microsoft.com/dotnet/sdk AS builder

WORKDIR /app
COPY . .

RUN dotnet publish -o /app/publish -c Release


# STAGE 2: CREATE RUNTIME IMAGE
FROM mcr.microsoft.com/dotnet/aspnet

WORKDIR /app

COPY --from=builder /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT [ "dotnet", "MeetupEvents.dll" ]