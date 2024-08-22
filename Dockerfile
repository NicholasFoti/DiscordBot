# Use the official .NET 8.0 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the csproj file
COPY YoutubeDiscordBot/YoutubeDiscordBot.csproj YoutubeDiscordBot/

# Restore dependencies
RUN dotnet restore YoutubeDiscordBot/YoutubeDiscordBot.csproj

# Copy the rest of the application
COPY YoutubeDiscordBot/ YoutubeDiscordBot/

# Build the project
RUN dotnet build --configuration Release --output /app/build /app/YoutubeDiscordBot/YoutubeDiscordBot.csproj

# Publish the project
RUN dotnet publish --configuration Release --output /app/publish /app/YoutubeDiscordBot/YoutubeDiscordBot.csproj

# Use the official .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "YoutubeDiscordBot.dll"]
