
// modal load
function showLoading() {
    $('.mymodal').addClass('show');
}

function hideLoading() {
    $('.mymodal').removeClass('show');
}


// display items
var displaySymbols = function () {
    showLoading();
    $.ajax({
        url: "/Home/_Items?fontId=" + $('#font_id').html(),
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

displaySymbols();




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
function feedback() {
    $('#modal_feedback').modal('hide');
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