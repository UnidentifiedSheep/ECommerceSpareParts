FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY . .

RUN --mount=type=cache,id=ecommerce-graphql-nuget,target=/root/.nuget/packages \
    bash scripts/ci/build-graphql.sh /fusion

FROM alpine:3.22

WORKDIR /fusion
COPY --from=build /fusion .

VOLUME ["/output"]

CMD ["sh", "-c", "cp -R /fusion/. /output/"]
