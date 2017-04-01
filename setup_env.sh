#!/bin/bash

# install dotnet core
echo "###Installing .NET Core"


sudo sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ yakkety main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893
sudo apt-get update

sudo apt-get install dotnet-dev-1.0.1 -y

echo "###Cloning the sample"
cd /home/$1
git clone https://github.com/seguler/azuredeploytest

cd azuredeploytest

# Inject the account name and key
sed -i '/string connectionString/c\string connectionString = "DefaultEndpointsProtocol=http;AccountName='$2';AccountKey='$3'";' Program.cs
	
chown -R $1 .
chmod -R 755 .

echo "###Restoring the nuget packages and building"
sudo -u $1 dotnet restore --configfile /home/$1/.nuget/NuGet/NuGet.Config
sudo -u $1 dotnet build

echo "Creating dummy files from dev/urandom"
mkdir test
cd test
for i in {1..8}
do
	head -c 1G </dev/urandom >mysamplefile.${i}
done

echo "###Resetting the permissions"
cd ..
chown -R $1 .

echo "###done"
