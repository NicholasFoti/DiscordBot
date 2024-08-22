# Use the official .NET 8.0 SDK preview image to build and publish the app
FROM mcr.microsoft.com/dotnet/nightly/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project files to the working directory
COPY . ./

# Restore dependencies
RUN dotnet restore

# Build the project
RUN dotnet build --configuration Release --output out

# Publish the project (prepares the app for deployment)
RUN dotnet publish --configuration Release --output out

# Use the official .NET runtime image for the final app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the built project from the build stage
COPY --from=build /app/out .

# Expose the port your application will run on
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "YoutubeDiscordBot.dll"]
