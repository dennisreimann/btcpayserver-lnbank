# LN bank

An extension for [BTCPay Server](https://github.com/btcpayserver) to use the BTCPay Lightning node in custodial mode.
It allows server admins to open up the internal LN node and give users access via custodial layer 3 wallets.

UPDATE: This will become a BTCPay Server plugin. See [this PR](https://github.com/btcpayserver/btcpayserver/pull/2701) for updates. 

## Demo flows

### Internal wallet payment

Here's a simple payment of two LNbank wallets:
One creates and invoice and the other wallet pays it.

![LNbank payment demo](./docs/img/demo-payment.gif)

### BTCPay Server integration

You can use LNbank wallets as individual Lightning Nodes in BTCPay Server.
This way, several merchants can share the internal Lightning Node;
each having their own separate store connected to their individual LNbank wallet.

![LNbank BTCPay Server connection string](./docs/img/btcpay-connection-string.png)

The "Company" wallet is set up as a Lightning node in the BTCPay Server instance on the right.
It creates an invoice for the payment request, which gets paid from the "Personal" wallet.
Works on top of and across all LN implementations in BTCPay Server.

![LNbank BTCPay Server integration demo](./docs/img/demo-btcpay.gif)
