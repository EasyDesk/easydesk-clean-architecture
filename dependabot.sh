#!/bin/bash

function dependabot() {
	echo "  - package-ecosystem: nuget"
    echo "    directory: ${1}"
	echo "    schedule:"
	echo "      interval: daily"
	echo "    labels:"
	echo "      - dependency"
	echo "    open-pull-requests-limit: 0 # no limit"
	echo ""
}

for PROJ in $( find . -name '*.csproj' | sed -r 's/^\.(.+)\/[^\/]+$/\1/g' ) ; do
	dependabot "$PROJ"
done
