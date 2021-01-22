![](https://github.com/ibosnic00/IndustrialEcuCommunicationApplication/blob/main/IndustrialEcuCommunicationApplication/Resources/IECA.png?raw=true)

# Industrial Ecu Communication Application
![Workflow](https://github.com/ibosnic00/IndustrialEcuCommunicationApplication/workflows/.NET%20Unit%20Tests/badge.svg)

*This is .NET CLI application which is based on can-utils's ```can-dump``` and ```can-send```.* 

## Instructions
  Two different setups can be used when publishing:
  - *self contained*
  - *framework dependant*
 
#### Prerequisites
Before we start we need to satisfy some prerequisites. Install **can-utils** and validate instalation:
```sh
sudo apt-get install can-utils
```

**How to validate:**
Open 2 separate terminals and execute commands:
*1st terminal*
```sh
$ candump vcan0
``` 
*2nd terminal*
```sh
$ cansend 1F334455#1122334455667788
``` 
Sent CAN message should appear in first terminal!

![](https://miro.medium.com/max/375/1*ElNfg92I1Zn0RNoSQS9wPw.gif)


## Usefull links
| Name | Needed for | Link |
| ------ | ------ | ------ |
| CAN-utils Git | More details about CAN drivers | https://github.com/linux-can/can-utils |
| .NET on Linux | Supported .NET releases and how to install them | https://docs.microsoft.com/en-us/dotnet/core/install/linux |