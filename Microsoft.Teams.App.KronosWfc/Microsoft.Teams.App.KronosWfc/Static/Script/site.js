var microsoftTeams;

$(document).ready(function () {
    microsoftTeams.initialize();
});

function submitActivity() {
    var user = $('#signinform').serializeArray()[0].value;
    var pass = $('#signinform').serializeArray()[1].value;
    var obj = {
        UserName: user,
        Password: pass
    };
    microsoftTeams.authentication.notifySuccess(JSON.stringify(obj));

}