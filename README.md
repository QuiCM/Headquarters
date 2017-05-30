# Headquarters
A C# Library for creating and running commands concurrently.


## Basics

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
      
      registry.HandleInput("example", context); //handle the input 'example', which will invoke the 'example' command
    }
  }
}
```

#### Receiving command output
When you ask the library to handle an input, you are provided with a unique ID.
When the library finishes handling an input, it invokes the `OnInputResult` event. The event data contains the result of the parsing (`InputResult` enum), the unique ID tied to the input, and the output from the input.
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
  static int handleId;
  
  public static void Main(string[] args)
  {
    IContextObject context = null; //null is used in this example
  
    using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
    {
      registry.AddCommand(typeof(ExampleCommand), new RegexString[] { "example" }, "An example command!");
      registry.OnInputResult += Registry_OnInputResult;
      
      handleId = registry.HandleInput("example", context); //handle the input 'example', which will invoke the 'example' command
    }
  }
  
  private static void Registry_OnInputResult(object sender, InputResultEventArgs e)
  {
    if (e.ID == handleId)
    {
      Console.WriteLine($"ID: {e.ID} - Result: {e.Result} - Output: {e.Output}"); //ID: 1 - Result: Success - Output: Hello world!
    }
  }
}
```

