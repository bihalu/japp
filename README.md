# Japp
Japp is just another package program ;-)  
It uses a container registry to store the packages  

# Install
Japp has a dependency on podman  
Please install podman first -> https://podman.io/docs/installation  

Then download binary from github release page -> https://github.com/bihalu/japp/releases  
```

```

# Usage
General usage **japp [options] [command]**

## Options

### --help, -h, -?
Show japp help  
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
  |__/      |_|   |_|    Version 1.0.0+51b299c33801ec9c84159d138fd34aee4b58a33e

Usage:
  japp [command] [options]

Options:
  -l, --logging <logging>  Set logging, default is console:information
  --version                Show version information
  -?, -h, --help           Show help and usage information

Commands:
  config          Set config values
  create          Create package template
  pull <package>  Pull package from registry
```

Get detailed help for e.g. command config **japp config --help**
```
$ japp config --help
Description:
  Set config values

Usage:
  japp config [options]

Options:
  -r, --registry <registry>  Set registry
  -t, --temp <temp>          Set temp dircevtory
  -c, --cleanup              Set cleanup
  --tls-verify               Set tls verify
  --reset                    Reset default config
  -?, -h, --help             Show help and usage information
```

### --version
Show japp version

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


## Commands

### config

### create

### build

### push

### pull

