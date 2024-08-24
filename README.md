# Japp
Japp is just another package program ;-)  
It uses a container registry to store the packages  

# Install
Japp has a dependency on podman  
Please [install podman](https://podman.io/docs/installation) first  
Then download the [japp binary](https://github.com/bihalu/japp/releases) from github release page
```
apt install -y podman


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

### --version
```
$ japp --version
0.1.0_alpha0
```

### --logging, -l
Set logging output and level seperated with colon  
Default is **console:information**  
Any combination of log output and log level is supported  

| log output  |
| ----------- |
| console     |
| file        |

| log level   |
| ----------- |
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
| ---------- | ------------------------------------------------ |
| registry   | 192.168.178.59:5000                              |
| cleanup    | false                                            |
| temp       | /tmp/japp (on linux)<br>%TEMP%/japp (on windows) |
| tls-verify | false                                            |

### create
Create japp package template in directory mypackage  
> If you ommit output directory then the files are created in the current directory
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

### push
Push japp package to registry  
> Make sure you have built the package before
```
$ japp push japp/example:1.0
Unhandled exception: System.NotImplementedException: The method or operation is not implemented.
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

# Trusted certificate
TODO describe how to accept self signed certificate from registry
