$id = get-random
$code = @"
using System;
namespace HelloWorld
{
	public class Program$id
	{
		public static void Main(){
			Console.WriteLine("Hello world!");
		}
	}
}
"@
 
Add-Type -TypeDefinition $code -Language CSharp;
iex "[HelloWorld.Program$id]::Main()";
Read-Host;