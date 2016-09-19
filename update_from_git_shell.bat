@echo off
cd %~dp0
@echo #
@echo # Updating all git submodules to their latest versions...
rem #1 git submodule update --init --remote --recursive
rem #2 git submodule init
rem #2 git submodule foreach --recursive git checkout master
rem #2 git submodule foreach --recursive git pull
bash .\.configure.sh
@echo #
@echo # Checking and updating symbolic links...
.\.configure.lnk
@echo #
@echo # Finished
@echo #
