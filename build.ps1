# Build Front-End
cd vue-server-ui
npm run build

# Clean up old folders / files
cd ..

# Setup path variables
$path = pwd
$wwwroot = "$path\VueServer\wwwroot"

# Check if the wwwroot directory or it's subdirectories exist, and delete them
if (Test-Path $wwwroot) {
	if (Test-Path $wwwroot\css) {
		Remove-Item $wwwroot\css -Recurse
	}
	if (Test-Path $wwwroot\fonts) {
		Remove-Item $wwwroot\fonts -Recurse
	}
	if (Test-Path $wwwroot\js) {
		Remove-Item $wwwroot\js -Recurse
	}
}

# Coffee Break
Start-Sleep -Seconds 1.5

# Create wwwroot folder if needed
if (-not (Test-Path $wwwroot -PathType Container)) {
	New-Item -ItemType Directory -Path $wwwroot
}

# Coffee Break
Start-Sleep -Seconds 1.5

# Copy files over from Front-End to Back-End wwwroot folder
if (Test-Path vue-server-ui\dist) {
	Copy-Item -Path $path\vue-server-ui\dist\* -Destination $wwwroot -Recurse
}

# Build Back-End
cd VueServer
dotnet publish -c Release -f netcoreapp3.1 -r win10-x64

# Return to base dir
cd ..