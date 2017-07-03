# Headquarters [![Build status](https://ci.appveyor.com/api/projects/status/vf5m4027llet9l2a?svg=true)](https://ci.appveyor.com/project/QuiCM/headquarters) 
A C# Library for creating and running commands concurrently.


Available on [NuGet](https://www.nuget.org/packages/Headquarters)

## Building

Headquarters targets the .NET Standard 1.5 framework and is built using Visual Studio 2017. It requires:
* NETStandard.Library >= 1.6.1
* Newtonsoft.Json >= 10.0.2
* System.ComponentModel.TypeConverter >= 4.3.0
* System.Threading.Thread >= 4.3.0

All of which are available on NuGet.

You will also need [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/get-started) installed.

To build, run `dotnet restore` followed by `dotnet build` in the base directory.

## Help

You can visit [the Wiki](../../wiki) to view examples, or join [Discord](https://discord.gg/s6xSJFD) for discussion.