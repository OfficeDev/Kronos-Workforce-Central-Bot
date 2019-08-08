var microsoftTeams;

$(document).ready(function () {
    microsoftTeams.initialize();
    microsoftTeams.getContext(function (context) {
        // Set CSS class for theme
        let theme = context.theme;
        if (theme && (theme === "dark" || theme === "contrast")) {
            let bodyElement = document.getElementsByTagName("body")[0];
            bodyElement.className += "theme-" + theme;
        }
        schedule(context.tid);
    });
});

function schedule(tid) {

    $.ajax({
        url: "/Login/ScheduleTab?tenant=" + tid,
        cache: false,
        dataType: "json",
        type: "GET",
        success: function (res) {
            if (res.success) {
                console.log(res.success);
                window.location.replace(res.success);
            }
        },
        error: function (res) {

        }
    });
}