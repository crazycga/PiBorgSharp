# PiBorgSharp - a .NET Library for the PiBorg
https://www.piborg.org/

[![CodeQL](https://github.com/crazycga/PiBorgSharp/actions/workflows/CodeQLAnalysis.yml/badge.svg)](https://github.com/crazycga/PiBorgSharp/actions/workflows/CodeQLAnalysis.yml)

## Overview
This project is a .NET Core implementation of the control library for the controllers manufactured by PiBorg.  The project
started with the implementation of ThunderSharp, but moved on from there.  Most of the code example provided by PiBorg were
all written in Python, and I personally wanted to move it to a managed language.  I use C# professionally and felt that it
was an appropriate place to put it.  

### Deviations From PiBorg Python Methods
A few of the deviations from the Python methods written by the PiBorg team are very intentional:

1. **Command settings in byte format:** this is a conscious choice on my part to separate the idea of jamming traffic down the bus as bytes, rather than looking at the code as integers.  I feel it makes it more manageable.
2. **Various commands in byte format as opposed to decimal format:** in the original code, the motor settings and LED settings, for example, were done as decimals where 0.0 < n < 1.0.  My objection to this is arithmetic: 255 / 100 = 2.55, meaning for every 0.1 change, the code was changing the speed of the motor by 2.55%.  This didn't seem right to me.

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

