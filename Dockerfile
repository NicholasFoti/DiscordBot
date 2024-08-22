# Use the official .NET SDK image to build and publish the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project files to the working directory
COPY . ./

# Restore dependencies
RUN dotnet restore /app/YoutubeDiscordBot/YoutubeDiscordBot.csproj

# Build the project
RUN dotnet build --configuration Release /app/YoutubeDiscordBot.sln

# Publish the project (prepares the app for deployment)
RUN dotnet publish --configuration Release /app/YoutubeDiscordBot.sln

# Use the official .NET runtime image for the final app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the built project from the build stage
COPY --from=build /app/out .

# Set environment variables (optional)
# ENV ASPNETCORE_URLS=http://+:80

# Expose the port your application will run on
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "YoutubeDiscordBot.dll"]
