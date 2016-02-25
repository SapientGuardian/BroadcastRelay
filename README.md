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
Coming soon!

## Requirements
You must install [Win10Pcap](http://www.win10pcap.org/download/) or [WinPcap](http://www.winpcap.org/install/default.htm) in order to use BroadcastRelay.

## Instructions
Coming soon!

## License
BroadcastRelay is released under the MIT license.