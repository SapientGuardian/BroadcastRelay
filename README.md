# BroadcastRelay
## Description
BroadcastRelay solves the problem of broadcast traffic needing to cross subnets by detecting and forwarding broadcast messages generated locally to specific addresses.

## Motivation
Many applications, largely games, use broadcast traffic to locate peers. 
While this is fine for simple network topologies, such as two computers connected by a home router, 
it is insufficient for more complex scenarios in which traffic must cross subnets, such as a VPN. 

## How it works
The Broadcast Relay uses [LibPacketGremlin](https://github.com/SapientGuardian/LibPacketGremlin) and [WinPcap](http://www.winpcap.org) to operate in a method 
similar to a firewall, inspecting network traffic and pulling out packets which meet specific criteria.
These packets are then modified to route to the configured addresses and retransmitted.

## Where to get it
Download [Broadcast Relay 1.0.0.0](https://github.com/SapientGuardian/BroadcastRelay/releases/download/1.0.0.0/BroadcastRelay_1.0.0.0.zip)

## Requirements
You must install [.NET Framework 4.6.1](https://www.microsoft.com/en-us/download/details.aspx?id=49981), as well as [Win10Pcap](http://www.win10pcap.org/download/) or [WinPcap](http://www.winpcap.org/install/default.htm) in order to use BroadcastRelay.

## Instructions
The first time you start Broadcast Relay, or any time you start it without any listening network adapters or destinations set, it will open a window. Once you have selected
an adapter on which to listen and defined at least one destination to relay, subsequent starts will go directly to the system tray.

1. Ensure you have met the [requirements](https://github.com/SapientGuardian/BroadcastRelay#requirements)
1. Select at least one adapter on the Listening Adapters tab. This selection chooses the adapters where broadcast packets should be captured.
If you only have one adapter, it will be selected automatically. There probably isn't any harm in selecting all of your adapters, but you have the ability
to be selective. If you aren't sure, what to select, you should probably select all of them.
1. Move to the Destinations tab. Here, you can define destinations where broadcast packets will be sent. If using VPN software like Hamachi, you would the
IP address of one or more of your peers here. If you're on a network with multiple subnets, this would be the IP address of the computer you're attempting to
communicate with. Add a peer by clicking the "Add" button in the lower right corner of the window. Enter the IP address in the empty text box of the newly
added row. Click the "Lock" button to lock in your selection, or the "X" button to delete it.
1. Activate the application that generates broadcast traffic. You should see the "Packets Relayed" count in the status bar increase. You can leave the window open,
or you can minimize it to the system tray. The application must be running in order to relay packets.

## License
BroadcastRelay is released under the MIT license.