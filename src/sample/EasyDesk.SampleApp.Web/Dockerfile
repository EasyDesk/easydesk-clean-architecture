FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/sample/Directory.Build.props", "src/sample/"]
COPY ["Directory.Build.props", "."]
COPY ["src/sample/EasyDesk.SampleApp.Web/EasyDesk.SampleApp.Web.csproj", "src/sample/EasyDesk.SampleApp.Web/"]
COPY ["src/ca/EasyDesk.CleanArchitecture.Web/EasyDesk.CleanArchitecture.Web.csproj", "src/ca/EasyDesk.CleanArchitecture.Web/"]
COPY ["src/ca/EasyDesk.CleanArchitecture.Infrastructure/EasyDesk.CleanArchitecture.Infrastructure.csproj", "src/ca/EasyDesk.CleanArchitecture.Infrastructure/"]
COPY ["src/commons/EasyDesk.Extensions.Configuration/EasyDesk.Extensions.Configuration.csproj", "src/commons/EasyDesk.Extensions.Configuration/"]
COPY ["src/commons/EasyDesk.Commons/EasyDesk.Commons.csproj", "src/commons/EasyDesk.Commons/"]
COPY ["src/ca/EasyDesk.CleanArchitecture.Application/EasyDesk.CleanArchitecture.Application.csproj", "src/ca/EasyDesk.CleanArchitecture.Application/"]
COPY ["src/commons/EasyDesk.Extensions.DependencyInjection/EasyDesk.Extensions.DependencyInjection.csproj", "src/commons/EasyDesk.Extensions.DependencyInjection/"]
COPY ["src/ca/EasyDesk.CleanArchitecture.DependencyInjection/EasyDesk.CleanArchitecture.DependencyInjection.csproj", "src/ca/EasyDesk.CleanArchitecture.DependencyInjection/"]
COPY ["src/ca/EasyDesk.CleanArchitecture.Domain/EasyDesk.CleanArchitecture.Domain.csproj", "src/ca/EasyDesk.CleanArchitecture.Domain/"]
COPY ["src/sample/EasyDesk.SampleApp.Application/EasyDesk.SampleApp.Application.csproj", "src/sample/EasyDesk.SampleApp.Application/"]
COPY ["src/sample/EasyDesk.SampleApp.Domain/EasyDesk.SampleApp.Domain.csproj", "src/sample/EasyDesk.SampleApp.Domain/"]
COPY ["src/sample/EasyDesk.SampleApp.Infrastructure/EasyDesk.SampleApp.Infrastructure.csproj", "src/sample/EasyDesk.SampleApp.Infrastructure/"]
COPY ["src/ca/EasyDesk.CleanArchitecture.Dal.PostgreSql/EasyDesk.CleanArchitecture.Dal.PostgreSql.csproj", "src/ca/EasyDesk.CleanArchitecture.Dal.PostgreSql/"]
COPY ["src/ca/EasyDesk.CleanArchitecture.Dal.EfCore/EasyDesk.CleanArchitecture.Dal.EfCore.csproj", "src/ca/EasyDesk.CleanArchitecture.Dal.EfCore/"]
COPY ["src/ca/EasyDesk.CleanArchitecture.Dal.SqlServer/EasyDesk.CleanArchitecture.Dal.SqlServer.csproj", "src/ca/EasyDesk.CleanArchitecture.Dal.SqlServer/"]
RUN dotnet restore "./src/sample/EasyDesk.SampleApp.Web/./EasyDesk.SampleApp.Web.csproj"
COPY . .
WORKDIR "/src/src/sample/EasyDesk.SampleApp.Web"
RUN dotnet build "./EasyDesk.SampleApp.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EasyDesk.SampleApp.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EasyDesk.SampleApp.Web.dll"]