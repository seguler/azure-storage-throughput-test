#!/bin/bash

# install dotnet core
echo "Installing .NET Core"


sudo sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ yakkety main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-key -y adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893
sudo apt-get -y update

sudo apt-get -y install dotnet-dev-1.0.1

echo "Cloning the sample"
git clone https://github.com/Azure-Samples/storage-blob-coreclr-linux-getting-started-w-data-movement-library

cd storage-blob-coreclr-linux-getting-started-w-data-movement-library
dotnet restore
dotnet build





