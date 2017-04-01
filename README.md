# azuredeploytest

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fseguler%2Fazuredeploytest%2Fmaster%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>

1. Click Deploy button to deploy the sample to a Linux VM on Azure
2. Once deployment is completed, connect to the VM via SSH using the administrator account you defined
3. `cd /home/<administrator>/azuredeploytest`
4. Run the application to upload the files to Azure Storage from 'test' folder
`dotnet run test`
