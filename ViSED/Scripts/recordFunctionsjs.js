
var recorder;
var publicBlobObj;

function createAudioElement(blob) {
    var aud = document.getElementById("sound");
    aud.controls = true;
    aud.src = blob;
    aud.type = 'audio/webm';


    //const downloadEl = document.createElement('a');
    //downloadEl.style = 'display: block';
    //downloadEl.innerHTML = 'download';
    //downloadEl.download = 'audio.webm';
    //downloadEl.href = blobUrl;
    //const audioEl = document.createElement('audio');
    //audioEl.controls = true;
    //const sourceEl = document.createElement('source');
    //sourceEl.src = blobUrl;
    //sourceEl.type = 'audio/webm';
    //audioEl.appendChild(sourceEl);
    //document.body.appendChild(audioEl);
    //document.body.appendChild(downloadEl);
}


function start() {
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
            if (recorder.state == 'inactive') {
                // convert stream data chunks to a 'webm' audio format as a blob
                const blob = new Blob(chunks, { type: 'audio/webm' });
                // convert blob to URL so it can be assigned to a audio src attribute
                createAudioElement(URL.createObjectURL(blob));
                publicBlobObj = blob;
            }
        };
        // start recording with 1 second time between receiving 'ondataavailable' events
        recorder.start();
        // setTimeout to stop recording after 4 seconds
    }).catch(console.error);
}

function stop() {
    recorder.stop();
    var data = new FormData;
    data.append("sound", publicBlobObj);
}


//рабочий вариант
function upload() {
    var xhr = new XMLHttpRequest();
    var data = new FormData;
    data.append("sound", publicBlobObj);
    xhr.open('POST', 'DocSave');
    // Listen to the upload progress.
    xhr.send(data);
}
