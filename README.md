![](https://github.com/ibosnic00/IndustrialEcuCommunicationApplication/blob/main/IndustrialEcuCommunicationApplication/Resources/IECA.png?raw=true)

# Industrial Ecu Communication Application
![Workflow](https://github.com/ibosnic00/IndustrialEcuCommunicationApplication/workflows/.NET%20Unit%20Tests/badge.svg)
***
*This is .NET CLI application which is based on can-utils's ```can-dump``` and ```can-send```.* 
___
## Instructions
  Two different setups can be used when publishing:
  - *self contained*
  - *framework dependant*

#### Prerequisites
Before we start we need to satisfy some prerequisites. Install **can-utils** and validate instalation:
```sh
sudo apt-get install can-utils
```

**How to validate requirements:**
Open 2 separate terminals and execute commands:
*1st terminal*
```sh
$ candump vcan0
```
*2nd terminal*
```sh
$ cansend vcan0 1F334455#1122334455667788
```
Sent CAN message should appear in first terminal!
***
## Configuring application
Two different configuration files need to be provided to the application:
	- *main configuration file*
	- *data parser configuration file*
	
#### Main configuration file
Main configuration file is used for setting desired address for *address claim* feature, path to *data parser configuration file*, 
application *wait period* after address claim procedure, *desired ECU name* used in address claim procedure (check usefull links section),
and PGN's which will be sent every N miliseconds (both PGN and period are configurable).
	
#### Data parser configuration file
This configuration file is used for converting J1939 messages to Human-Readable format.
For example J1939 message 
```sh
0x18CFFF00 0x01 0x55 0x12 0xAA 0x02 0x55 0x0A 0xEE	
```
will be saved in log file in format:
```sh
Continuous Torque & Speed Limit Request 
Minimum Continuous Engine Speed Limit Request: 32 
Maximum Continuous Engine Speed Limit Request: 2720
```
Configuration provided in this repository contains all publicly available PGN's and SPN's (refer to J1939-71 document), so it is recomended to use
it as base configuration, and add only some manufacturer-specific PGN's if necessary.
***
## Starting application	
Application is started with 2 required arguments from list below:
	
| Short argument | Long argument |                         Description                          |
| :------------: | :-----------: | :----------------------------------------------------------: |
|       -c       | --canChannel  | CAN channel which will be used. Only one allowed from list: can0, can1, can2, can3 |
|       -f       | --appCfgFile  | Path to app configuration file (.json) which will be parsed. |

Example
```sh
$ ./IECA -c can0 -f cfg.json
```
or	
```sh
$ ./IECA --canChannel can0 --appCfgFile /home/desktop/cfg.json
```
***
## Log files
Log files are created in ./logs folder (relative to app executing location), and named in format
    ***hh*-*mm* _ ieca _*YYYYMMDD*.log**
In case log exceeds 5 MB size, new log file will be auto-created.
***
## Usefull links
|        Name         |                          Needed for                          | Link                                                         |
| :-----------------: | :----------------------------------------------------------: | ------------------------------------------------------------ |
|    CAN-utils Git    |                More details about CAN drivers                | https://github.com/linux-can/can-utils                       |
|    .NET on Linux    |       Supported .NET releases and how to install them        | https://docs.microsoft.com/en-us/dotnet/core/install/linux   |
| J1939 address claim | Details about address claim procedure and ECU name parameters | https://copperhilltech.com/blog/sae-j1939-address-claim-procedure-sae-j193981-network-management/ |

|      |      |      |
| ------ | ------ | ------ |
|      |      |      |
|      |      |      |
|      |      |      |