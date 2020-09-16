"use strict";

// Theme Switch
const COLOR_MODES = ["light", "dark"];
const THEME_ATTR = "data-btcpay-theme";
const STORE_ATTR = "btcpay-theme";
const systemColorMode = window.matchMedia("(prefers-color-scheme: dark)").matches ? COLOR_MODES[1] : COLOR_MODES[0];
const userColorMode = window.localStorage.getItem(STORE_ATTR);
const initialColorMode = COLOR_MODES.includes(userColorMode) ? userColorMode : systemColorMode;

function setColorMode (mode) {
    if (COLOR_MODES.includes(mode)) {
        window.localStorage.setItem(STORE_ATTR, mode);
        document.documentElement.setAttribute(THEME_ATTR, mode);
    }
};

setColorMode(initialColorMode);

document.querySelectorAll(".btcpay-theme-switch").forEach(function (link) {
    link.addEventListener("click", function (e) {
        e.preventDefault();
        const current = document.documentElement.getAttribute(THEME_ATTR) || COLOR_MODES[0];
        const mode = current === COLOR_MODES[0] ? COLOR_MODES[1] : COLOR_MODES[0];
        setColorMode(mode);
    });
});

// SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/InvoiceHub")
    .withAutomaticReconnect()
    .build();

connection.on("Message", function (message) {
    console.log("SignalR message", message);
});

connection.start().then(function () {
    console.log("SignalR connected");
}).catch(function (err) {
    return console.error(err.toString());
});

window.send = function(message) {
    connection.invoke("SendMessage", message).catch(function (err) {
        return console.error(err.toString());
    });
};
