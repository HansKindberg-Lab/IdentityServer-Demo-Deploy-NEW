param
(
	[Parameter(Mandatory)]
	[SecureString]$password
)

$_certificateStoreLocation = "CERT:\CurrentUser\My\";
$_name = "Identity-Server-Signing";
$_subject = "CN=$($_name)";

$certificate = New-SelfsignedCertificate `
	-CertStoreLocation $_certificateStoreLocation `
	-HashAlgorithm "sha256" `
	-KeyAlgorithm "RSA" `
	-KeyExportPolicy "Exportable" `
	-KeyLength 2048 `
	-KeySpec "Signature" `
	-Subject $_subject;

$certificateFilePath = "$($PSScriptRoot)\$($_name).pfx";
$certificateValueFilePath = "$($certificateFilePath).bytes";
$subject = $certificate.Subject;
$thumbprint = $certificate.Thumbprint;
	
Export-PfxCertificate -Cert $certificate -FilePath $certificateFilePath -Password $password;

[System.Convert]::ToBase64String((Get-Content $certificateFilePath -Encoding Byte)) | Out-File $certificateValueFilePath;

$blob = Get-Content $certificateValueFilePath;

Remove-Item -Path "$($_certificateStoreLocation)$($certificate.Thumbprint)";
Remove-Item -Path $certificateFilePath;
Remove-Item -Path $certificateValueFilePath;

Write-Host;
Write-Host "Note the following:";
Write-Host;
Write-Host "Blob:";
Write-Host $blob;
Write-Host;
Write-Host "Name:";
Write-Host $_name;
Write-Host;
Write-Host "Subject:";
Write-Host $subject;
Write-Host;
Write-Host "Thumbprint:";
Write-Host $thumbprint;

Write-Host;
Write-Host "And remember your password!";

Write-Host;
Write-Host "Press any key to close";
Read-Host;