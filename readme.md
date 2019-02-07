# Dotnet core 3.1 / Vue webserver

A custom dotnet core 3.1 webserver used to host files for download/viewing and uploading files to host machine. Also has a little note taking component.

## Technologies used:

Vue.js 2.6.x

Vuetify.js 2.1.5

Dotnet core 3.1

## Instructions:

1. Clone repo

2. create following files based on appsettings.json with the appropriate information

`appsettings.development.json`

`appsettings.production.json`

3. You need to go into ./VueServer and run the following to get all the node package dependencies.

`npm install`

4. Since it's an SPA in development it is currently setup to run on port 7757 only, this can be changed now in the configuration appsettings file.

`dotnet run --urls https://localhost:7757`

5. It has to be run in development for you to be able to register accounts. This feature is disabled in production.

6. Currently FFMpeg is packaged with VueServer.Services project, but not included in the repo. You will need to download this from https://www.ffmpeg.org/download.html and put ffmpeg.exe and ffprobe.exe in the base level of VueServer.Services project folder.

### Required Software:

- Dotnet core 3.1

- MS SQL Server / MySQL if using one of those (See below) 

- Node.js

#### Notes:

1. In appsettings.json: Options.DatabaseType is either 1, 2, or 3 currently. Other database providers can be added later fairly easily.

`1 = SQLite`
`2 = MS SQL Server`
`3 = MySQL`

2. In ./VueServer/ClientApp/config there are two files which are used to set the axios default path to send requests as

