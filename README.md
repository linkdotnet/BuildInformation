# Incremental Build Information Generator
This project provides a simple and easy-to-use C# source generator that embeds build information, such as the build time, platform, warning level, and configuration, directly into your code. By using the `BuildInformation` class, you can quickly access and display these details.

## Features
* Embeds build date (in UTC) in your code
* Embeds platform (AnyCPU, x86, x64, ...) information in your code
* Embeds compiler warning level in your code
* Embeds build configuration (e.g., Debug, Release) in your code

## Usage
To use the `BuildInformation` class in your project, add the NuGet package:

```no-class
dotnet add package LinkDotNet.BuildInformation
```

Here is some code how to use the class:
```csharp
using System;

Console.WriteLine($"Build at: {BuildInformation.BuildAt}");
Console.WriteLine($"Platform: {BuildInformation.Platform}");
Console.WriteLine($"Warning level: {BuildInformation.WarningLevel}");
Console.WriteLine($"Configuration: {BuildInformation.Configuration}");
```

You can also hover over the properties to get the currently held value (xmldoc support). An example output could look like this:
```no-class
Build at: 2023-03-23T12:34:56.7890123Z
Platform: AnyCPU
Warning level: 4
Configuration: Debug
```

## Contributing
If you would like to contribute to the project, please submit a pull request or open an issue on the project's GitHub page. We welcome any feedback, bug reports, or feature requests.

## License
This project is licensed under the MIT License.