## About
**This is still in an INDEV, PRE-RELEASE stage. Numerous features are unfinished and it is far from being fully featured.**

RichNote is a basic tabbed note editor. Not only is it capable of standard text editing, but it also supports most basic rich text formatting. Plus, it also has some advanced features like autosaving. RichNote utilizes WinUI 3 to leverage fluent design.

I decided to create RichNote due to my experiences with the Windows 11 Notepad. I loved how much of an improvement it is over the old one, but being a Windows 10 user myself, I can't use it without weird workarounds. So, I decided to write my own program with similar functionality and more.

### Current features
* Basic UI
* Tab controls
* Separate TXT/RTF editors

### To-do features
* File handling (opening and saving)
* Autosaving
* Text formatting
* Status bar
* Settings

## Building
1. Verify you have all the requirements installed.

2. Install Visual Studio 2022 with the "WinUI application development" and ".NET desktop development" workloads.

3. Clone the GitHub repository. 

4. This is an unpackaged app, so make sure the configuration is set to "RichNote (Unpackaged)" before building.
![](https://i.postimg.cc/9XxMv6ND/Screenshot-9.png)

## Contributing
Want to contribute? Pull requests and issues are always open. Fork according to the license.

### Requirements
* [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [Windows App SDK 1.7.2](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)

### Libraries used
* UDE.CSharp (planned)
* Json.NET (planned)
