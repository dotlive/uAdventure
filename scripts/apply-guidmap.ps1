################################################################################
# Copyright (C) 2018 The e-UCM Learning Group
#
# Licensed under the Apache License, Version 2.0 (the "License"); you may not
# use this file except in compliance with the License.
#
# You may obtain a copy of the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
# WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#
# See the License for the specific language governing permissions and
# limitations under the License.
################################################################################

$project_path = "$($pwd)\BuildProject\uAdventurePackage\uAdventurePackage"
$exchange_folder = "$($pwd)\Exchange"
$guid_file = "$($exchange_folder)\guidmap.csv"
$log_file = "$($exchange_folder)\\unity.log"
$fix_guids_method = "uAdventure.Editor.FileIDUtil.SwitchPrefabsGUIDsToDLL"

$unity = "C:\\Program Files\\Unity\\Editor\\Unity.exe"
$arguments = "-batchmode -force-free -nographics -silent-crashes -logFile $($log_file) -projectPath $($project_path) -quit -executeMethod $($fix_guids_method) $($guid_file)"

Write-Output "Applying GUIDMap to $($project_path)"
$process = Start-Process $unity $arguments -Wait -PassThru

$logContent = Get-Content $log_file
Write-Output "Logs from build: `r`n $($logContent)"

If ( $process.ExitCode -eq 0 ) 
{
    If ( $logContent -contains "*Scripts have compiler errors.*") 
    {
        Write-Error "Scripts have compiler errors!"
        exit 1
    }
    Else 
    {
        Write-Output "GUIDMap applied successfully."
    }
}
Else 
{
    Write-Error "GUIDMap application failed. Exited with $($process.ExitCode)."
    exit $process.ExitCode
}

# Here we should test for missing scripts!
exit 0