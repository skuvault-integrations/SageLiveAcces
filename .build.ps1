﻿param
(
)

$project_short_name = "SageLive"
$project_name = "$($project_short_name)Access"

# Folder structure:
# \build - Contains all code during the build process
# \build\artifacts - Contains all files during intermidiate bulid process
# \build\output - Contains the final result of the build process
# \release - Contains final release files for upload
# \release\archive - Contains files archived from the previous builds
# \src - Contains all source code
$build_dir = "$BuildRoot\build"
$build_artifacts_dir = "$build_dir\artifacts"
$build_output_dir = "$build_dir\output"
$release_dir = "$BuildRoot\release"
$archive_dir = "$release_dir\archive"

$src_dir = "$BuildRoot\src"
$solution_file = "$src_dir\$($project_name).sln"
	
# Use MSBuild.
#use Framework\v4.0.30319 MSBuild

try
{
    $VSFolder = Get-ChildItem ( Join-Path ${env:ProgramFiles(x86)} -ChildPath "Microsoft Visual Studio" | Join-Path -ChildPath  "2017" ) -Directory | Select-Object -first 1
    $msbuild15Path = Join-Path $VSFolder.FullName -ChildPath "MSBuild" | Join-Path -ChildPath "15.0" | Join-Path -ChildPath "Bin" | Join-Path -ChildPath "msbuild.exe"
}
catch { }
if ( $msbuild15Path -AND ( Test-Path $msbuild15Path ) )
{ 
    Set-Alias MSBuild $msbuild15Path
    Write-Host "Found BuildTools15: $msbuild15Path"
}
elseIf ( Test-Path "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\14.0" ) 
{ 
    $msbuild14Path = Join-Path -Path ( Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\14.0" ).MSBuildToolsPath -ChildPath "MSBuild.exe"
    Set-Alias MSBuild $msbuild14Path
    Write-Host "Found BuildTools14: $msbuild14Path" 
}
else
{
    Write-Host "To build this solution you need to install BuildTools14 or BuildTools15"
    Write-Host "BuildTools14: https://www.microsoft.com/en-us/download/details.aspx?id=48159"
    Write-Host "BuildTools15: https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=BuildTools&rel=15"
    exit
}


task Clean { 
	exec { MSBuild "$solution_file" /t:Clean /p:Configuration=Release /v:quiet } 
	Remove-Item -force -recurse $build_dir -ErrorAction SilentlyContinue | Out-Null
}

task Init Clean, { 
    New-Item $build_dir -itemType directory | Out-Null
    New-Item $build_artifacts_dir -itemType directory | Out-Null
    New-Item $build_output_dir -itemType directory | Out-Null
}

task Build {
	exec { MSBuild "$solution_file" /t:Build /p:Configuration=Release /v:minimal /p:OutDir="$build_artifacts_dir\" }
}

task Package  {
	New-Item $build_output_dir\$project_name\lib\net45 -itemType directory -force | Out-Null
	Copy-Item $build_artifacts_dir\$project_name.??? $build_output_dir\$project_name\lib\net45 -PassThru |% { Write-Host "Copied " $_.FullName }
}

# Set $script:Version = assembly version
task Version {
	assert (( Get-Item $build_artifacts_dir\$project_name.dll ).VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)')
	$script:Version = $matches[1]
}

task Archive {
	New-Item $release_dir -ItemType directory -Force | Out-Null
	New-Item $archive_dir -ItemType directory -Force | Out-Null
	Move-Item -Path $release_dir\*.* -Destination $archive_dir
}

task NuGet Package, Version, {

	Write-Host ================= Preparing $project_name Nuget package =================
	$text = "$project_short_name webservices API wrapper."
	# nuspec
	Set-Content $build_output_dir\$project_name\$project_name.nuspec @"
<?xml version="1.0"?>
<package>
	<metadata>
		<id>$project_name</id>
		<version>$Version</version>
		<authors>Agile Harbor</authors>
		<owners>Agile Harbor</owners>
		<projectUrl>https://github.com/agileharbor/$project_name</projectUrl>
		<licenseUrl>https://raw.github.com/agileharbor/$project_name/master/License.txt</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<copyright>Copyright (C) Agile Harbor, LLC</copyright>
		<summary>$text</summary>
		<description>$text</description>
		<tags>$project_short_name</tags>
		<dependencies> 
			<group targetFramework="net45">	
				<dependency id="Netco" version="1.5.6" />
				<dependency id="CuttingEdge.Conditions" version="1.2.0.0" />
				<dependency id="ServiceStack.Text" version="4.0.60" />
			</group>
		</dependencies>
	</metadata>
</package>
"@
	# pack
	$nuget = "$($src_dir)\.nuget\NuGet"
	
	exec { & $nuget pack $build_output_dir\$project_name\$project_name.nuspec -Output $build_dir }
	
	$push_project = Read-Host "Push $($project_name) " $Version " to NuGet? (Y/N)"
	Write-Host $push_project
	if( $push_project -eq "y" -or $push_project -eq "Y" )	{
		Get-ChildItem $build_dir\*.nupkg |% { exec { & $nuget push  $_.FullName }}
	}
}

task . Init, Build, Package, NuGet