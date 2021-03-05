# Dotnet 5 / Vue webserver

A custom dotnet 5 webserver used to host files for download/viewing and uploading files to host machine.

Additional modules include:

1. Chat system
2. Notes
3. Weight tracker
4. Library cataloguing system

**To activate a module for a user: log in to an adminstrator account and go to the admin-tools page.**
**From here you can enable modules and additional features of those modules.**

## Technologies used:

- Dotnet core 5
- Vue.js 2.6.11
- Vuetify.js 2.4.5

## Setup instructions:

1. Clone repo

2. Create the following files in `./src/Server/VueServer` based on appsettings.json with the appropriate information

	- `appsettings.development.json`
	- `appsettings.production.json`

3. Create the following files in `./src/Client/vue-server-ui` based on `.env.*` with the appropriate information.

	- `.env.development.local`
	- `.env.production.local`

### Building Instructions:

1. Client code is in `./src/Client/vue-server-ui` folder. Run `npm install` in this folder to start.
	
	a) To run front-end server with hot-module replacement:

	`npm run serve`
	
	b) To build front-end server
	
	`npm run build`

2. Server code is in `./src/Server/VueServer` and is currently setup to run on port 7757 by default. This can now be changed in the configuration files: `appsettings.*.json` & `config.*.env.json` (the hostname and/or port must match).

	`dotnet run --urls https://localhost:7757`

3. To build everything on windows 10 x64 and publish the app run `powershell .\build.ps1`. 
This will place the output in the `.\build` folder.

#### Required Software:

- Dotnet 5
- Node.js 10.x+
- MS SQL Server / MySQL (optional)

##### Notes:

1. In appsettings.json: Options.DatabaseType is either 1, 2, or 3 currently. Other database providers can be added later fairly easily.

	- `1 = SQLite`
	- `2 = MS SQL Server`
	- `3 = MySQL`

2. Currently FFMpeg is packaged with VueServer.Services project, but not included in the repo. You will need to download this from https://www.ffmpeg.org/download.html and put ffmpeg.exe and ffprobe.exe in the base level of VueServer.Services project folder.

3. Application has to be run in development for you to be able to register accounts. This feature is disabled in production for now for security reasons.