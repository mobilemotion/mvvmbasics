﻿using MVVMbasics.Attributes;

namespace MvvmApp.Core.Services
{
	/// <summary>
	/// Represents an MVVM Service that is platform-independent, meaning that the implementations of the various service
	/// methods work on all supported platforms (in contrast to a platform-specific service that needs specific
	/// implementations for each supported platform).
	/// </summary>
	[MvvmService]
    public class SamplePortableService : ISamplePortableService
    {
	    //TODO: Add service methods
    }
}
