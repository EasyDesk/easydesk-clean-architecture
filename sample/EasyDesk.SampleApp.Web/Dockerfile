FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["sample/EasyDesk.SampleApp.Web/EasyDesk.SampleApp.Web.csproj", "sample/EasyDesk.SampleApp.Web/"]
COPY ["src/EasyDesk.CleanArchitecture.Web/EasyDesk.CleanArchitecture.Web.csproj", "src/EasyDesk.CleanArchitecture.Web/"]
COPY ["src/EasyDesk.CleanArchitecture.Infrastructure/EasyDesk.CleanArchitecture.Infrastructure.csproj", "src/EasyDesk.CleanArchitecture.Infrastructure/"]
COPY ["src/EasyDesk.CleanArchitecture.Application/EasyDesk.CleanArchitecture.Application.csproj", "src/EasyDesk.CleanArchitecture.Application/"]
COPY ["src/EasyDesk.CleanArchitecture.DependencyInjection/EasyDesk.CleanArchitecture.DependencyInjection.csproj", "src/EasyDesk.CleanArchitecture.DependencyInjection/"]
COPY ["src/EasyDesk.CleanArchitecture.Domain/EasyDesk.CleanArchitecture.Domain.csproj", "src/EasyDesk.CleanArchitecture.Domain/"]
COPY ["sample/EasyDesk.SampleApp.Application/EasyDesk.SampleApp.Application.csproj", "sample/EasyDesk.SampleApp.Application/"]
COPY ["sample/EasyDesk.SampleApp.Domain/EasyDesk.SampleApp.Domain.csproj", "sample/EasyDesk.SampleApp.Domain/"]
COPY ["sample/EasyDesk.SampleApp.Infrastructure/EasyDesk.SampleApp.Infrastructure.csproj", "sample/EasyDesk.SampleApp.Infrastructure/"]
COPY ["src/EasyDesk.CleanArchitecture.Dal.PostgreSql/EasyDesk.CleanArchitecture.Dal.PostgreSql.csproj", "src/EasyDesk.CleanArchitecture.Dal.PostgreSql/"]
COPY ["src/EasyDesk.CleanArchitecture.Dal.EfCore/EasyDesk.CleanArchitecture.Dal.EfCore.csproj", "src/EasyDesk.CleanArchitecture.Dal.EfCore/"]
RUN dotnet restore "sample/EasyDesk.SampleApp.Web/EasyDesk.SampleApp.Web.csproj"
COPY . .
WORKDIR "/src/sample/EasyDesk.SampleApp.Web"
RUN dotnet build "EasyDesk.SampleApp.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EasyDesk.SampleApp.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EasyDesk.SampleApp.Web.dll"]