# Dotnet 5 / Vue webserver

A custom dotnet 5 webserver used to host files for download/viewing and uploading files to host machine.

Additional modules include:

1. Chat system
2. Notes
3. Weight tracker
4. Library cataloguing system

**To activate a module for a user log in to an adminstrator account and go to the admin-tools page. From here you can enable modules and additional features of those modules.**

## Technologies used:

- Dotnet core 5
- Vue.js 2.6.11
- Vuetify.js 2.4.5

## Instructions:

1. Clone repo

2. Create the following files in ./VueServer based on appsettings.json with the appropriate information

	- `appsettings.development.json`
	- `appsettings.production.json`

3. Create the following files in ./vue-server-ui/src/config based on config.env.js with the appropriate information. Production uses 'production' from the NODE_ENV.

	- `config.dev.env.js`
	- `config.prod.env.js`

4. Front-End code is in ./vue-server-ui folder
	
	a) To run front-end server with hot-module replacement:

	`npm run serve`
	
	b) To build front-end server
	
	`npm run build`

5. Since it's an SPA in development it is currently setup to run on port 7757 only, this can be changed now in the configuration appsettings file & config.env.json file (these must match).

	`dotnet run --urls https://localhost:7757`

6. It has to be run in development for you to be able to register accounts. This feature is disabled in production for now for security reasons.

7. To build everything on windows 10 x64 and publish the app run .\build.ps1. This will place the output in the .\build folder.

### Required Software:

- Dotnet 5
- MS SQL Server / MySQL (if using type of provider, you can use SQLite if you want instead without installer anything)
- Node.js 10.x+

#### Notes:

1. In appsettings.json: Options.DatabaseType is either 1, 2, or 3 currently. Other database providers can be added later fairly easily.

	- `1 = SQLite`
	- `2 = MS SQL Server`
	- `3 = MySQL`

2. Currently FFMpeg is packaged with VueServer.Services project, but not included in the repo. You will need to download this from https://www.ffmpeg.org/download.html and put ffmpeg.exe and ffprobe.exe in the base level of VueServer.Services project folder.