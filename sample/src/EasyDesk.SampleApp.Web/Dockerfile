FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["EasyDesk.SampleApp.Web/EasyDesk.SampleApp.Web.csproj", "EasyDesk.SampleApp.Web.Web/"]
COPY ["EasyDesk.SampleApp.Infrastructure/EasyDesk.SampleApp.Infrastructure.csproj", "EasyDesk.SampleApp.Infrastructure/"]
COPY ["EasyDesk.SampleApp.Application/EasyDesk.SampleApp.Application.csproj", "EasyDesk.SampleApp.Application/"]
COPY ["EasyDesk.SampleApp.Domain/EasyDesk.SampleApp.Domain.csproj", "EasyDesk.SampleApp.Domain/"]
RUN dotnet restore "EasyDesk.SampleApp.Web/EasyDesk.SampleApp.Web.csproj"
COPY . .
WORKDIR "/src/EasyDesk.SampleApp.Web"
RUN dotnet build "EasyDesk.SampleApp.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EasyDesk.SampleApp.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EasyDesk.SampleApp.Web.dll"]