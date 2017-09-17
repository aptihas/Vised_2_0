var audio_context;
var recorder;
var seconds_timer_id;
var publicBlobObj;

function startUserMedia(stream) {
    var input = audio_context.createMediaStreamSource(stream);
    recorder = new Recorder(input);
}

function startRecording() {
    recorder && recorder.record();
    var rec = document.getElementById("rec");
    var stp = document.getElementById("stp");
    rec.style = "display:none;width:30px;";
    stp.style = "display:inherit;width:30px;";
    timer();
}

function stopRecording() {
    recorder && recorder.stop();
    var rec = document.getElementById("rec");
    var stp = document.getElementById("stp");
    rec.style = "display:inherit;width:30px;";
    stp.style = "display:none;width:30px;";
    // create WAV download link using audio data blob
    createDownloadLink();
    clearInterval(seconds_timer_id);

    recorder.clear();
}

function timer() {

    var seconds = 60;
    seconds_timer_id = setInterval(function () {
        if (seconds > 0 && seconds < 61) {
            var secontText;
            seconds--;
            if (seconds < 10) {
                secontText = "00:0" + seconds;
            }
            else {
                secontText = "00:" + seconds;
            }
            $(".seconds").text(secontText);
        } else {
            stopRecording();
        }
    }, 1000);
}

function createDownloadLink() {
    recorder && recorder.exportWAV(function (blob) {
        var urlr = URL.createObjectURL(blob);
        var audio = document.getElementById("sound");
        audio.src = urlr;
        audio.controls = true;
        publicBlobObj = blob;
    });
}

function sendAudio() {
    var req = new Request('Sound', { method: 'POST', body: publicBlobObj });
    fetch(req).then(
        () => console.log('Файл сохранен'),
        () => console.log('Ошибка сохранения')
    );
}



window.onload = function init() {
    try {
        // webkit shim
        window.AudioContext = window.AudioContext || window.webkitAudioContext;
        navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia;
        window.URL = window.URL || window.webkitURL;

        audio_context = new AudioContext;
    } catch (e) {
        alert('No web audio support in this browser!');
    }
};

navigator.getUserMedia({ audio: true }, startUserMedia, function (e) {
    __log('No live audio input: ' + e);
});