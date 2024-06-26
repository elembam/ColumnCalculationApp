# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy the csproj and restore as distinct layers
COPY *.fsproj .
RUN dotnet restore

# Copy the remaining files and build the application
COPY . .
RUN dotnet publish -c Release -o /app

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Expose the port the app runs on
EXPOSE 5000

# Run the application
ENTRYPOINT ["dotnet", "ColumnCalculationApp.dll"]
