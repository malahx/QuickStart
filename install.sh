#!/bin/bash
VAR=QuickStart
cp -f bin/${VAR}.dll GameData/${VAR}/Plugins/
cp -f README.md GameData/${VAR}/
cp -f COPYING GameData/${VAR}/
cp -f ${VAR}.version GameData/${VAR}/
rm -rf ../00KSP-dev/GameData/${VAR}/
cp -rf GameData/${VAR} ../00KSP-dev/GameData/${VAR}
