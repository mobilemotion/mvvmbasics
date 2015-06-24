param($installPath, $toolsPath, $package, $project)

#***************************************************************************************************
# Since references to the MVVMbasicsXF library are added manually by the install.ps1 script, we need
# to remove them manually when uninstalling the MVVMbasics framework
#***************************************************************************************************

$vsproject = $project.Object
$references = $vsproject.References
$mvvmBasicsXFReference = $references | Where-Object "Name" -eq "MVVMbasicsXF" | Select-Object -First 1
If ($mvvmBasicsXFReference -ne $null)
{
    Write-Host ("Removing reference to MVVMbasicsXF from project '{0}'" -f $project.Name)

    $mvvmBasicsXFReference.Remove()
}