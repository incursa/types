[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string] $PackageDirectory = "artifacts/repro1"
)

$ErrorActionPreference = "Stop"

$resolvedPackageDirectory = (Resolve-Path -LiteralPath $PackageDirectory).Path
$package = Get-ChildItem -LiteralPath $resolvedPackageDirectory -Filter "Incursa.Types.Primitives.*.nupkg" |
    Where-Object { -not $_.Name.EndsWith(".snupkg", [StringComparison]::OrdinalIgnoreCase) } |
    Sort-Object Name |
    Select-Object -Last 1

if ($null -eq $package) {
    throw "No Incursa.Types.Primitives package was found in '$resolvedPackageDirectory'."
}

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

    $namespace = [System.Xml.XmlNamespaceManager]::new($nuspec.NameTable)
    $namespace.AddNamespace("n", $nuspec.DocumentElement.NamespaceURI)
    $packageId = $nuspec.SelectSingleNode("/n:package/n:metadata/n:id", $namespace).InnerText
    $packageVersion = $nuspec.SelectSingleNode("/n:package/n:metadata/n:version", $namespace).InnerText
    $runtimeDependencies = @($nuspec.SelectNodes("/n:package/n:metadata/n:dependencies//n:dependency", $namespace))

    if (-not [string]::Equals($packageId, "Incursa.Types.Primitives", [StringComparison]::Ordinal)) {
        throw "Unexpected package ID '$packageId'."
    }

    if ($runtimeDependencies.Count -ne 0) {
        $ids = $runtimeDependencies | ForEach-Object { $_.GetAttribute("id") }
        throw "Primitives package contains runtime dependencies: $($ids -join ', ')."
    }
}
finally {
    $archive.Dispose()
}

$consumerRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("incursa-primitives-consumer-" + [Guid]::NewGuid().ToString("N"))
try {
    dotnet new console --name PackageConsumer --output $consumerRoot --no-restore | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to create package consumer fixture."
    }

    $consumerProject = Join-Path $consumerRoot "PackageConsumer.csproj"
    $consumerPackageCache = Join-Path $consumerRoot "packages"
    $projectText = Get-Content -LiteralPath $consumerProject -Raw
    $projectText = $projectText -replace "<TargetFramework>[^<]+</TargetFramework>", "<TargetFramework>net8.0</TargetFramework>"
    Set-Content -LiteralPath $consumerProject -Value $projectText -Encoding utf8

    dotnet add $consumerProject package $packageId --version $packageVersion --source $resolvedPackageDirectory --no-restore | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to add '$packageId' version '$packageVersion' to the package consumer fixture."
    }

    @'
using Incursa;

FastId id = FastId.From(42);
Percentage percentage = Percentage.ParseScaled("12.5");
Duration duration = Duration.Parse("PT1H");
Money money = new(1.01m);
BigDecimal exact = BigDecimal.Parse("12345678901234567890.0100");
BigDecimal roundedEighth = BigDecimal.One.DivideRounded(BigDecimal.Parse("8"), 2, BigDecimalRoundingMode.ToNearestTiesToEven);
ElapsedDuration elapsed = ElapsedDuration.Parse("-PT1.0000001S");
LocalDateTime local = LocalDateTime.Parse("2024-02-29T12:34:56.1234567");

if (id.Value != 42 || percentage.Value != 0.125m || duration.Hours != 1 || money.ToWords() != "one dollar and one cent" || exact.ToString() != "1234567890123456789001e-2" || roundedEighth.ToString() != "12e-2" || elapsed.Units != -10000001 || local.SubsecondTicks != 1234567 || LocalDateTime.FromTicks(local.Ticks) != local)
{
    return 1;
}

Console.WriteLine($"{id}|{percentage}|{duration}|{money.ToWords()}|{exact}|{roundedEighth}|{elapsed}|{local}");
return 0;
'@ | Set-Content -LiteralPath (Join-Path $consumerRoot "Program.cs") -Encoding utf8

    dotnet restore $consumerProject --packages $consumerPackageCache -p:RestoreAdditionalProjectSources=$resolvedPackageDirectory
    if ($LASTEXITCODE -ne 0) {
        throw "The package-only consumer restore failed."
    }

    dotnet run --project $consumerProject --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "The package-only consumer failed."
    }
}
finally {
    if (Test-Path -LiteralPath $consumerRoot) {
        Remove-Item -LiteralPath $consumerRoot -Recurse -Force
    }
}

Write-Host "Verified $packageId ${packageVersion}: package-only restore/build/run passed with zero runtime package dependencies."
