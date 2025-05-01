# Docker build jappinfo.c
This example shows how to build a static binary within a docker container.

## Source code
First we create jappinfo.c source code.
```c
#include <stdio.h>

int main() {
    printf("Hello, World!\n");
    return 0;
}
```

## Dockerfile
Next we create a Dockerfile.
```Dockerfile
FROM alpine:latest
RUN apk add --no-cache build-base
WORKDIR /app
COPY . /app
RUN gcc -static -o jappinfo jappinfo.c
CMD ["xxd", "jappinfo"]
```

## Build
```sh
docker image build -t jappinfo .
```

## Run
Docker run dumps the xxd output to stdio.
With xdd -r we convert it back to a binary.
```sh
docker run -it --rm jappinfo | xxd -r > jappinfo
chmod +x jappinfo
```

## Execute
Finaly we can execute the binary.
```sh
./jappinfo
Hello, World!
```