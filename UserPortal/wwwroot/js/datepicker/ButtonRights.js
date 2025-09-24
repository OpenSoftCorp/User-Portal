$(document).ready(function () {


    var screenpermission = $("#screenpermission").val();
    var buttonlist = $("#buttonlist").val();

   // alert(screenpermission);

    var btnid = buttonlist.split(",");
    var btnid1 = btnid[0];
    var btnid2 = btnid[1];
    var btnid3 = btnid[2];
    var btnid4 = btnid[3];
    var btnid5 = btnid[4];
    var btnid6 = btnid[5];
    var btnid7 = btnid[6];
    var btnid8 = btnid[7];

    var btnper1 = screenpermission[0];
    var btnper2 = screenpermission[1];
    var btnper3 = screenpermission[2];
    var btnper4 = screenpermission[3];
    var btnper5 = screenpermission[4];
    var btnper6 = screenpermission[5];
    var btnper7 = screenpermission[6];
    var btnper8 = screenpermission[7];

   // debugger;

    if (screenpermission == "00000000") {

        $(".btn").prop("disabled", true);
    }
    if (typeof btnid1 != 'undefined') {
        if (btnper1 == "1") {

            $("#"+btnid1).prop('disabled', false);
            //document.getElementById(btnid1).disabled = false;
        }
        else {
            $("#"+btnid1).prop('disabled', true);
            //document.getElementById(btnid1).disabled = true;
        }
    }

    if (typeof btnid2 != 'undefined') {

        if (btnper2 == "1") {
            $("#"+btnid2).prop('disabled', false);
            //document.getElementById(btnid2).disabled = false;
        }
        else {
            $("#"+btnid2).prop('disabled', true);
           // document.getElementById(btnid2).disabled = true;
        }
    }

    if (typeof btnid3 != 'undefined') {
        if (btnper3 == "1") {
            $("#"+btnid3).prop('disabled', false);
           // document.getElementById(btnid3).disabled = false;
        }
        else {
            $("#"+btnid3).prop('disabled', true);
            //document.getElementById(btnid3).disabled = true;
        }
    }

    if (typeof btnid4 != 'undefined') {
        if (btnper4 == "1") {
            $("#"+btnid4).prop('disabled', false);
            //document.getElementById(btnid4).disabled = false;
        }
        else {
          //  alert(btnper4);
            $("#"+btnid4).prop('disabled', true);
            //document.getElementById(btnid4).disabled = true;
        }
    }

    if (typeof btnid5 != 'undefined') {
        if (btnper5 == "1") {
            $("#"+btnid5).prop('disabled', false);
            //document.getElementById(btnid5).disabled = false;
        }
        else {
            $("#"+btnid5).prop('disabled', true);
           // document.getElementById(btnid5).disabled = true;
        }
    }

    if (typeof btnid6 != 'undefined') {
        if (btnper6 == "1") {
            $("#"+btnid6).prop('disabled', false);
           // document.getElementById(btnid6).disabled = false;
        }
        else {
            $("#"+btnid6).prop('disabled', true);
            //document.getElementById(btnid6).disabled = true;
        }
    }

    if (typeof btnid7 != 'undefined') {
        if (btnper7 == "1") {
            $("#"+btnid7).prop('disabled', false);
           // document.getElementById(btnid7).disabled = false;
        }
        else {
            $("#"+btnid7).prop('disabled', true);
            //document.getElementById(btnid7).disabled = true;
        }
    }
    if (typeof btnid8 != 'undefined') {
        if (btnper8 == "1") {
            $("#"+btnid8).prop('disabled', false);
            //document.getElementById(btnid8).disabled = false;
        }
        else {
            $("#"+btnid8).prop('disabled', true);
            //document.getElementById(btnid8).disabled = true;
        }
    }


    // for highlight button //
    $("button").on('click', function () {
        window.localStorage.setItem('btnid', this.id);
    });

    var btnValue = window.localStorage.getItem("btnid");

    var path = window.location.pathname;
    var pathArray = window.location.pathname.split('/');
    var btnValue = pathArray[2];

    $("#" + btnValue).addClass('btnactive');

    if (btnValue == "undefined") {
        window.onbeforeunload = function () {
            window.localStorage.removeItem("btnid")
        };
    }



});