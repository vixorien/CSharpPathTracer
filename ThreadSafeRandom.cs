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
		private static Random _global = new Random();

		[ThreadStatic]
		private static Random _local;

		public static Random Instance
		{
			get
			{
				if (_local == null)
				{
					int seed;
					lock (_global) seed = _global.Next();
					_local = new Random(seed);
				}

				return _local;
			}
		}
	}
}
