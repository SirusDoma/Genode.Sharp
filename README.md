# GENODE - Game EngiNe On DEmand #
- **Author**: SirusDoma
- **Email**: com@cxo2.me
- **Latest Version**: 1.0

Although the name of project has "Game Engine" on it, the library itself is actually only a sets of multimedia API which is fast, cross-platform and most important, it is **simple**.  

The API Structure and Implementation mostly taken from [SFML](https://github.com/SFML/SFML), but it is **NOT** 1 : 1 SFML Implementation and it's written purely in C#.

--------------------------------------------------------------------------------------------------

## About the Project ##
The idea of this project is to provides an OOP approach and simple access to OpenGL and OpenAL API as well as flexibility of implementation of the target application. Initially, I try to create a simple framework. However, it seems I'm overdoing it by *accidentally* implementing **most** of SFML API into C# (luckily, it is not a waste!)

So now, I name this project as **GENODE** - Game EngiNe On DEmand; This name even not originally made by me, it was taken from a similar project that created by someone who I admire.  
In hope to make me stop reinventing the wheel and stay focus to the game itself!!!

--------------------------------------------------------------------------------------------------

## Structure ##
At the first glance, the project structure similar to SFML, but as being said before, it is not 1 : 1 SFML Implementation.  
Some codes, structures and logics are changed to fit C# language, OpenTK (OpenGL binding for C#) and my satisfication. The (known) differences as follows:
- This framework doesn't provide networking module.
- Audio module provide lesser amount of API than SFML provides. By default, it is support WAV and OGG only. Also, there is no Audio Writer and Capture.
- Added some interfaces and additional classes to aid multiple class inheritances as C# doesn't support it.
- OpenTK take care most of OpenGL Context operations.
- Some / most part of API may or may not make no sense at all!

--------------------------------------------------------------------------------------------------

## Compiling the Project ##
By default, the project target framework is targeted to .NET 2.0 to ensure maximum backward compatibility against old hardware and / or old projects targeted to old framework.  

It is may required to configure the Build Configuration Platform (`x86`/`x64`) of target application to match the library configuration. Avoid using `Any CPU` platform, because this framework uses native external dependency (e.g: the engine may fail when deciding which version of `openal32.dll` to use).  

This project also make a use of [Fody Costura](https://github.com/Fody/Costura/) to embed native dependencies on Windows. 
It may not work against Mono Environment (Linux / OSX) so it is required to configure Fody.Costura to exclude all dependencies or uninstall the Fody.Costura itself before compiling the project on Linux or OSX.  

To enable Fody.Costura to embed all native dependencies, the `.dll` files must be placed in `Source/Genode/Resources/`, depending the build version, `x86` may placed inside `Source/Genode/Resources/costura32` while `x64` in `Source/Genode/Resources/costura64`

--------------------------------------------------------------------------------------------------

## Dependencies ##
This library uses several dependencies to perform specific operations.
The dependencies are separated into 2 types: Internal and External:

- External dependencies are included under `Dependencies` folder. If Fody.Costura is uninstalled or external dependencies are excluded, these dependencies must be installed or shipped along with the application and located under same folder with the main of application. These dependencies may installed by default in certain Operating System. This folder also used to store Nuget packages.
- Internal dependencies are compiled along with this library during compilation, the source code is located under `Source\Genode\Dependencies`

List of dependencies:
- [OpenTK](https://github.com/opentk/opentk)
- [SharpFont](https://github.com/Robmaister/SharpFont)
- [NVorbis](https://github.com/ioctlLR/NVorbis)

--------------------------------------------------------------------------------------------------

## License ##
This is an open-sourced library licensed under the [MIT license](http://github.com/SirusDoma/Genode/blob/master/LICENSE).
