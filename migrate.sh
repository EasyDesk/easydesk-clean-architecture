#!/bin/bash
set -e

USAGE_HELP="Usage: $0 <sample|auth|sagas|messaging|framework|all> <migration_name>"


if [ $# -ne 2 ] || [ -z "$1" ] || [ -z "$2" ] ; then
	echo $USAGE_HELP
	exit 1
fi

TARGET=$1
MIGRATION_NAME=$2

function DAL_MIGRATION_COMMAND_1() {
	dotnet ef migrations add "$1" \
	-s "src/EasyDesk.CleanArchitecture.Dal.$2" \
	-p "src/EasyDesk.CleanArchitecture.Dal.$2" \
	--output-dir "Migrations/$3" \
	--context "${3}Context" \
	--no-build
}

function DAL_MIGRATION_COMMAND() {
	DAL_MIGRATION_COMMAND_1 "$1" PostgreSql "$2"
	DAL_MIGRATION_COMMAND_1 "$1" SqlServer "$2"
}

function SAMPLE_MIGRATION_COMMAND() {
	dotnet ef migrations add "$1" \
	-s "sample/EasyDesk.SampleApp.Web" \
	-p "sample/EasyDesk.SampleApp.Infrastructure" \
	--output-dir "Migrations" \
	--context "SampleAppContext" \
	--no-build
}

AUTH_MIGRATION_COMMAND="DAL_MIGRATION_COMMAND ${MIGRATION_NAME} Authorization"

SAGAS_MIGRATION_COMMAND="DAL_MIGRATION_COMMAND ${MIGRATION_NAME} Sagas"

MESSAGING_MIGRATION_COMMAND="DAL_MIGRATION_COMMAND ${MIGRATION_NAME} Messaging"

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

	framework)
		$AUTH_MIGRATION_COMMAND
		$SAGAS_MIGRATION_COMMAND
		$MESSAGING_MIGRATION_COMMAND
	;;

	all)
		$AUTH_MIGRATION_COMMAND
		$SAGAS_MIGRATION_COMMAND
		$MESSAGING_MIGRATION_COMMAND
		SAMPLE_MIGRATION_COMMAND "$MIGRATION_NAME"
	;;

	*)
		echo $USAGE_HELP
		exit 1
	;;
esac
