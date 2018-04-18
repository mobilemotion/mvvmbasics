==========================================
=== Welcome to the MVVMbasics library! ===
==========================================

This is an MVVM application that targets multiple platforms.


--- Solution architecture ---
The Core project contains Data Models and Viewmodels. The platform-specific projects (WPF, UWP, Xamarin.Forms) contain
Views to be bound to the Viewmodels defined within the Core project.
Services that are implemented platform-independently are contained within the Core project. Services that need to be 
implemented separately for each platform consist of a common Service Interface (within the Core project) and individual 
Service Implementations (within each platform-specific project).


--- Project setup ---
Remove those projects from the solution that represent platforms you don't want to support (or those projects that are
not supported by your Visual Studio installation), and adjust namespaces, assembly names and app manifests to fit your
needs!