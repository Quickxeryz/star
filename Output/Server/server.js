const hostname = "";
const port = 8085;
const https = require("https");
const fs = require("fs");
const ws = require("ws");
let server = https.createServer({
    cert: fs.readFileSync("Server/cert.pem"),
    key: fs.readFileSync("Server/key.pem")
}, (req, res) => {
    switch (req.url) {
        case "/":
            res.setHeader("Content-Type", "text/html");
            res.writeHead(200);
            res.end(fs.readFileSync("Server/client.html"));
            break;
        case "/client.js":
            res.setHeader("Content-Type", "application/javascript");
            res.writeHead(200);
            res.end(fs.readFileSync("Server/client.js"));
            break;
    }
}).listen(port, hostname);
//console.log("Server started: https://" + hostname + ":" + port);
// Websocket
const wss = new ws.Server({ server, path: '/ws' });
wss.on("connection", function connection(ws) {
    ws.send("connected");
    ws.on("message", (data) => {
        console.log(data.toString());
        //ws.send('Receive: ' + data)
    });
});