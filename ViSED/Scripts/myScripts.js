function getFileName() {
    var file = document.getElementById('uploaded-file').value;
    file = file.replace(/\\/g, "/").split('/').pop();
    document.getElementById('file-name').innerHTML = 'Имя файла: ' + file;
}

function backFist() {
    var poluchateli = document.getElementById("poluchateli");
    poluchateli.style = "display:inherit;";
    var text = document.getElementById("text");
    text.style = "display:none;";
}
function backSecond() {
    var text = document.getElementById("text");
    text.style = "display:inherit;";
    var files = document.getElementById("files");
    files.style = "display:none;";
}

function poluchateli() {
    var poluchateli = document.getElementById("poluchateli");
    poluchateli.style = "display:none;";
    var text = document.getElementById("text");
    text.style = "display:inherit;";
}

function text() {
    var text = document.getElementById("text");
    text.style = "display:none;";
    var files = document.getElementById("files");
    files.style = "display:inherit;";
}

function letterType() {
    var rdo = document.getElementById("taskType");
    var deadline = document.getElementById("deadline");

    if (rdo.value === "1")
    {
        deadline.style = "display:inherit;";
    }
    else
    {
        deadline.style = "display:none;";
    }
}

function spisokVibor() {
    var vibor = document.getElementById("spisokpouchateleySelect");
    var spis = document.getElementById("spisokpouchateley");

    if (vibor.value === "1") {
        spis.style = "display:none;";
    }
    else {
        spis.style = "display:inherit;";
    }
}