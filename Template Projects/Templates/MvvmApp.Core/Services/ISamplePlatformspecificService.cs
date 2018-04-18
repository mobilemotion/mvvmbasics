using MVVMbasics.Services;

namespace MvvmApp.Core.Services
{
	/// <summary>
	/// Represents an MVVM Service that is platform-specific, meaning that this interface just provides the service
	/// methods' signatures that need to be implemented in platform-specific service classes (in contrast to a
	/// platform-independent, portable service that includes only method implementations that work on all supported
	/// platforms).
	/// </summary>
	public interface ISamplePlatformspecificService : IService
	{
		//TODO: Add signatures of service methods (to be implemented in platform-specific service classes)
	}
}
