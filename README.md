# Apple Fitness Workout Mapper

[![Build status](https://github.com/martincostello/apple-fitness-workout-mapper/workflows/build/badge.svg?branch=main&event=push)](https://github.com/martincostello/apple-fitness-workout-mapper/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush)

## Overview

A .NET web application for visualising workouts from Apple Fitness in a map.

![AppleFitnessWorkoutMapper](./docs/images/app-screenshot.png "AppleFitnessWorkoutMapper")

### Getting started

To get started with the application to run it for your own workout data, check
out the [Getting started guide](https://github.com/martincostello/apple-fitness-workout-mapper/blob/main/docs/getting-started.md "Getting started").

### Help

If you are having issues with the application, check out the
[help guide](https://github.com/martincostello/apple-fitness-workout-mapper/blob/main/docs/help.md#help "Help").

## Feedback

Any feedback or issues can be added to the issues for this project in
[GitHub](https://github.com/martincostello/apple-fitness-workout-mapper/issues).

## Repository

The repository is hosted in [GitHub](https://github.com/martincostello/apple-fitness-workout-mapper): [https://github.com/martincostello/apple-fitness-workout-mapper.git](https://github.com/martincostello/apple-fitness-workout-mapper.git)

## License

This project is licensed under the
[Apache 2.0](https://github.com/martincostello/apple-fitness-workout-mapper/blob/main/LICENSE) license.

## Building and Testing

### Prerequisites

To be able to build and test the application on your own computer, you will need
the software listed below. You can also optionally install additional software
to help with local development if you would like to customise it or explore how
it works.

#### Required

1. [.NET SDK](https://dotnet.microsoft.com/download)
1. [Git](https://git-scm.com/downloads)
1. [node.js](https://nodejs.org/en/download/)

#### Optional

1. [Visual Studio Code](https://code.visualstudio.com/download)
1. [PowerShell](https://github.com/PowerShell/PowerShell#get-powershell)
1. [Google Chrome](https://www.google.com/chrome/) (to run the UI tests)

To build, publish and test the application locally, run the following command
after cloning the repository if you have PowerShell installed:

```powershell
./build.ps1
```

Otherwise run the following command to build and test the application:

```sh
dotnet test
```
