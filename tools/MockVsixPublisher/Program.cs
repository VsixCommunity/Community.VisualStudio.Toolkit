using System;

Console.Out.WriteLine("MockVsixPublisher.exe");
Console.Out.WriteLine("---------------------");

// Write each command line flag and its corresponding argument on the same line.
int index = 0;
while (index < args.Length)
{
    string currentArg = args[index];
    string? nextArg = (index + 1) < args.Length ? args[index + 1] : null;

    // If this argument is a flag and the next argument
    // is not a flag, then write them both on the same line.
    if (currentArg.StartsWith('-') && (nextArg is not null) && !nextArg.StartsWith('-'))
    {
        Console.Out.WriteLine($"{currentArg} {nextArg}");
        index += 2;
    }
    else
    {
        Console.Out.WriteLine(currentArg);
        index += 1;
    }
}

// Write the error message to stderr if there is a
// message specified via the environment variable.
string? error = Environment.GetEnvironmentVariable("VSIX_PUBLISHER_ERROR");
if (!string.IsNullOrEmpty(error))
{
    Console.Error.WriteLine(error);
}

// Get the exit code from an environment variable. This allows a failure to be mocked.
if (!int.TryParse(Environment.GetEnvironmentVariable("VSIX_PUBLISHER_EXIT_CODE"), out int exitCode))
{
    // This isn't strictly required, because `exitCode` will be set to the default
    // value if parsing fails, but without doing this, the compiler produces
    // a message stating that the return value of `TryParse` was not checked.
    exitCode = 0;
}

return exitCode;
