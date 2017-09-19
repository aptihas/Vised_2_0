function createAudioElement(blob) {
    var aud = document.getElementById("sound");
    aud.controls = true;
    aud.src = blob;
    aud.type = 'audio/webm';
}

var recorder;

function start() {
    var rec = document.getElementById("rec");
    var stp = document.getElementById("stp");
    var delRec = document.getElementById("delRec");
    delRec.hidden = true;
    rec.hidden = true;
    stp.hidden = false;
    rec.className = "enableFlase";
    stp.className = "enableTrue";
    navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
        // store streaming data chunks in array
        const chunks = [];
        // create media recorder instance to initialize recording
        recorder = new MediaRecorder(stream);
        // function to be called when data is received
        recorder.ondataavailable = e => {
            // add stream data to chunks
            chunks.push(e.data);
            // if recorder is 'inactive' then recording has finished
            if (recorder.state === 'inactive') {
                // convert stream data chunks to a 'webm' audio format as a blob
                var blob = new Blob(chunks, { type: 'audio/webm' });
                // convert blob to URL so it can be assigned to a audio src attribute
                createAudioElement(URL.createObjectURL(blob));
                var xhr = new XMLHttpRequest();
                var data = new FormData;
                data.append("sound", blob);
                xhr.open('POST', '/User/SoundTemp', true);
                xhr.send(data);
            }
        };
        // start recording with 1 second time between receiving 'ondataavailable' events
        recorder.start();
        // setTimeout to stop recording after 4 seconds
    }).catch(console.error);
}

function stop() {
    var rec = document.getElementById("rec");
    var stp = document.getElementById("stp");
    var delRec = document.getElementById("delRec");
    rec.className = "enableTrue";
    stp.className = "enableFlase";
    delRec.className = "enableTrue";
    delRec.hidden = false;
    rec.hidden = false;
    stp.hidden = true;
    recorder.stop();
}

function recordDel() {
    var xhr = new XMLHttpRequest();
    xhr.open('POST', '/User/SoundTempDel', true);
    xhr.send("");
    var delRec = document.getElementById("delRec");
    delRec.hidden = true;
    var aud = document.getElementById("sound");
    aud.controls = true;
    aud.src = null;
}