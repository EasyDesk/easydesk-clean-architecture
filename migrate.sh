#!/bin/bash
set -e

USAGE_HELP="Usage: $0 <sample|auth|sagas|messaging|msg|audit|auditing|framework|all> <migration_name>"


if [ $# -ne 2 ] || [ -z "$1" ] || [ -z "$2" ] ; then
	echo $USAGE_HELP
	exit 1
fi

TARGET=$1
MIGRATION_NAME=$2

function DAL_MIGRATION_COMMAND_GENERATOR() {
	dotnet ef migrations add "$1" \
		-s "src/ca/EasyDesk.CleanArchitecture.Dal.$2" \
		-p "src/ca/EasyDesk.CleanArchitecture.Dal.$2" \
		--output-dir "Migrations/$3" \
		--context "${3}Context" \
		--no-build
}

function DAL_MIGRATION_COMMAND() {
	DAL_MIGRATION_COMMAND_GENERATOR "$1" PostgreSql "$2"
	DAL_MIGRATION_COMMAND_GENERATOR "$1" SqlServer "$2"
	DAL_MIGRATION_COMMAND_GENERATOR "$1" Sqlite "$2"
}

function SAMPLE_MIGRATION_COMMAND_GENERATOR() {
	dotnet ef migrations add "$1" \
		-s "src/sample/EasyDesk.SampleApp.Web" \
		-p "src/sample/EasyDesk.SampleApp.Infrastructure" \
		--output-dir "EfCore/Migrations/$2" \
		--context "${2}SampleAppContext" \
		--no-build \
		-- --dbprovider "$2"
}

function SAMPLE_MIGRATION_COMMAND() {
	SAMPLE_MIGRATION_COMMAND_GENERATOR "$1" PostgreSql
	SAMPLE_MIGRATION_COMMAND_GENERATOR "$1" SqlServer
	SAMPLE_MIGRATION_COMMAND_GENERATOR "$1" Sqlite
}

AUTH_MIGRATION_COMMAND="DAL_MIGRATION_COMMAND ${MIGRATION_NAME} Auth"

SAGAS_MIGRATION_COMMAND="DAL_MIGRATION_COMMAND ${MIGRATION_NAME} Sagas"

MESSAGING_MIGRATION_COMMAND="DAL_MIGRATION_COMMAND ${MIGRATION_NAME} Messaging"

AUDITING_MIGRATION_COMMAND="DAL_MIGRATION_COMMAND ${MIGRATION_NAME} Auditing"

dotnet build

case "$TARGET" in

	sample)
		SAMPLE_MIGRATION_COMMAND "$MIGRATION_NAME"
	;;

	auth)
		$AUTH_MIGRATION_COMMAND
	;;

	sagas)
		$SAGAS_MIGRATION_COMMAND
	;;

	messaging | msg)
		$MESSAGING_MIGRATION_COMMAND
	;;

	audit | auditing)
		$AUDITING_MIGRATION_COMMAND
	;;

	framework)
		$AUTH_MIGRATION_COMMAND
		$SAGAS_MIGRATION_COMMAND
		$MESSAGING_MIGRATION_COMMAND
		$AUDITING_MIGRATION_COMMAND
	;;

	all)
		$AUTH_MIGRATION_COMMAND
		$SAGAS_MIGRATION_COMMAND
		$MESSAGING_MIGRATION_COMMAND
		$AUDITING_MIGRATION_COMMAND
		SAMPLE_MIGRATION_COMMAND "$MIGRATION_NAME"
	;;

	*)
		echo $USAGE_HELP
		exit 1
	;;
esac
