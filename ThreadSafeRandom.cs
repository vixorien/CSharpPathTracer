using System;

namespace CSharpPathTracer
{
	/// <summary>
	/// Creates a new random objects for any thread that 
	/// requests an instance.  Idea from here:
	/// https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way/
	/// </summary>
	static class ThreadSafeRandom
	{
		// A single global random used to seed the thread-specific objects
		private static Random _global = new Random();

		// A local random object per thread
		[ThreadStatic]
		private static Random _local;

		/// <summary>
		/// Gets a thread-specific random object
		/// </summary>
		public static Random Instance
		{
			get
			{
				// If this thread doesn't have its object yet, make it
				if (_local == null)
				{
					int seed;
					lock (_global) 
						seed = _global.Next();
					_local = new Random(seed);
				}

				return _local;
			}
		}
	}
}
