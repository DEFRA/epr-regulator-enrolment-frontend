FROM defradigital/dotnetcore-development:dotnet8.0 AS build-env

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

# Copy everything
COPY --chown=dotnet FrontendRegulatorAccountEnrollment.Web/*.csproj ./FrontendRegulatorAccountEnrollment.Web/
COPY --chown=dotnet FrontendRegulatorAccountEnrollment.Core/*.csproj ./FrontendRegulatorAccountEnrollment.Core/

# Restore as distinct layers
RUN dotnet restore "FrontendRegulatorAccountEnrollment.Web/FrontendRegulatorAccountEnrollment.Web.csproj"

# Install NPM
USER root
RUN apk update && apk add --no-cache npm
USER dotnet
ENV NODE_EXTRA_CA_CERTS=/usr/local/share/ca-certificates/internal-ca.crt

# Build and publish a release
COPY --chown=dotnet FrontendRegulatorAccountEnrollment.Web/. ./FrontendRegulatorAccountEnrollment.Web/.
COPY --chown=dotnet FrontendRegulatorAccountEnrollment.Core/. ./FrontendRegulatorAccountEnrollment.Core/.
WORKDIR /home/dotnet/FrontendRegulatorAccountEnrollment.Web

RUN dotnet publish -c Release -o out

# Build runtime image
FROM defradigital/dotnetcore:dotnet8.0
COPY --from=build-env --chown=dotnet /home/dotnet/FrontendRegulatorAccountEnrollment.Web/out .

# Add internationalisation support
USER root
RUN apk add icu-libs libintl
USER dotnet

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

CMD dotnet FrontendRegulatorAccountEnrollment.Web.dll
