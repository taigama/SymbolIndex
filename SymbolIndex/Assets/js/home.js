// change font
function changeFont(event, id) {
    event.preventDefault;
    $('#font_current_name').html('loading...');
    $.ajax({
        url: "/Home/_FontStyle?fontId=" + id,
        type: "GET",
        dataType: 'html',
        traditional: true,
        success: function (data) {
            $('#font_style').replaceWith(data);
            changeFontCanvas();
        },
        error: function (err) {
            showError(err);
            alert(err.statusText);
        }
    });
    displaySymbols(id);
    $('#key').val('');
}

// ----------- show info-dialog -------------
var dialog = $('#dialog-item');
var canvasDom = $('#canvas-symbol')[0];
var canvas = canvasDom.getContext('2d');
var posDialog, xDialog, yDialog;
var widthDialog = dialog.width();
var heightDialog = dialog.height();
var item, rectItem;
var widthView, heightView;
var color;
var callbackLoaded;

function calculatePositionDialog(item) {
    calculateViewSize();

    rectItem = item[0].parentElement.parentElement.getBoundingClientRect();
    xDialog = rectItem.right;
    if ((xDialog + widthDialog) > widthView) {
        xDialog = rectItem.left - widthDialog - 22;
    }
    yDialog = rectItem.top;
    if ((yDialog + heightDialog) > heightView) {
        yDialog = rectItem.bottom - heightDialog - 51;
    }
    return [xDialog, yDialog];
}

function drawSymbolToDialog(item) {
    changeFontCanvas();// may be the font was not loaded, make sure the browser loaded the font
    canvas.clearRect(0, 0, 300, 300);
    canvas.fillStyle = color;
    canvas.fillText(item.html(), 150, 160);
}

function initColor() {
    color = getCookie('color');
    if (!color) {
        color = '#F00';
    }
    $('#color-picker').val(color.replace('#', ''));
}

function onChangeColor() {
    color = '#' + $('#color-picker').val();
    drawSymbolToDialog(item);
    setCookie("color", color, 30);
}
$('#color-picker').change(onChangeColor);

// show the symbol-info dialog, for downloading img or copying character
function showDialog(id, event) {
    event.stopPropagation();
    item = $('#' + id);
    posDialog = calculatePositionDialog(item);
    dialog.css({ top: posDialog[1], left: posDialog[0] });

    drawSymbolToDialog(item);
    $('#dialog-tags').html('Tags: ' + $('#' + id + '_tags').html());

    dialog.addClass('show');
    linkImage();
}

function closeDialog() {
    dialog.removeClass('show');
}

function calculateViewSize() {
    widthView = $(window).width();
    heightView = $(window).height();
}

// change the font of the canvas, happend when you changed font
function changeFontCanvas() {
    canvas.font = '250px "' + $('#font_name').html() + '"';
}
canvasDom.width = 300;
canvasDom.height = 300;
canvas.textAlign = "center";
canvas.textBaseline = "middle";
changeFontCanvas();


// ------------ copy ----------------
function copyText() {
    var dt = new clipboard.DT();
    dt.setData("text/plain", item.html());
    clipboard.write(dt);
    showSnackbar();
}

// ------------ copy ----------------
function copyTextWithFormat() {
    var dt = new clipboard.DT();
    dt.setData("text/plain", item.html());
    var data = '<div style="font-family:' + "'" + $('#font_name').text() + "'" +
        ";color:#" + $('#color-picker').val() +
        '"><font size="8" face="' +
        $('#font_name').text() + '">' +
        he.encode(item.text()) +
        '</font></div>';
    dt.setData("text/html", data);
    clipboard.write(dt);
    showSnackbar();
}


var link = $('.link-img');
var downloadImg;
//stackoverflow.com/questions/11620698/how-to-trigger-a-file-download-when-clicking-an-html-button-or-javascript
function linkImage() {
    link.unbind("click");
    link.click(downloadImg);
}

//stackoverflow.com/questions/16245767/creating-a-blob-from-a-base64-string-in-javascript
function b64toBlob(b64Data, contentType, sliceSize) {
    contentType = contentType || '';
    sliceSize = sliceSize || 512;

    var byteCharacters = atob(b64Data);
    var byteArrays = [];

    for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
        var slice = byteCharacters.slice(offset, offset + sliceSize);

        var byteNumbers = new Array(slice.length);
        for (var i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }

        var byteArray = new Uint8Array(byteNumbers);

        byteArrays.push(byteArray);
    }

    var blob = new Blob(byteArrays, { type: contentType });
    return blob;
}

//stackoverflow.com/questions/45197097/cant-save-canvas-as-image-on-edge-browser
downloadImg = function () {   

    var nameFile = $('#dialog-tags').html().replace(/\s\s+/g, '').replace('Tags:', '').replace(/ /g, '');
    if (!nameFile) {
        nameFile = 'no-tag-symbol';
    }
    nameFile += '_' + $('#font_current_name').html().replace(/ /g, '-') + '.png';

    canvasDom.isDrawingMode = false;
    if (!window.localStorage) { alert("This function is not supported by your browser."); return; }

    var blob = new Blob([b64toBlob(canvasDom.toDataURL('png').replace(/^data:image\/(png|jpg);base64,/, ""), "image/png")], { type: "image/png" });
    saveAs(blob, nameFile);
    //canvasDom.isDrawingMode = true;
}


// ----------------------------------------------

// modal load
function showLoading() {
    closeDialog();
    $('.mymodal-outer').addClass('show');
    $('.mymodal').addClass('show');
}

function hideLoading() {
    $('.mymodal-outer').removeClass('show');
    $('.mymodal').removeClass('show');
}


// display items
var displaySymbols = function (id) {
    showLoading();
    $.ajax({
        url: "/Home/_Items?fontId=" + id,
        type: "GET",
        dataType: 'html',
        traditional: true,
        success: function (data) {
            $('#items').html(data);
            $('#font_current_name').html($('#font_name').html());
            hideLoading();
        },
        error: function (err) {
            showError(err);
            alert(err.statusText);
            hideLoading();
        }
    });
}
displaySymbols($('#font_id').html());


//  search
function onKeypressSearch(e) {
    var keynum;

    if (window.event) { // IE
        keynum = e.keyCode;
    } else if (e.which) { // Netscape/Firefox/Opera
        keynum = e.which;
    }

    // phím enter
    if (keynum == 13) {
        onSearch();
    }
}

// just like displaySymbols
function onSearch() {
    showLoading();

    var id = $('#font_id').html();
    var key = $('#key').val();

    $.ajax({
        url: "/Home/_Items?fontId=" + id + "&key=" + key,
        type: "GET",
        dataType: 'html',
        traditional: true,
        success: function (data) {
            $('#items').html(data);
            hideLoading();
        },
        error: function (err) {
            showError(err);
            alert(err.statusText);
            hideLoading();
        }
    });
}

// snack bar

// Get the snackbar DIV
var snackbar = document.getElementById("snackbar");
var timeout;

var hideSnackbar = function() {
    snackbar.className = snackbar.className.replace("show", "");
}
function showSnackbar() {
    // check if he has already had the class
    if (snackbar.classList.contains("show")) {
        clearTimeout(timeout);
    }
    else {
        // Add the "show" class to DIV
        snackbar.className = "show";
    }
    // After 3 seconds, remove the show class from DIV
    timeout = setTimeout(hideSnackbar, 3000);
}


// feedback
function openFeedback(event) {
    event.preventDefault;
    $('#modal-feedback').modal('show');
}

function feedback() {
    $('#modal-feedback').modal('hide');
    var content = getFeedback();

    $.ajax({
        url: '/Home/Feedback/',
        type: "GET",
        data: { 'content': content },
        dataType: 'json',
        success: function (result) {
            alert(result.text);
        },
        error: function (err) {
            showError(err);
            alert(err.statusText);
        }
    });
}

function getDataRadio() {

    var radios = document.getElementsByName('feed_func_item');
    for (var i = 0, length = radios.length; i < length; i++) {
        if (radios[i].checked) {
            if (radios[i].value == "Khác") {
                return $('#feed_func_orther').val();
            }
            else {
                return radios[i].value;
            }
        }
    }

    return "";
}

function focusRadio(index) {
    var radios = document.getElementsByName('feed_func_item');
    if (index >= radios.length)
        return;
    radios[index].checked = true;
}

function getFeedback() {
    var str = "";

    var name = $('#feed_name').val();
    var email = $('#feed_email').val();
    if (name || email) {
        if (name) {
            str += '- Name: ' + name + '\n';
        }
        if (email) {
            str += '- Email: ' + email + '\n';
        }
        str += '\n';
    }


    var color_navi = $('#feed_color_navi').val();
    var color_body = $('#feed_color_body').val();
    var color_item = $('#feed_color_item').val();
    if (color_navi || color_body || color_item) {
        str += '- Color------\n';
        if (color_navi) {
            str += '--- navigate bar: ' + color_navi + '\n';
        }
        if (color_body) {
            str += '--- body: ' + color_body + '\n';
        }
        if (color_item) {
            str += '--- item: ' + color_item + '\n';
        }
        str += '\n';
    }

    var radio = getDataRadio();
    if (radio) {
        str += '- Function------\n' +
            '--- item click: ' + radio + '\n\n';
    }

    if ($('#feed_overall').val()) {
        str += '- Overall------\n' + $('#feed_overall').val();
    }

    return str;
}

// show error
function showError(err) {
    var win = window.open("", "MsgWindow", "width=1366,height=768");
    win.document.write(err.responseText);
    win.focus();
}

// welcome dialog
function closeWelcome() {
    callbackLoaded = undefined;

    var popup_welcome = $('.popup-welcome');
    if (popup_welcome) {
        popup_welcome.remove();
    }

    setCookie("first_view", 1, 30);
}

function checkWelcome() {
    var cookie = getCookie("first_view");
    if (cookie) {
        closeWelcome();
        return;
    }

    $('.nav.nav-tabs>.active>a').click();
}

// about
function showAbout(event) {
    event.preventDefault();
    setCookie("first_view", 1, -1);
    window.location = "/Home/About"
}

// cookies
// I dont remember where I get these cookies function. If you are the author of these scripts, please feedback and send me the link you wrote this function (stackoverflow). I will include your link here (and more info if you want)
function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}
function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

initColor();
checkWelcome();