# Japp
Japp is just another package program ;-)  
It uses a container registry to store the packages  

# Install
Japp has a dependency on podman  
Please [install podman](https://podman.io/docs/installation) first  
Then download the [japp binary](https://github.com/bihalu/japp/releases) from github release page
```
sudo apt install -y podman curl

sudo curl -L -o /usr/local/bin/japp https://github.com/bihalu/japp/releases/download/v0.1.0-alpha0/japp

sudo chmod +x /usr/local/bin/japp
```

# Usage
**japp [options] [command]**

## Options

### --help, -h, -?
```
$ japp --help
Description:

     _
    (_)
     _  __ _ _ __  _ __
    | |/ _` | '_ \| '_ \
    | | (_| | |_) | |_) |
    | |\__,_| .__/| .__/
   _/ |     | |   | |    Just another package program ;-)
  |__/      |_|   |_|    Version 0.1.0_alpha0

Usage:
  japp [command] [options]

Options:
  -l, --logging <logging>  Set logging, default is console:information
  --version                Show version information
  -?, -h, --help           Show help and usage information

Commands:
  config          Set config values
  create          Create japp package template
  build           Build japp package
  pull <package>  Pull package from registry
  push <package>  Push package and images to registry
  login           Login to registry
  logout          Logout from registry
```

Get detailed help for e.g. command config
```
$ japp config --help
Description:
  Set config values

Usage:
  japp config [options]

Options:
  -r, --registry <registry>  Set registry
  -t, --temp <temp>          Set temp directory
  -c, --cleanup              Set cleanup
  --tls-verify               Set tls verify
  --reset                    Reset default config
  -?, -h, --help             Show help and usage information
```

### --logging, -l
Set logging output and level seperated with colon  
Default is **console:information**  
Any combination of log output and log level is supported  

| log output  |
| :---------- |
| console     |
| file        |

| log level   |
| :---------- |
| verbose     |
| debug       |
| information |
| warning     |
| error       |
| fatal       |
| off         |

Build japp package and turn logging off
```
$ japp --logging console:off build
```

Pull japp package and log debug to file /tmp/japp/japp.log
```
$ japp --logging file:debug pull japp/example:1.0
```

### --version
```
$ japp --version
0.1.0_alpha0
```

## Commands

### config
Show all config values
```
$ japp config
[12:13:13 INF] Config file: C:\Users\Hansi\.japp\config.json
 {
  "Registry": "192.168.178.59:5000",
  "TempDir": "C:\\Users\\Hansi\\AppData\\Local\\Temp\\japp",
  "Cleanup": false,
  "TlsVerify": false
}
```

Set registry to docker.io
```
$ japp config --registry docker.io
```

Set cleanup to true
```
$ japp config --cleanup true
```

Set temp directory to /tmp/japp
```
$ japp config --temp /tmp/japp
```

Set tls verify to true  
> You can use this option if the certificate for your registry is [trusted](#trusted-certificate)
```
$ japp config --tls-verify true
```

Reset config to default values
```
$ japp config --reset
```

default config values

| name       | value                                            |
| :--------- | :----------------------------------------------- |
| registry   | 192.168.178.59:5000                              |
| cleanup    | false                                            |
| temp       | /tmp/japp (on linux)<br>%TEMP%/japp (on windows) |
| tls-verify | false                                            |

### create
Create japp package template in directory mypackage  
> If you ommit output directory then the files are created in the current directory  
> If you do create again existing files are not overwritten
```
$ japp create --output mypackage
[12:51:34 INF] Create file mypackage\package.yml
[12:51:34 INF] Create file mypackage\logo.png
[12:51:34 INF] Create file mypackage\README.md
```

### build
Build japp package from directory mypackage  
> If you ommit input directory then the package is build in the current directory
```
$ japp build --input mypackage
[13:03:59 INF] Pull container image docker.io/rancher/cowsay:latest
[13:04:02 INF] Build japp package docker.io/japp/example:1.0
```

### pull
Pull japp package japp/example:1.0 to output directory mypackage
> If you ommit output directory then the package is pulled in the current directory  
> The long sub directory name is the unique package id
```
$ japp pull japp/example:1.0 --output mypackage
[13:27:09 INF] Package japp/example:1.0 pulled to mypackage\93d183809f4706f25ee981dc25751a8d17eb976cfea9a9db093e3a66d9fd276a
```

### push
Push japp package to registry  
> Make sure you have built the package before
```
$ japp push japp/mypackage:1.0
[13:33:42 INF] Push japp package 192.168.178.59:5000/japp/mypackage:1.0
```

With the option --retag all container images of the japp package are also pushed
```
$ japp push japp/mypackage:1.0 --retag
[13:35:03 INF] Push japp package 192.168.178.59:5000/japp/mypackage:1.0
[13:35:05 INF] Push container image 192.168.178.59:5000/rancher/cowsay:latest
```

### login
Login to registry  
> necessary if it is a private registry with authentication
```
$ japp login --username admin
Password: ******
[13:13:54 INF] Login Succeeded!
```

### logout
Logout from registry
```
$ japp logout
[13:13:07 INF] Removed login credentials for 192.168.178.59:5000
```

### install
Install japp package
> Make sure you have built or pulled the package before
```
$ japp install japp/example:1.0
[18:11:37 INF] Run 3 tasks in sequence...
[18:11:37 INF] Task (1/3) Test1 - echo Test
[18:11:37 INF] Task (2/3) Test2 - sleep 3
[18:11:40 INF] Task (3/3) Test3 - echo Test
[18:11:40 INF] Done 3 tasks (Returncode: 0, Duration: 00:00:03.0281008)
```

To quickly test a package you can install it directly from the input directory
```
$ japp install japp/example:1.0 --input mypackage
[18:11:05 INF] Run 3 tasks in sequence...
[18:11:05 INF] Task (1/3) Test1 - echo Test
[18:11:05 INF] Task (2/3) Test2 - sleep 3
[18:11:08 INF] Task (3/3) Test3 - echo Test
[18:11:08 INF] Done 3 tasks (Returncode: 0, Duration: 00:00:03.1081523)
```


# Build japp from source
You need .net8.0 sdk and git  
Example for building japp on debian 12
```
# install .net8.0 sdk -> see https://learn.microsoft.com/de-de/dotnet/core/install/linux-debian
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0

# install git
sudo apt install -y git

# clone japp repo
git clone https://github.com/bihalu/japp.git
cd japp

# build
dotnet build
#  Wiederherzustellende Projekte werden ermittelt...
#  Alle Projekte sind für die Wiederherstellung auf dem neuesten Stand.
#  japp.lib -> /root/japp/japp.lib/bin/Debug/net8.0/japp.lib.dll
#  japp.test -> /root/japp/japp.test/bin/Debug/net8.0/japp.test.dll
#  japp.cli -> /root/japp/japp.cli/bin/Debug/net8.0/japp.dll

# Der Buildvorgang wurde erfolgreich ausgeführt.
#    0 Warnung(en)
#    0 Fehler

# Verstrichene Zeit 00:00:02.49

# test
dotnet test
# ...
# Die Testausführung wird gestartet, bitte warten...
# Insgesamt 1 Testdateien stimmten mit dem angegebenen Muster überein.

#Bestanden!   : Fehler:     0, erfolgreich:     7, übersprungen:     0, gesamt:     7, Dauer: 71 ms - japp.test.dll (net8.0)

# publish linux version
dotnet publish --runtime linux-x64 -p:PublishSingleFile=true --self-contained true japp.cli/japp.cli.csproj

# publish windows version
dotnet publish --runtime win-x64 -p:PublishSingleFile=true --self-contained true japp.cli/japp.cli.csproj
```

# Setup local registry
TODO descripe how to setup a local registry

# Trusted certificate
TODO describe how to accept self signed certificate from registry
