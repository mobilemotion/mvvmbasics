using MvvmApp.Core.Services;
using MVVMbasics.Attributes;

namespace MvvmApp.WPF.Services
{
	/// <summary>
	/// Represents an MVVM Service that is platform-specific, meaning that this class implements a portable service
	/// interface and all the service methods defined in this interface (in contrast to a platform-independent, portable
	/// service that includes only method implementations that work on all supported platforms).
	/// </summary>
	[MvvmService]
	public class SamplePlatformspecificService : ISamplePlatformspecificService
	{
		//TODO: Add implementations of service methods (as defined in the portable service interface)
	}
}
