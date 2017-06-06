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

## Basic Use

#### Creating a command
Creating a command requires a class with the `[CommandClass]` attribute, and one method with a `[CommandExecutor]` attribute and a first parameter that inherits `IContextObject`:

```
[CommandClass]
public class ExampleCommand
{
  [CommandExecutor]
  public object Execute(IContextObject context)
  {
    return "Hello world!";
  }
}
```

#### Registering a command
Once you have a command created, you can register it with the library so that it may be used.
The command registering method takes 3 parameters: the command's type, an `IEnumerable<RegexString>` containing command aliases, and a description string:
```
public class ExampleApplication
{
  public static void Main(string[] args)
  {
    using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
    {
      registry.AddCommand(typeof(ExampleCommand), new RegexString[] { "example" }, "An example command!");
    }
  }
}
```

#### Running a command
With a command created and registered, you can send input into the library to be parsed.
Input can be sent in with a context object that will be passed to the command. If no context is required, `null` is acceptable:
```
public class ExampleApplication
{
  public static void Main(string[] args)
  {
    IContextObject context = null; //null is used in this example
  
    using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
    {
      registry.AddCommand(typeof(ExampleCommand), new RegexString[] { "example" }, "An example command!");
      
      registry.HandleInput("example", context, CommandCallback); //handle the input 'example', which will invoke the 'example' command
    }
  }
}
```

#### Receiving command output
When you ask the library to handle an input, you also provide a callback method.
When the library finishes handling an input, it invokes the callback, providing an InputResult enum detailing the result of the processing, and an object returned by the command.
```
[CommandClass]
public class ExampleCommand
{
  [CommandExecutor]
  public object Execute(IContextObject context)
  {
    return "Hello world!";
  }
}

public class ExampleApplication
{
  public static void Main(string[] args)
  {
    IContextObject context = null; //null is used in this example
  
    using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
    {
      registry.AddCommand(typeof(ExampleCommand), new RegexString[] { "example" }, "An example command!");
      
      registry.HandleInput("example", context, CommandCallback); //handle the input 'example', which will invoke the 'example' command
    }
  }

  private static void CommandCallback(InputResult result, object output)
  {
      Console.WriteLine($"Result: {result} - Output: {output}"); //Result: Success - Output: Hello world!
  }
}
```
