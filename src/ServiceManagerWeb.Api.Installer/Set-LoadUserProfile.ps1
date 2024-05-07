Import-Module WebAdministration

Set-ItemProperty "IIS:\AppPools\ServiceManagerUtilsApiAppPool" -Name "processModel.loadUserProfile" -Value "True"