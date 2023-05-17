# installation
1. install nodejs
2. to run the server you need a certificates for https. This can be generated with the following steps:
    1. install openssl
    2. generate a certificate (this can done with the command: openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "key.pem" -out "cert.pem" -subj "/")
    3. put the "key.pem" and "cert.pem" in the "Server" folder
3. you need to put the server-ip into the client.js and server.js in the server folder. (you can check for the ip with ipconfig)

# connect to server
- connect to the server with https://ip:port
- be sure that all web devices are on the same network as the machine running the game