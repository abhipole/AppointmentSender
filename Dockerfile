# ============================
# BUILD STAGE
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + project files
COPY ["AppointmentSender.sln", "./"]
COPY ["AppointmentSender/AppointmentSender.csproj", "AppointmentSender/"]

# Restore dependencies
RUN dotnet restore "AppointmentSender/AppointmentSender.csproj"

# Copy rest of the source
COPY . .

# Publish app
WORKDIR "/src/AppointmentSender"
RUN dotnet publish "AppointmentSender.csproj" -c Release -o /app/publish

# ============================
# RUNTIME STAGE
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Agar ye web API hai:
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Agar console app hai to bhi ye chalega, port ignore ho jayega.
ENTRYPOINT ["dotnet", "AppointmentSender.dll"]
