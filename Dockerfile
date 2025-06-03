# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY */*.csproj ./                       # Hoặc điều chỉnh nếu project không có folder con
RUN for file in */*.csproj; do dotnet restore "$file"; done

COPY . .
RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "AppointmentHospital.dll"]
