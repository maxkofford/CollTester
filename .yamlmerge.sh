#!/usr/bin/bash
BINARY='C:\Program Files\Unity\Editor\Data\Tools\UnityYAMLMerge.exe'
if [ -e "${BINARY}" ] ; then
	"${BINARY}" merge -p "$1" "$2" "$3" "$4"
	exit 0
else
	echo "Can't find the yaml merge tool"
	exit 1
fi
