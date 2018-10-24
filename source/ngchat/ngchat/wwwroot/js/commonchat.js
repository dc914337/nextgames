"use strict";
var ping_timeout = 5000;
var connection = new signalR.HubConnectionBuilder().withUrl("/commonChatHub").build();


function showNextMessage(username, message) {
    $(".messages-container").append(
        $("<div>").attr("class", "message-block")
            .append($("<div>").attr("class", "username").text(username))
            .append($("<p>").attr("class", "message").text(message)));
}

function showOnlineUsers(usernames) {
    $("#online-users").empty();
    console.log("Showed " + usernames.length + " users");
    $.each(usernames, function (i, val) {
        $("#online-users").append(
            $("<div>").attr("class", "username").text(val)
        );
    });
    
}

connection.on("ReceiveMessage", function (user, message) {
    showNextMessage(user, message);
    $('html, body').scrollTop($(document).height());
});

connection.on("ReceiveOnlineList", function (users) {
    showOnlineUsers(users);
});

$('#sendButton').click(function (event) {
    var message = $("#messageInput").val();
    connection.invoke("SendMessage", message).catch(function (err) {
        return console.error(err.toString());
    });
    $("#messageInput").val(null);
    event.preventDefault();
});

connection.start().then(function () {
    var MS_PER_DAY = 60 * 24 * 60000;
    var historyFrom = new Date(new Date() - MS_PER_DAY);
    connection.invoke("GetMessageHistory", historyFrom).then(function (historyMsgs) {
        console.log(historyMsgs);
        $.each(historyMsgs, function (i, val) { showNextMessage(val.username, val.message) });
    }
    ).catch(err => console.error(err.toString()));

    //ping(); //no need to ping, we just connected, server knows that we are active
    setInterval(ping, ping_timeout);
    
}).catch(function (err) {
    return console.error(err.toString());
});



function ping() {
    console.log("ping");
    connection.invoke("Ping").catch(function (err) {
        return console.error(err.toString());
    });
}