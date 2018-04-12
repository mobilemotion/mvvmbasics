==========================================
=== Welcome to the MVVMbasics library! ===
==========================================

This is an MVVM application that targets multiple platforms.


--- Solution architecture ---
The Core project contains Data Models and Viewmodels. The platform-specific projects (DesktopApp, UniversalApp, 
PhoneSilverlightApp) contain Views to be bound to the Viewmodels defined within the Core project.
Services that are implemented platform-independently are contained within the Core project. Services that need to be 
implemented separately for each platform consist of a common Service Interface (within the Core project) and individual 
Service Implementations (within each platform-specific project).


--- Project setup ---
For each target platform you want to support, add one of the "Universal Windows App / WPF Desktop App / Xamarin.Forms
App for MVVMbasics Core project" to the current solution, and ensure that these platform-dependent projects include a
reference to this Core project.