# StarDustTerminal
a spiritual successor of the Open-Term terminal for cosmos. stardust is a graphical terminal that has support for command suggestion, coloring, ANSI, Images and much more. all integrated with the standard Console class of cosmos

## Usage:

to create a new terminal just:

```csharp
var term = new Terminal(font,canvas,width,height);
```

you can add commands to the terminal like this:

```csharp
term.Commands.Add("echo", (string[] args) =>
{
    string buffer = "";
    foreach (var item in args)
    {
        buffer += item + " ";
    }
    buffer = buffer.Remove(0, args[0].Length + 1);
    Console.WriteLine(buffer);
});
```
