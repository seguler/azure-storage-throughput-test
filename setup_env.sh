#!/bin/bash

# install dotnet core
echo "Installing .NET Core"


sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ yakkety main" > /etc/apt/sources.list.d/dotnetdev.list'
apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893
apt-get update

apt-get install dotnet-dev-1.0.1 -y

echo "Cloning the sample"
cd /home
git clone https://github.com/Azure-Samples/storage-blob-coreclr-linux-getting-started-w-data-movement-library

cd storage-blob-coreclr-linux-getting-started-w-data-movement-library
dotnet restore
dotnet build

echo "Creating dummy files from dev/urandom"
mkdir test
cd test
dd if=/dev/urandom of=samplefile bs=1G count=2

echo "Resetting the permissions"
cd ..
chmod -R 755 .

echo "done"




