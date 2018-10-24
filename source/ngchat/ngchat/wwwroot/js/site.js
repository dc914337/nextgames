// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
"use strict";
var PING_TIMEOUT = 2000;
var LOAD_HISTORY_MS = 60 * 24 * 60000; //how much history to load declared in MS (currently one day of history is loaded)

var connection = new signalR.HubConnectionBuilder().withUrl("/commonChatHub").build();
connection.start().then(function () {
    var historyFrom = new Date(new Date() - LOAD_HISTORY_MS);
    connection.invoke("GetMessageHistory", historyFrom).then(function (historyMsgs) {
        $.each(historyMsgs, function (i, val) { showNextMessage(val.username, val.message) });
    }
    ).catch(err => console.error(err.toString()));
    setInterval(ping, PING_TIMEOUT);
}).catch(function (err) {
    return console.error(err.toString());
});



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

function ping() {
    console.log("ping");
    connection.invoke("Ping").catch(function (err) {
        return console.error(err.toString());
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





