const ip = "192.168.178.44";
const port = "8085";

function setPlayerName() {
    let name = nameInput.value.trim();
    if (name == "") {
        output.innerText = "Playername can't be empty!";
    } else {
        playerName = name;
        output.innerText = "Playername set to " + playerName;
    }
}

// websocket
function connect() {
    serverButton.innerText = "Reconnect to server";
    output.innerText = "Connected";
    ws = new WebSocket("wss://" + ip + ":" + port + "/ws");
    ws.onmessage = msg => {
        //data = msg.data;
        ws.send(playerName + ":None");
    };
    ws.onclose = e => {
        output.innerText = e.reason;
    };
};

// mic input
class Microphone {
    constructor() {
        navigator.mediaDevices.getUserMedia({ audio: true })
            .then(function (stream) {
                this.audioContext = new AudioContext();
                this.microphone = this.audioContext.createMediaStreamSource(stream);
                this.analyser = this.audioContext.createAnalyser();
                this.dataArray = new Float32Array(this.analyser.fftSize);
                this.microphone.connect(this.analyser);
            }.bind(this))
            .catch(function (err) {
                alert("Microphone access is necassary for the website to function correctly!");
            });
    }
}

function autoCorrelate(buffer, sampleRate) {
    // Perform a quick root-mean-square to see if we have enough signal
    var SIZE = buffer.length;
    var sumOfSquares = 0;
    for (var i = 0; i < SIZE; i++) {
        var val = buffer[i];
        sumOfSquares += val * val;
    }
    var rootMeanSquare = Math.sqrt(sumOfSquares / SIZE)
    if (rootMeanSquare < 0.01) {
        return -1;
    }
    // Find a range in the buffer where the values are below a given threshold.
    var r1 = 0;
    var r2 = SIZE - 1;
    var threshold = 0.2;
    // Walk up for r1
    for (var i = 0; i < SIZE / 2; i++) {
        if (Math.abs(buffer[i]) < threshold) {
            r1 = i;
            break;
        }
    }
    // Walk down for r2
    for (var i = 1; i < SIZE / 2; i++) {
        if (Math.abs(buffer[SIZE - i]) < threshold) {
            r2 = SIZE - i;
            break;
        }
    }
    // Trim the buffer to these ranges and update SIZE.
    buffer = buffer.slice(r1, r2);
    SIZE = buffer.length
    // Create a new array of the sums of offsets to do the autocorrelation
    var c = new Array(SIZE).fill(0);
    // For each potential offset, calculate the sum of each buffer value times its offset value
    for (let i = 0; i < SIZE; i++) {
        for (let j = 0; j < SIZE - i; j++) {
            c[i] = c[i] + buffer[j] * buffer[j + i]
        }
    }
    // Find the last index where that value is greater than the next one (the dip)
    var d = 0;
    while (c[d] > c[d + 1]) {
        d++;
    }
    // Iterate from that index through the end and find the maximum sum
    var maxValue = -1;
    var maxIndex = -1;
    for (var i = d; i < SIZE; i++) {
        if (c[i] > maxValue) {
            maxValue = c[i];
            maxIndex = i;
        }
    }
    var T0 = maxIndex;
    var x1 = c[T0 - 1];
    var x2 = c[T0];
    var x3 = c[T0 + 1]
    var a = (x1 + x3 - 2 * x2) / 2;
    var b = (x3 - x1) / 2
    if (a) {
        T0 = T0 - b / (2 * a);
    }
    return sampleRate / T0;
}

function record() {
    if (!isRecording) {
        isRecording = true;
        recordButton.innerText = "Stop record";
        recordButton.style.background = "green";
        interval = setInterval(function () { // delay for user interaktion
            // find the frequency peak
            microphone.analyser.getFloatTimeDomainData(microphone.dataArray);
            let hz = autoCorrelate(microphone.dataArray, microphone.audioContext.sampleRate)
            // Handle rounding
            let node = "";
            if (hz === -1 || hz == 0) {
                node = "None";
            } else {
                let nodeNumber = Math.round((Math.log(hz / 440) / Math.log(2)) * 12 + 49);
                nodeNumber = nodeNumber - 4;
                while (nodeNumber < 0) {
                    nodeNumber += 12;
                }
                nodeNumber = nodeNumber % 12;
                switch (nodeNumber) {
                    case 0:
                        node = "C";
                        break;
                    case 1:
                        node = "CH";
                        break;
                    case 2:
                        node = "D";
                        break;
                    case 3:
                        node = "DH";
                        break;
                    case 4:
                        node = "E";
                        break;
                    case 5:
                        node = "F";
                        break;
                    case 6:
                        node = "FH";
                        break;
                    case 7:
                        node = "G";
                        break;
                    case 8:
                        node = "GH";
                        break;
                    case 9:
                        node = "A";
                        break;
                    case 10:
                        node = "AH";
                        break;
                    case 11:
                        node = "B";
                        break;
                    default:
                        node = "None";
                        break;
                }
            }
            output.innerText = node;
            ws.send(playerName + ":" + node);
        }, 100);
    } else {
        clearInterval(interval); // stops reading interval
        recordButton.innerText = "Record";
        recordButton.style.background = "red";
        isRecording = false;
    }
}

function requestMic() {
    microphone = new Microphone();
}

let ws;
let output;
let serverButton;
let nameInput;
let recordButton;
window.onload = function () {
    output = document.getElementById("output");
    nameInput = document.getElementById("nameInput");
    serverButton = document.getElementById("serverButton");
    recordButton = document.getElementById("record");
}
let microphone = new Microphone();
let isRecording = false;
let interval; // for reading mic
let playerName = "";
