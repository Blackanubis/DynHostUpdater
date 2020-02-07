# DynHostUpdater
A simple application allowing to automatically update at regular intervals the DynHost of my OVH domain.

## Using this code ##
This is a C# Visual Studio project, so you'll need something that can handle Visual Studio projects, which you can get for free from https://www.visualstudio.com.

It is written for the Windows Presentation Foundation (WPF), targetting .net Core 3.1, MyToolkit and Newtonsoft.Json library.

## Using the application ##
1. Configure the parameters
2. Press the Save button

"TimeToRefresh": in seconds, refresh time example: 60

"HostAdress": name of your DNS example: mysite.com

"UrlUpdater": address to call in case of IP change add the parameter {0} to integrate the public IPI example: www.ovh.com/nic/update?system=dyndns&hostname=mysite.com&myip={0}

"Login": your login (optional)

"Password": your password (optional)

