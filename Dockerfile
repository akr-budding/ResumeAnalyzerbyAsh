FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["backend/ResumeAnalyzer.API/ResumeAnalyzer.API.csproj", "backend/ResumeAnalyzer.API/"]
RUN dotnet restore "backend/ResumeAnalyzer.API/ResumeAnalyzer.API.csproj"
COPY backend/ResumeAnalyzer.API/ backend/ResumeAnalyzer.API/
RUN dotnet publish "backend/ResumeAnalyzer.API/ResumeAnalyzer.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ResumeAnalyzer.API.dll"]
