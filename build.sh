#!/bin/bash

# Setup path variables
path=$(pwd)
build="$path/build"
server="$path/src/Server/VueServer.Main"
client="$path/src/Client/vue-server-ui"
wwwroot="$server/wwwroot"
dist="$client/dist"
env="ubuntu.22.04-x64"
framework="net5.0"
serverBuildOutput="$server/bin/Release/$framework/$env/publish/"

# Build Front-End
cd $client
npm run build

# Check if the wwwroot directory or its subdirectories exist, and delete them
if [ -d "$wwwroot" ]; then
    if [ -d "$wwwroot/css" ]; then
        rm -rf "$wwwroot/css"
    fi
    if [ -d "$wwwroot/fonts" ]; then
        rm -rf "$wwwroot/fonts"
    fi
    if [ -d "$wwwroot/js" ]; then
        rm -rf "$wwwroot/js"
    fi
fi

# Coffee Break
sleep 1.5

# Create wwwroot folder if needed
if [ ! -d "$wwwroot" ]; then
    mkdir -p "$wwwroot"
fi

# Coffee Break
sleep 1.5

# Copy files over from Front-End to Back-End wwwroot folder
if [ -d "$dist" ]; then
    cp -r "$dist/"* "$wwwroot"
fi

# Build Back-End
cd $server
dotnet publish -c Release -f $framework -r $env

# Handle build directory
if [ -d "$build" ]; then
    rm -rf "$build"
    mkdir -p "$build"
fi

# Coffee Break
sleep 1.5

# Copy files over from Front-End to Back-End wwwroot folder
if [ -d "$serverBuildOutput" ]; then
    cp -r "$serverBuildOutput"/* "$build"
fi

# Return to base dir
cd $path

