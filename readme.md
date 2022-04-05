# Dotnet 5 / Vue webserver

A custom modular dotnet 5 webserver

Additional modules include:

1. Chat system - SignalR live chat system
2. Fileserver - Browse, Download, upload, view functionality
3. Notes
4. Weight tracker
5. Library - Book cataloguing system

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

3. Initialize database:

	- If running in dev mode and your plan is to use database migrations run the following commands in `./src/Server/VueServer` folder. These migrations will be places in the `./Migrations` folder from this relative path.
	
		1. `dotnet ef migrations add InitialCreate --context [Insert Context Type here. See below]`
		
		2. `dotnet ef database update --context [Insert Context Type here. See below]`
	
	- If running in production and you just want to setup the database once:
	
		1. ... Coming soon!

4. To build everything on windows 10 x64 and publish the app run `powershell .\build.ps1`. This will place the output in the `.\build` folder.

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

3. System will be created with a default Adminstrator account called `admin`. The password can be found in: `./src/Server/Domain/Common/DomainConstants.cs` under variable `DEFAULT_PASSWORD`

4. Context Types:

	- `SqliteWSContext`
	- `SQLServerWSContext`
	- `MySqlWSContext`