# Use the latest version of the .NET SDK as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the project files to the container
COPY . .

# Build the application
RUN dotnet publish -c Release -o out

# Use a lighter version of .NET for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory in the container
WORKDIR /app

# Copy the published app from the build environment
COPY --from=build /app/out .

# Expose the port used by the application
EXPOSE 80

# Command to run the application
CMD ["dotnet", "CryptoProject.dll"]
