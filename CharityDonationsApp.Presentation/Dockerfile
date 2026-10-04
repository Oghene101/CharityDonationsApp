FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["CharityDonationsApp.Domain/CharityDonationsApp.Domain.csproj", "CharityDonationsApp.Domain/"]
COPY ["CharityDonationsApp.Application/CharityDonationsApp.Application.csproj", "CharityDonationsApp.Application/"]
COPY ["CharityDonationsApp.Infrastructure/CharityDonationsApp.Infrastructure.csproj", "CharityDonationsApp.Infrastructure/"]
COPY ["CharityDonationsApp.Presentation/CharityDonationsApp.Presentation.csproj", "CharityDonationsApp.Presentation/"]
RUN dotnet restore "CharityDonationsApp.Presentation/CharityDonationsApp.Presentation.csproj"
COPY . .
WORKDIR "/src/CharityDonationsApp.Presentation"
RUN dotnet build "CharityDonationsApp.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CharityDonationsApp.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CharityDonationsApp.Presentation.dll"]
