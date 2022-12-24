#!/usr/bin/env bash

dotnet publish ./Poncho.Cli/Poncho.Cli.fsproj \
    --configuration Release \
    --framework net7.0 \
    --output ./app \
    --self-contained True \
    --runtime osx.11.0-x64 \
    --verbosity Normal \
    /property:PublishSingleFile=True \
    /property:IncludeNativeLibrariesForSelfExtract=True \
    /property:DebugType=None \
    /property:DebugSymbols=False
