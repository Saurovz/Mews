FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

# required build args
ARG FEED_ACCESSTOKEN
ARG PACKAGE_VERSION
# validate required build args
RUN if [ -z "${FEED_ACCESSTOKEN}" ]; then echo "Error: FEED_ACCESSTOKEN is not set"; exit 1; fi
RUN if [ -z "${PACKAGE_VERSION}" ]; then echo "Error: PACKAGE_VERSION is not set"; exit 1; fi

# Install credentials provider and register custom feed
RUN <<eot bash
  apt-get update
  apt-get install -y curl
  curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | sh
eot
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://pkgs.dev.azure.com/mews/_packaging/mews/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"

COPY . .

# Run dotnet restore excluding specific projects
RUN find . -name '*.csproj' \
    -not -path '*/aspire/*' -not -path '*/Aspire/*' \
    -exec dotnet restore {} \;

RUN dotnet publish "src/hosts/TaxManager.Api/TaxManager.Api.csproj" -c Release -o /app --no-restore /p:Version=$PACKAGE_VERSION

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "TaxManager.dll"]
