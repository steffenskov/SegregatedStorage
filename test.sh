#!/bin/sh
rm -rf TestResults coverage-report 2> /dev/null

dotnet test --results-directory ./TestResults --collect:"XPlat Code Coverage"

reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:coverage-report \
  -reporttypes:Html \
  -filefilters:"-**/obj/**"

rm -rf TestResults 2> /dev/null
xdg-open coverage-report/index.html
