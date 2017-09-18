
function createAudioElement(blob) {
    var aud = document.getElementById("sound");
    aud.controls = true;
    aud.src = blob;
    aud.type = 'audio/webm';
}

var recorder;

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
    recorder.stop();
}

//рабочий вариант
//function upload() {
//    var xhr = new XMLHttpRequest();
//    

//    var user_to_id = document.getElementsByName("user_to_id");
//    var myDocs = document.getElementsByName("myDocs");
//    var text = document.getElementsByName("text");
//    var doc_id = document.getElementsByName("doc_id");
//    var attachment = document.getElementsByName("attachment");
//    var taskType = document.getElementById("taskType");
//    var deadline = document.getElementById("deadline");

//    data.append("user_to_id", user_to_id);
//    data.append("myDocs", myDocs);
//    data.append("text", text);
//    data.append("doc_id", doc_id);
//    data.append("attachment", attachment);
//    data.append("taskType", taskType);
//    data.append("deadline", deadline);
//    data.append("sound", publicBlobObj);

//    xhr.open('POST', '/User/Sound', true);
//    // Listen to the upload progress.
//    xhr.send(data);

//    //var fromaa = document.getElementById("forma");
//    //fromaa.submit();
//}