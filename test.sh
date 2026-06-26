#!/bin/sh

set -e

rm -rf TestResults 2> /dev/null

dotnet test --results-directory ./TestResults --collect:"XPlat Code Coverage"

reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:coverage-report \
  -reporttypes:Html \
  -filefilters:"-**/obj/**"


[ -f coverage-report/index.html ]
xdg-open coverage-report/index.html
