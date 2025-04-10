ARG DOTNET_VERSION
FROM otharbor.corp.riotinto.org/otug-dockerimages/aspnet-template:${DOTNET_VERSION}
ARG DOTNET_PROJECT
ENV DOTNET_PROJECT=${DOTNET_PROJECT}
# { here RUN your custom-script.sh e.g: update-ca-certificates, etc. }
RUN apk update && apk upgrade && apk add --no-cache libldap && ln -s libldap.so.2 /usr/lib/libldap-2.5.so.0
COPY app/publish .