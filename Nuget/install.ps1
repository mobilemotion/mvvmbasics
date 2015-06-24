param($installPath, $toolsPath, $package, $project)

#***************************************************************************************************
# Most of the MVVMbasics components can be added automatically by positioning them within Nuget's
# framework-specific folders within the /lib/ folder. However, this doesn't work for the following
# project types:
# - Xamarin.Forms iOS: Libraries positioned within Nuget's /lib/monotouch/ folder are not copied to
#                      Xamarin.Forms iOS project, therefore we need to add a reference to the
#                      MVVMbasicsXF library manually
# - Xamarin.Forms PCL: Portable Class Libraries targeting Profile 78 can be either "standard" PCLs
#                      or Xamarin.Forms content libraries - if the latter is the case (which can be
#                      detected by checking if the project references Xamarin..Forms.Code), we need
#                      to add a reference to the MVVMbasicsXF library manually
# - Xamarin.Forms WinPhone: WinPhone projects can either target Windows Phone Silverlight or
#                           Xamarin.Forms - if the latter is the case (which can be detected by
#                           checking if the project references Xamarin..Forms.Code), we need to
#                           remove the MVVMbasicsPS library and add a reference to the MVVMbasicsXF
#                           library manually
#***************************************************************************************************

$mvvmBasicsXFPath = ("{0}\lib\MVVMbasicsXF\MVVMbasicsXF.dll" -f $installPath)

$vsproject = $project.Object
$references = $vsproject.References
$count = 0
$xamarinReferences = $references | Where-Object "Name" -eq "Xamarin.Forms.Core" | Foreach-Object {$count++} # Measure-Object doesn't work in PASH (used by Xamarin Studio Mac)
$projectType = $project.Properties | Where-Object "Name" -eq "TargetFrameworkMoniker" | Select-Object "Value"

If ($count -ge 1)
{
	If ($projectType.Value.StartsWith("WindowsPhone,"))
	{
		# Xamarin.Forms WinPhone Project: Remove MVVMbasicsPS reference and add MVVMbasicsXF reference

		Write-Host ("{0} is a Xamarin.Forms WinPhone project..." -f $project.Name)

		$mvvmBasicsPSReference = $references | Where-Object "Name" -eq "MVVMbasicsPS" | Select-Object -First 1
		If ($mvvmBasicsPSReference -ne $null)
		{
			Write-Host "...removing reference to MVVMbasicsPS"

			$mvvmBasicsPSReference.Remove()
		}

		Write-Host ("...adding reference to MVVMbasicsXF from path {0}" -f $mvvmBasicsXFPath)

		$references.Add($mvvmBasicsXFPath)
	}
	If ($projectType.Value.StartsWith(".NETPortable,"))
	{
		# Xamarin.Forms PCL Project: Add MVVMbasicsXF reference

		Write-Host ("{0} is a Xamarin.Forms PCL project..." -f $project.Name)
		Write-Host ("...adding reference to MVVMbasicsXF from path {0}" -f $mvvmBasicsXFPath)

		$references.Add($mvvmBasicsXFPath)
	}
}