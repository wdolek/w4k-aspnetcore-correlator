#!/bin/bash
current_dir=$(pwd)

cd ./test/W4k.AspNetCore.Correlator.UnitTests

dotnet test -c:Debug \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:Exclude="[xunit*]\*" \
  /p:CoverletOutput=./TestResults/

reportgenerator \
  -reports:TestResults/coverage.cobertura.xml \
  -targetdir:./TestResults/html \
  -reporttypes:HTML

cd $current_dir
