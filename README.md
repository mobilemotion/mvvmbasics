# Building Apps for more than one platform with no additional effort!

MVVMbasics is a .NET library that provides App developers with a framework to develop Apps for multiple target platforms without writing redundant code. If you've got a great idea for implementing a Windows 8 App - why not publish the same App also for Windows Phone, or as a small Desktop Tool to be used on conventional personal computers? With MVVMbasics you don't even need to write additional code besides creating the basic user interface for the additional target platforms. In addition, even if you don't want to release Apps for all the platforms right now, you can still add additional target platforms later on without the need of converting the whole codebase to different APIs.

The MVVMbasics framework builds upon the well-known MVVM design pattern and the .NET Portable Class Library technology, however it standardizes the differences in implementations for the Windows Desktop (WPF), Windows 8 Store Apps, and Windows Phone platforms, allowing to write code that seamlessly interacts with all those platforms.

The framework consists of the MVVMbasics core library and platform-specific Extension Packages that act as wrappers to the different platform-specific APIs and interfaces. Apps based on MVVMbasics may target all platforms for which an Extension Package is available. At the moment, this includes the following platforms:
* Windows Store Apps for Windows 8.1 and higher
* Windows Phone 8.1 and higher
* Windows Phone Silverlight 8.0
* Android (via Xamarin.Forms)
* iOS (via Xamarin.Forms)
* WPF Desktop Applications

However, MVVMbasics is an interesting option even for single-platform projects, since it is a very light-weight extension library that offeres standardized implementations of Data Binding through the INotifyPropertyChanged interface as well as Command Binding, and therefore helps reducing the necessity of writing redundant code for each newly created App project.

## New features in Version 2.2.6
* Support for Visual Studio 2015 and C# 6
* Correct back navigation in Store Apps (via Visual Studio project templates)

## New features in Version 2.2.5
* Correct resolution of nested property desclarations in Commands’ CanExecute conditions
* Automatic binding of Commands to CanExecute conditions when using MvvmCommandAutobinding attribute

## New features in Version 2.2.3
* Passing custom parameters during event-to-command redirection
* Simple built-in service container
* Support for 3rd party IoC containers

## New features in Version 2.2.0
* Full Xamarin.Forms support
* Attribute-based bindable property declaration

## New features in Version 2.1
* Smart UI dispatcher
* Common way of injecting a ServiceLocator instance into both Viewmodels and Services
* Bugfix in the visual XAML editor
* Correct back navigation on Windows Phone 8.1 back button press (via Visual Studio project templates)

## New features in Version 2.0
* Support for Universal Apps (Windows 8.1 Store Apps, Windows Phone 8.1)
* Simplified package structure
* Simpler & more flexible Data Binding
* Simplified Command binding
* Shorter declaration of navigation parameters
* Event to Command Binding
* New _Hidden_ and _Shown_ view states

## Resources
For detailed information about the usage of the MVVMbasics framework, visit the product pages at **[http://mvvmbasics.mobilemotion.eu](http://mvvmbasics.mobilemotion.eu)**:

**[→ Version History](http://www.mobilemotion.eu/?page_id=1142&amp;lang=en)**

**[→ Documentation & Tutorials](http://www.mobilemotion.eu/?page_id=739&amp;lang=en)**

**[→ Project Templates](http://www.mobilemotion.eu/?page_id=747&amp;lang=en)**

**[→ Class Reference](http://mvvmbasics.mobilemotion.eu/reference/)**

**[→ Download](http://www.mobilemotion.eu/?page_id=751&amp;lang=en)**
