[CmdletBinding()]
param (
    [Parameter(Mandatory=$false, HelpMessage="Enter your config name defined in developers.json: ")][string]$configName,
    [switch]$skipBuild
)

$environment = Get-Content 'developers.json' | Out-String | ConvertFrom-Json
$config = $environment.$configName
$developerPrefix = $config.Prefix

$stackName = "pecuniary-transactionquery-stack"

$sourceFile = "samTemplate.yaml"
$localSourceFile = "$sourceFile.local"
Write-Host "`nCreating/updating $localSourceFile based on $sourceFile..."
Copy-Item samTemplate.yaml $localSourceFile

if ($config.Prefix)
{  
    Write-Host "`n`tDeveloper config selected" -ForegroundColor Yellow
    Write-Host "`Parameters from " -NoNewline
    Write-Host "developers.json:`n" -ForegroundColor Cyan
    Write-Host "`tdeveloperPrefix: `t`t $developerPrefix" -ForegroundColor Yellow

    $stackName = $developerPrefix + "-" + $stackName

    (Get-Content $localSourceFile) `
        -replace 'pecuniary-', "$developerPrefix-pecuniary-" `
        -replace 'Name: pecuniary', "Name: $developerPrefix-pecuniary" |
    Out-File $localSourceFile -Encoding utf8

    Write-Host "`nDone! $localSourceFile updated. Please use this file when deploying to our own AWS stack.`n"

    Write-Host "Press [enter] to continue deploying stack to AWS (Ctrl+C to exit)" -NoNewline -ForegroundColor Green
    Read-Host
}

if (!$skipBuild)
{
    Write-Host "`n`nRestoring projects..." -ForegroundColor Cyan

    dotnet restore

    Write-Host "`n`nBuilding projects..." -ForegroundColor Cyan

    dotnet publish -c Release
}

Write-Host "`n`nDeploying stack $stackName..." -ForegroundColor Cyan

dotnet-lambda deploy-serverless `
    --stack-name $stackName `
    --template $localSourceFile `
    --region us-west-2 `
    --s3-bucket pecuniary-deployment-artifacts

# Handle deploy errors
if ($lastexitcode -ne 0) {
    throw "Error deploying" + $stackName
}

# Get the API Gateway Base URL
$stack = aws cloudformation describe-stacks --stack-name $stackName --region us-west-2 | ConvertFrom-Json
$outputKey = $stack.Stacks.Outputs.OutputKey.IndexOf("PecuniaryApiGatewayBaseUrl")
$apiGatewayBaseUrl = $stack.Stacks.Outputs[$outputKey].OutputValue

# Add scopes
Write-Host "`n`Adding OAuth scopes..."
aws lambda invoke `
    --function-name "pecuniary-AddScopes" `
    --payload """{ """"ApiGatewayBaseUrl"""": """"$apiGatewayBaseUrl"""" }""" `
    --region us-west-2 `
    outfile.json

# Handle add scope errors
if ($lastexitcode -ne 0) {
    throw "Error adding OAuth scopes"
}
Remove-Item outfile.json

Write-Host "`n`n YOUR STACK NAME IS:   " -NoNewLine
Write-Host "$stackName   `n`n" -ForegroundColor Cyan
