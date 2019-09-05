using Microsoft.Xna.Framework;


namespace Nez
{
	public static class GameServiceContainerExt
	{
		/// <summary>
		/// Adds the service and returns the added service for method chaining
		/// </summary>
		/// <returns>The service.</returns>
		/// <param name="self">Self.</param>
		/// <param name="provider">Provider.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T AddService<T>(this GameServiceContainer self, T provider)
		{
			self.AddService(typeof(T), provider);
			return provider;
		}

		/// <summary>
		/// Gets the service
		/// </summary>
		/// <returns>The service.</returns>
		/// <param name="self">Self.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetService<T>(this GameServiceContainer self) where T : class
		{
			var service = self.GetService(typeof(T));

			if (service == null)
				return null;

			return (T) service;
		}

		/// <summary>
		/// attempts to get the service and if it does not exist it will be created. Requires a parameterless
		/// constructor.
		/// </summary>
		/// <returns>The or add service.</returns>
		/// <param name="self">Self.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetOrAddService<T>(this GameServiceContainer self) where T : class, new()
		{
			var service = self.GetService(typeof(T));

			if (service == null)
				return AddService(self, new T());

			return (T) service;
		}
	}
}