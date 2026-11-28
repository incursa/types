[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string] $PackageDirectory = "artifacts/repro1",

    [Parameter(Mandatory = $false)]
    [string] $BaselineCommit = "d6ce1c8392aa76149374b08d9cdb97c66d5f2084"
)

$ErrorActionPreference = "Stop"
$resolvedPackageDirectory = (Resolve-Path -LiteralPath $PackageDirectory).Path

function Get-Package([string] $id) {
    $package = Get-ChildItem -LiteralPath $resolvedPackageDirectory -Filter "$id.*.nupkg" |
        Where-Object {
            -not $_.Name.EndsWith(".snupkg", [StringComparison]::OrdinalIgnoreCase) -and
            $_.Name -match "^$([regex]::Escape($id))\.\d"
        } |
        Sort-Object Name |
        Select-Object -Last 1
    if ($null -eq $package) {
        throw "No $id package was found in '$resolvedPackageDirectory'."
    }

    return $package
}

function Read-Package([System.IO.FileInfo] $package) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($package.FullName)
    try {
        $nuspecEntry = $archive.Entries |
            Where-Object { $_.FullName.EndsWith(".nuspec", [StringComparison]::OrdinalIgnoreCase) } |
            Select-Object -First 1
        if ($null -eq $nuspecEntry) {
            throw "Package '$($package.Name)' has no nuspec."
        }

        $reader = [System.IO.StreamReader]::new($nuspecEntry.Open())
        try {
            [xml] $nuspec = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }

        $frameworkDlls = @($archive.Entries |
            Where-Object { $_.FullName -match '^lib/([^/]+)/[^/]+\.dll$' } |
            ForEach-Object { [regex]::Match($_.FullName, '^lib/([^/]+)/').Groups[1].Value } |
            Sort-Object -Unique)
        return @{
            Nuspec = $nuspec
            Frameworks = $frameworkDlls
        }
    }
    finally {
        $archive.Dispose()
    }
}

function Get-Metadata([xml] $nuspec, [string] $xpath) {
    $namespace = [System.Xml.XmlNamespaceManager]::new($nuspec.NameTable)
    $namespace.AddNamespace("n", $nuspec.DocumentElement.NamespaceURI)
    return $nuspec.SelectSingleNode($xpath, $namespace)
}

function Get-MetadataNodes([xml] $nuspec, [string] $xpath) {
    $namespace = [System.Xml.XmlNamespaceManager]::new($nuspec.NameTable)
    $namespace.AddNamespace("n", $nuspec.DocumentElement.NamespaceURI)
    return @($nuspec.SelectNodes($xpath, $namespace))
}

function Invoke-DotNet([string[]] $arguments) {
    & dotnet @arguments | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function New-PackageConsumer(
    [string] $root,
    [string] $name,
    [string] $packageId,
    [string] $version,
    [string] $program) {
    $consumerRoot = Join-Path $root $name
    Invoke-DotNet @("new", "console", "--name", $name, "--output", $consumerRoot, "--no-restore")
    $project = Join-Path $consumerRoot "$name.csproj"
    $projectText = Get-Content -LiteralPath $project -Raw
    $projectText = $projectText -replace "<TargetFramework>[^<]+</TargetFramework>", "<TargetFramework>net8.0</TargetFramework>"
    Set-Content -LiteralPath $project -Value $projectText -Encoding utf8
    Invoke-DotNet @("add", $project, "package", $packageId, "--version", $version, "--source", $resolvedPackageDirectory, "--no-restore")
    Set-Content -LiteralPath (Join-Path $consumerRoot "Program.cs") -Value $program -Encoding utf8
    Invoke-DotNet @("restore", $project, "--packages", $qualificationPackageCache, "-p:RestoreAdditionalProjectSources=$resolvedPackageDirectory")
    Invoke-DotNet @("build", $project, "--configuration", "Release", "--no-restore")
    Invoke-DotNet @("run", "--project", $project, "--configuration", "Release", "--no-build", "--no-restore")
    return @{
        Root = $consumerRoot
        Project = $project
        Output = Join-Path $consumerRoot "bin/Release/net8.0"
        Assets = Join-Path $consumerRoot "obj/project.assets.json"
    }
}

$primitivesPackage = Get-Package "Incursa.Types.Primitives"
$typesPackage = Get-Package "Incursa.Types"
$primitives = Read-Package $primitivesPackage
$types = Read-Package $typesPackage
$primitivesId = (Get-Metadata $primitives.Nuspec "/n:package/n:metadata/n:id").InnerText
$typesId = (Get-Metadata $types.Nuspec "/n:package/n:metadata/n:id").InnerText
$primitivesVersion = (Get-Metadata $primitives.Nuspec "/n:package/n:metadata/n:version").InnerText
$typesVersion = (Get-Metadata $types.Nuspec "/n:package/n:metadata/n:version").InnerText

if ($primitivesId -ne "Incursa.Types.Primitives" -or $typesId -ne "Incursa.Types") {
    throw "Unexpected package identities '$primitivesId' and '$typesId'."
}

if ($primitivesVersion -ne $typesVersion) {
    throw "Package versions are not synchronized: Primitives=$primitivesVersion Types=$typesVersion."
}

if (@($primitives.Frameworks).Count -ne 1 -or $primitives.Frameworks[0] -ne "net8.0" -or
    @($types.Frameworks).Count -ne 1 -or $types.Frameworks[0] -ne "net8.0") {
    throw "Both packages must expose exactly the preserved net8.0 target framework."
}

$primitivesDependencies = @(Get-MetadataNodes $primitives.Nuspec "/n:package/n:metadata/n:dependencies//n:dependency")
if ($primitivesDependencies.Count -ne 0) {
    throw "Primitives contains runtime package dependencies."
}

$typesPrimitivesDependency = Get-Metadata $types.Nuspec "/n:package/n:metadata/n:dependencies//n:dependency[@id='Incursa.Types.Primitives']"
if ($null -eq $typesPrimitivesDependency -or $typesPrimitivesDependency.GetAttribute("version") -ne $primitivesVersion) {
    throw "Types does not declare the synchronized Primitives package version."
}

$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("itq-" + [Guid]::NewGuid().ToString("N").Substring(0, 8))
$qualificationPackageCache = Join-Path $temporaryRoot "p"
$baselineRoot = Join-Path $temporaryRoot "baseline"
$baselineAttached = $false
try {
    New-Item -ItemType Directory -Path $temporaryRoot | Out-Null

    $primitivesConsumer = New-PackageConsumer $temporaryRoot "PrimitivesConsumer" $primitivesId $primitivesVersion @'
using Incursa;
using System.Text.Json;

BigDecimal exact = BigDecimal.Parse("12345678901234567890.0100");
CalendarDuration calendar = CalendarDuration.Parse("P1Y1DT1.0000001S");
ElapsedDuration elapsed = ElapsedDuration.Parse("-PT1.0000001S");
LocalDateTime local = LocalDateTime.Parse("2024-02-29T12:34:56.1234567");
ExactPeriod period = ExactPeriod.FromElapsed(DateTimeOffset.UnixEpoch, new ElapsedDuration(1));

if (exact.ToString() != "1234567890123456789001e-2" ||
    calendar.ToString() != "P12M1DT1.0000001S" ||
    elapsed.Units != -10000001 ||
    LocalDateTime.FromTicks(local.Ticks) != local ||
    period.Elapsed.Units != 1 ||
    JsonSerializer.Deserialize<CalendarDuration>(JsonSerializer.Serialize(calendar)) != calendar)
{
    return 1;
}

Console.WriteLine("Primitives package-only consumer passed.");
return 0;
'@

    $primitivesAssets = Get-Content -LiteralPath $primitivesConsumer.Assets -Raw | ConvertFrom-Json
    $primitivesLibraryNames = @($primitivesAssets.libraries.PSObject.Properties.Name)
    $primitivesRuntimePackages = @($primitivesLibraryNames | Where-Object { $_ -notlike "Incursa.Types.Primitives/*" })
    if ($primitivesRuntimePackages.Count -ne 0) {
        throw "Primitives consumer resolved unexpected packages: $($primitivesRuntimePackages -join ', ')."
    }

    $typesConsumer = New-PackageConsumer $temporaryRoot "TypesConsumer" $typesId $typesVersion @'
using Incursa;

EmailAddress email = EmailAddress.Parse("person@example.com");
Money money = new(1.01m);
Duration duration = Duration.Parse("PT1H");

if (email.ToString() != "person@example.com" ||
    money.ToWords() != "one dollar and one cent" ||
    duration.Hours != 1 ||
    typeof(Money).Assembly.GetName().Name != "Incursa.Types.Primitives" ||
    typeof(EmailAddress).Assembly.GetName().Name != "Incursa.Types")
{
    return 1;
}

Console.WriteLine("Types package-only consumer and transitive Primitives reference passed.");
return 0;
'@

    $typesAssets = Get-Content -LiteralPath $typesConsumer.Assets -Raw | ConvertFrom-Json
    $typesLibraryNames = @($typesAssets.libraries.PSObject.Properties.Name)
    if ($typesLibraryNames -notcontains "Incursa.Types/$typesVersion" -or
        $typesLibraryNames -notcontains "Incursa.Types.Primitives/$primitivesVersion") {
        throw "Types consumer did not resolve both Incursa packages."
    }

    & git worktree add --detach $baselineRoot $BaselineCommit | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to materialize compatibility baseline $BaselineCommit."
    }

    $baselineAttached = $true
    $baselinePackages = Join-Path $temporaryRoot "baseline-packages"
    New-Item -ItemType Directory -Path $baselinePackages | Out-Null
    Invoke-DotNet @(
        "build",
        (Join-Path $baselineRoot "Incursa.Types/Incursa.Types.csproj"),
        "--configuration", "Release",
        "-p:PackageVersion=0.9.0-compatbaseline"
    )
    Invoke-DotNet @(
        "pack",
        (Join-Path $baselineRoot "Incursa.Types/Incursa.Types.csproj"),
        "--configuration", "Release",
        "--output", $baselinePackages,
        "--no-build",
        "-p:PackageVersion=0.9.0-compatbaseline"
    )

    $oldConsumerRoot = Join-Path $temporaryRoot "OldCompiledConsumer"
    Invoke-DotNet @("new", "console", "--name", "OldCompiledConsumer", "--output", $oldConsumerRoot, "--no-restore")
    $oldProject = Join-Path $oldConsumerRoot "OldCompiledConsumer.csproj"
    $oldProjectText = Get-Content -LiteralPath $oldProject -Raw
    $oldProjectText = $oldProjectText -replace "<TargetFramework>[^<]+</TargetFramework>", "<TargetFramework>net8.0</TargetFramework>"
    Set-Content -LiteralPath $oldProject -Value $oldProjectText -Encoding utf8
    Invoke-DotNet @("add", $oldProject, "package", "Incursa.Types", "--version", "0.9.0-compatbaseline", "--source", $baselinePackages, "--no-restore")
    Set-Content -LiteralPath (Join-Path $oldConsumerRoot "Program.cs") -Value @'
using Incursa;

Money money = new(12.34m);
Duration duration = Duration.Parse("PT1H");
Percentage percentage = Percentage.ParseScaled("12.5");

if (money.Value != 12.34m || duration.Hours != 1 || percentage.Value != 0.125m ||
    typeof(Money).Assembly.GetName().Name != "Incursa.Types.Primitives" ||
    Type.GetType("Incursa.Money, Incursa.Types", throwOnError: true) != typeof(Money))
{
    return 1;
}

Console.WriteLine("Old compiled consumer resolved moved types through current forwarders.");
return 0;
'@ -Encoding utf8
    Invoke-DotNet @("restore", $oldProject, "--packages", $qualificationPackageCache, "-p:RestoreAdditionalProjectSources=$baselinePackages")
    Invoke-DotNet @("build", $oldProject, "--configuration", "Release", "--no-restore")

    $oldOutput = Join-Path $oldConsumerRoot "bin/Release/net8.0"
    Copy-Item -LiteralPath (Join-Path $typesConsumer.Output "Incursa.Types.dll") -Destination $oldOutput -Force
    Copy-Item -LiteralPath (Join-Path $typesConsumer.Output "Incursa.Types.Primitives.dll") -Destination $oldOutput -Force

    $hostRoot = Join-Path $temporaryRoot "CompatibilityHost"
    Invoke-DotNet @("new", "console", "--name", "CompatibilityHost", "--output", $hostRoot, "--no-restore")
    $hostProject = Join-Path $hostRoot "CompatibilityHost.csproj"
    $hostProjectText = Get-Content -LiteralPath $hostProject -Raw
    $hostProjectText = $hostProjectText -replace "<TargetFramework>[^<]+</TargetFramework>", "<TargetFramework>net8.0</TargetFramework>"
    Set-Content -LiteralPath $hostProject -Value $hostProjectText -Encoding utf8
    Set-Content -LiteralPath (Join-Path $hostRoot "Program.cs") -Value @'
using System.Reflection;
using System.Runtime.Loader;

string consumerDirectory = Path.GetFullPath(args[0]);
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    if (name.Name is null || !name.Name.StartsWith("Incursa.", StringComparison.Ordinal))
    {
        return null;
    }

    string candidate = Path.Combine(consumerDirectory, $"{name.Name}.dll");
    return File.Exists(candidate) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(candidate) : null;
};

Assembly consumer = AssemblyLoadContext.Default.LoadFromAssemblyPath(
    Path.Combine(consumerDirectory, "OldCompiledConsumer.dll"));
object? result = consumer.EntryPoint!.Invoke(null, new object?[] { Array.Empty<string>() });
return result switch
{
    int exitCode => exitCode,
    Task<int> task => await task,
    _ => 0,
};
'@ -Encoding utf8
    Invoke-DotNet @("restore", $hostProject, "--packages", $qualificationPackageCache)
    Invoke-DotNet @("run", "--project", $hostProject, "--configuration", "Release", "--no-restore", "--", $oldOutput)
}
finally {
    if ($baselineAttached) {
        & git worktree remove --force $baselineRoot | Out-Null
    }

    if (Test-Path -LiteralPath $temporaryRoot) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}

Write-Host "Verified both $typesVersion packages, resolved runtime graphs, synchronized dependency constraint, and old compiled-consumer forwarding."
