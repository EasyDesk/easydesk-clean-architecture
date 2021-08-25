FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["EasyDesk.SampleApp.Web.Web/EasyDesk.SampleApp.Web.Web.csproj", "EasyDesk.SampleApp.Web.Web/"]
COPY ["EasyDesk.SampleApp.Web.Infrastructure/EasyDesk.SampleApp.Web.Infrastructure.csproj", "EasyDesk.SampleApp.Web.Infrastructure/"]
COPY ["EasyDesk.SampleApp.Web.Application/EasyDesk.SampleApp.Web.Application.csproj", "EasyDesk.SampleApp.Web.Application/"]
COPY ["EasyDesk.SampleApp.Web.Domain/EasyDesk.SampleApp.Web.Domain.csproj", "EasyDesk.SampleApp.Web.Domain/"]
RUN dotnet restore "EasyDesk.SampleApp.Web.Web/EasyDesk.SampleApp.Web.Web.csproj"
COPY . .
WORKDIR "/src/EasyDesk.SampleApp.Web.Web"
RUN dotnet build "EasyDesk.SampleApp.Web.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EasyDesk.SampleApp.Web.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EasyDesk.SampleApp.Web.Web.dll"]