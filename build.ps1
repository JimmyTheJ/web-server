# Setup path variables
$path = pwd
$build = "$path\build"
$server = "$path\src\Server\VueServer"
$client = "$path\src\Client\vue-server-ui"
$wwwroot = "$server\wwwroot"
$dist = "$client\dist"
$serverBuildOutput = "$server\bin\Release\net5.0\win10-x64\publish\"

# Build Front-End
pushd $client
npm run build

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
if (Test-Path $dist) {
	Copy-Item -Path "$dist\*" -Destination $wwwroot -Recurse
}

# Build Back-End
pushd $server
dotnet publish -c Release -f net5.0 -r win10-x64

if (Test-Path $build) {
	Remove-Item $build -Recurse
	New-Item -ItemType Directory -Path $build
}

# Coffee Break
Start-Sleep -Seconds 1.5

# Copy files over from Front-End to Back-End wwwroot folder
if (Test-Path $serverBuildOutput) {
	Copy-Item -Path $serverBuildOutput -Destination $build -Recurse
}

# Return to base dir
pushd $path