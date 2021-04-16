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
Single configuration file need to be provided to the application:
 - *main configuration file* (Resources/ieca_app_configuration.json)

#### Main configuration file
Main configuration file is used for setting desired address for *address claim* feature, 
application *wait period* after address claim procedure, *desired ECU name* used in address claim procedure (check useful links section),
*MQTT Client* configuration and *engine description*.

##### Configuration - Address settings	

| Parameter     | Description                                                  |
| ------------- | ------------------------------------------------------------ |
| WantedAddress | Address Logger will claim in case it's not already taken. If it is taken, it will start iterating for Min to Max address and take first available one. **Min value: 1 Max value: 240** |
| MinAddress    | Iteration start address in case WantedAddress is taken **Min value: 1 Max value: 240** |
| MaxAddress    | Iteration end address in case WantedAddress is taken **Min value: 1 Max value: 240** |



##### Configuration - Address claim wait period settings

This parameter is used to tell the application how long to wait before it starts communication after Address Claim is successful. Per J1939 standard, minimum of 1.25s is required waiting period.



##### Configuration - ECU name settings		

| Parameter                 | Description                                                  |
| ------------------------- | ------------------------------------------------------------ |
| Arbitrary Address Capable | This 1-bit field indicates whether an ECU is self-configurable and can use an arbitrary source address to resolve an address claim conflict. If this bit is set to “1”, the ECU will resolve an address conflict with an ECU whose NAME has a higher priority (lower numeric value) by adopting a new source address. An ECU which computes it’s address and can claim only that particular address is not considered arbitrary address capable (i.e., On-Highway Trailers.) |
| Industry Group            | 3-bit field defined and assigned by the committee. Industry Group definitions may be found in Appendix B.7 of the SAE J1939 base document. The Industry Group field identifies NAMEs associated with a particular industry that uses SAE J1939, for example: On-Highway Equipment or Agricultural Equipment. |
| Vehicle System Instance   | 4-bit field which indicates the occurrence of a particular Vehicle System within a connected network. Note that in the case of single or first Vehicle System of a particular type, the instance field should be set to zero indicating the first instance. |
| Vehicle System            | 7-bit field defined and assigned by the committee, which when combined with the Industry Group can be correlated to a common name. Vehicle System provides a common name for a group of functions within a connected network. Examples of Vehicle Systems for currently defined Industry Groups are “tractor” in the “Common” Industry Group, “Trailer” in the On-Highway Industry Group, and planter in the “Agricultural Equipment” Industry Group. |
| Function                  | 8-bit field defined and assigned by the committee. When Function has a value of 0 to 127, its definition is not dependent on any other field. When Function has a value greater than 127, its definition depends on Vehicle System. Function, when combined with the Industry Group and the Vehicle System fields can be correlated to a common name for specific hardware. The common name formed from the combination does not imply any specific capabilities. |
| Function Instance         | 5-bit field which indicates the particular occurrence of a Function on the same Vehicle System on a given network. Note that in the case of single or first Function of a particular type, the instance field should be set to zero indicating the first instance. Individual manufacturers and integrators are advised that some agreement in the interpretation and use of Function Instances may be necessary. As an example, consider an implementation consisting of two engines and two transmissions. It may be important that engine instance 0 be physically connected to transmission instance 0 and that engine instance 1 be physically connected to transmission instance 1. |
| ECU Instance              | 3-bit field that indicates which one of a group of electronic control modules associated with a given Function is being referenced. For example, in the case where a single engine is managed by two separate control units, each of which is attached to the same SAE J1939 network, the ECU Instance Field will be set to 0 for the first ECU and 1 for the second ECU. Note that in the case of single or first ECUs of a type, the instance fields should be set to zero indicating the first instance. |
| Manufacturer Code         | 11-bit field that indicates which company was responsible for the production of electronic control module for which this NAME is being referenced. Manufacturer codes are assigned by committee and may be found in the SAE J1939 base document. The Manufacturer Code field is not dependent on any other field in the NAME. |
| Identity Number           | 21-bit field in the name assigned by the ECU manufacturer. The Identity Number is necessary in circumstances where it is possible that the NAME would not otherwise be unique (i.e., could be identical). This field should be unique and non-varying with removal of power. This field is necessary to resolve any address contention. It is the manufacturer’s responsibility to provide this uniqueness among his products (for example, through the use of identity number, serial number, time/date code, etc.) |



##### Configuration - MQTT Client settings

Prior to MQTT setup, make sure You read www.u-blox.com/en/blogs/insights/mqtt-beginners-guide
	**SamplingTimeTopic** - MQTT topic IECA application listens to. Contains time in milliseconds which represents how often should application request information from ECU.

​	**EngineDataPayloadTopic** - MQTT topic IECA application will publish to. All interpreted engine data will be in .json format.

​	**ConnectionRetryTimeMilliseconds** - In case IECA application looses connection to MQTT broker, it will retry connecting periodically according to set value.



##### Configuration - Engine Data settings

Engine parameter definitions (strings) that will be included in .json payload.

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
## Usefull links
|        Name         |                          Needed for                          | Link                                                         |
| :-----------------: | :----------------------------------------------------------: | ------------------------------------------------------------ |
|    CAN-utils Git    |                More details about CAN drivers                | https://github.com/linux-can/can-utils                       |
|    .NET on Linux    |       Supported .NET releases and how to install them        | https://docs.microsoft.com/en-us/dotnet/core/install/linux   |
| J1939 address claim | Details about address claim procedure and ECU name parameters | https://copperhilltech.com/blog/sae-j1939-address-claim-procedure-sae-j193981-network-management/ |
