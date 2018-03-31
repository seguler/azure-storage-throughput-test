#!/bin/bash

echo "###Installing .NET Core"

curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-xenial-prod xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get update

sudo apt-get install dotnet-sdk-2.1.4 -y

echo "###Cloning the sample"
cd /home/$1
git clone https://github.com/seguler/azuredeploytest
cd azuredeploytest

echo "###Inject the account name and key"
sed -i '/string connectionString/c\string connectionString = "DefaultEndpointsProtocol=http;AccountName='$2';AccountKey='$3'";' Program.cs
	
chown -R $1 .
chmod -R 755 .

echo "###Restoring the nuget packages and building"
sudo -u $1 dotnet restore --configfile /home/$1/.nuget/NuGet/NuGet.Config
sudo -u $1 dotnet build

echo "###Creating 100 files each 1GB from dev/urandom"
cd /mnt
for i in $(seq 1 100)
do
	head -c 1G </dev/urandom >mysamplefile.${i}
done

echo "###Resetting the permissions"
cd /home/$1/azuredeploytest
chown -R $1 .

echo "###done"
