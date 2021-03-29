# PiBorgSharp - a .NET Library for the PiBorg
https://www.piborg.org/

[![CodeQL](https://github.com/crazycga/PiBorgSharp/actions/workflows/CodeQLAnalysis.yml/badge.svg)](https://github.com/crazycga/PiBorgSharp/actions/workflows/CodeQLAnalysis.yml)

## Overview
This project is a .NET Core implementation of the control library for the controllers manufactured by PiBorg.  The project
started with the implementation of ThunderSharp, but moved on from there.  Most of the code example provided by PiBorg were
all written in Python, and I personally wanted to move it to a managed language.  I use C# professionally and felt that it
was an appropriate place to put it.  

## Dot Net on Raspberry Pi
The .NET Core 3.1 (at the time of this writing) runtime engine is available from Microsoft.  Installation is done using the steps
located at Microsoft's website: https://docs.microsoft.com/en-us/dotnet/iot/deployment

## NuGet Packages  

### PiBorgSharp.ThunderBorg 
![Nuget](https://img.shields.io/nuget/v/PiBorgSharp.ThunderBorg) ![Nuget](https://img.shields.io/nuget/dt/PiBorgSharp.ThunderBorg) [![ThunderBorg DLLs](https://github.com/crazycga/PiBorgSharp/actions/workflows/DLLThunderSharp.yml/badge.svg)](https://github.com/crazycga/PiBorgSharp/actions/workflows/DLLThunderSharp.yml)

The NuGet package is available with the command 

`Install-Package PiBorgSharp.ThunderBorg`

## Future
This .md is part of the project, and will be updated as time goes on.

## Credits
Special thanks to the following repos and repo owners:
- https://github.com/piborg
- https://github.com/mshmelev/RPi.I2C.Net
- https://github.com/unosquare/raspberryio (and others in their domain)

#### Special thanks to:
- https://github.com/ArronChurchill (an employee of PiBorg and forum moderator)
- https://github.com/mshmelev (provided the original code for the LibNativeI2C project and the C# wrappers)

