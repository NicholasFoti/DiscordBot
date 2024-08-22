# Use the official .NET 8.0 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the csproj file and restore any dependencies
COPY YoutubeDiscordBot.csproj ./
RUN dotnet restore YoutubeDiscordBot.csproj

# Copy the rest of the application
COPY . .

# Build the project
RUN dotnet build --configuration Release --output /app/build

# Publish the project to a specific directory
RUN dotnet publish --configuration Release --output /app/publish /app/YoutubeDiscordBot.csproj

# Use the official .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Expose any necessary ports (optional)
# EXPOSE 80

# Set environment variables (optional)
# ENV ASPNETCORE_URLS=http://+:80

# Start the application
ENTRYPOINT ["dotnet", "YoutubeDiscordBot.dll"]
