
$groupName = "cannawatch-group"

$group = (az group create --location canadacentral --name $groupName)
[<35;15;22M]
$appServiceName = "cannawatch"
$appServiceProperties = @{
	
}
#az resource create --name $appServiceName --resource-group $groupName --resource-type 
