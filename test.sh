#!/bin/sh

rm TestResults -rf 2> /dev/null
rm coverage-report -rf 2> /dev/null

dotnet test -p:EmitGenerated=true --results-directory ./TestResults --coverage --coverage-output-format cobertura

reportgenerator -reports:"TestResults/**/*.cobertura.xml" -targetdir:coverage-report -reporttypes:Html

rm TestResults -rf 2> /dev/null

xdg-open coverage-report/index.htm
