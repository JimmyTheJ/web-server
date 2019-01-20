# Dotnet core 2.2 / Vue webserver

A custom dotnet core 2.2 webserver used to host files for download/viewing and uploading files to host machine. Also has a little note taking component.

## Technologies used:

Vue.js

Vuetify.js

Dotnet core 2.2

## Instructions:

1. Clone repo

2. create following files based on appsettings.json with the appropriate information

`appsettings.development.json`

`appsettings.production.json`

3. Since it's an SPA in development it is currently setup to run on port 7757 only. If you're running from the command line you'll need to either force the application to run on this port or find every instance of 7757 in the app and change to whatever you want to use. If using Visual Studio you can setup a profile in Properties/launchSettings.json to use port 7757.

`dotnet run --urls https://localhost:7757`

### Required Software:

- Dotnet core 2.2

- MS SQL Server (See below)

- Node.js

#### Notes:

1. In appsettings.json: Options.DatabaseType is either 1 or 2 currently. Other database providers can be added later fairly easily.

`1 = SQLite`
`2 = MS SQL Server`

2. In ./VueServer/ClientApp/config there are two files which are used to set the axios default path to send requests as

