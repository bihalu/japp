FROM alpine:latest
RUN apk add --no-cache build-base
WORKDIR /app
COPY . /app
RUN gcc -static -o jappinfo jappinfo.c && strip jappinfo
CMD ["xxd", "jappinfo"]