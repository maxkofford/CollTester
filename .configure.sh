#!/bin/bash
#
# Ensure everyone has the Unity YAML merge tool setup in the repository
git config --local --remove-section merge 2>/dev/null
git config --local --remove-section mergetool.unityyamlmerge 2>/dev/null
git config --local --add merge.tool unityyamlmerge
git config --local --add mergetool.unityyamlmerge.trustExitCode false
git config --local --add mergetool.unityyamlmerge.cmd '"$cdup/.yamlmerge.sh" "$BASE" "$REMOTE" "$LOCAL" "$MERGED"'
#
# Update all the submodules
git submodule init
git submodule update --remote --recursive
# git submodule foreach --recursive git checkout master
git submodule foreach -q --recursive 'branch="$(git config -f $toplevel/.gitmodules submodule.$name.branch)"; git checkout $branch'
git submodule foreach --recursive git pull
