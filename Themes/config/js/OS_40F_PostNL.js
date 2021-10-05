$(document).ready(function () {
    $('#OS_40F_PostNL_cmdSave').unbind("click");
    $('#OS_40F_PostNL_cmdSave').click(function () {
        $('.processing').show();
        $('.actionbuttonwrapper').hide();
        // lower case cmd must match ajax provider ref.
        nbxget('OS_40F_PostNL_savesettings', '.OS_40F_PostNLdata', '.OS_40F_PostNLreturnmsg');
    });

    $('.selectlang').unbind("click");
    $(".selectlang").click(function () {
        $('.editlanguage').hide();
        $('.actionbuttonwrapper').hide();
        $('.processing').show();
        $("#nextlang").val($(this).attr("editlang"));
        // lower case cmd must match ajax provider ref.
        nbxget('OS_40F_PostNL_selectlang', '.OS_40F_PostNLdata', '.OS_40F_PostNLdata');
    });

    $(document).on("nbxgetcompleted", OS_40F_PostNL_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function OS_40F_PostNL_nbxgetCompleted(e) {
        $('.processing').hide();
        $('.actionbuttonwrapper').show();
        $('.editlanguage').show();

        if (e.cmd == 'OS_40F_PostNL_selectlang') {
                        
        }
    };
});

