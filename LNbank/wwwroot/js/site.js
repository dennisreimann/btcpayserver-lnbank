"use strict"

// Clipboard
window.copyToClipboard = (e, text) => {
    if (navigator.clipboard) {
        e.preventDefault()
        const item = e.currentTarget
        const data = text || item.getAttribute('data-clipboard')
        const confirm = item.querySelector('[data-clipboard-confirm]') || item
        const message = confirm.getAttribute('data-clipboard-confirm') || 'Copied ✔'
        if (!confirm.dataset.clipboardInitialText) {
            confirm.dataset.clipboardInitialText = confirm.innerText
            confirm.style.minWidth = confirm.getBoundingClientRect().width + 'px'
        }
        navigator.clipboard.writeText(data).then(() => {
            confirm.innerText = message
            setTimeout(() => {
                confirm.innerText = confirm.dataset.clipboardInitialText
            }, 2500)
        })
        item.blur()
    }
}

window.copyUrlToClipboard = e => {
    window.copyToClipboard(e,  window.location)
}

// SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/InvoiceHub")
    .withAutomaticReconnect()
    .build()

connection.on("Message", console.debug)

connection.start()
    .catch(console.error)

window.send = message => {
    connection
        .invoke("SendMessage", message)
        .catch(console.error)
}

document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-clipboard]").forEach(item => {
        item.addEventListener("click", window.copyToClipboard)
    })

    document.querySelectorAll(".btcpay-theme-switch").forEach(link => {
        link.addEventListener("click", e => {
            e.preventDefault()
            const current = document.documentElement.getAttribute(THEME_ATTR) || COLOR_MODES[0]
            const mode = current === COLOR_MODES[0] ? COLOR_MODES[1] : COLOR_MODES[0]
            setColorMode(mode)
        })
    })
})
