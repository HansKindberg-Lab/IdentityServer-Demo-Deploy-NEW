$code = @"
	using Microsoft.Extensions.Configuration;
	public class MyClass
	{
		public static int Multiply(int a, int b)
		{
			return a * b;
		}
	}
"@
<#
Add-Type -Path Microsoft.Extensions.Configuration.dll;
Add-Type -TypeDefinition $code;
$result = [MyClass]::Multiply(4, 3);
Write-Host $result;
Read-Host;
#>
Write-Host $env:NUGET_PACKAGES;
Read-Host;