using System;

namespace mmchecker.util
{
	/// <summary>
	/// The class is a customized Configuration which caches checker-specific
	/// properties for fast access and convenience.
	/// Needs to be clean up
	/// </summary>
	public class CheckerConfiguration : Configuration
	{
		static bool doPrintExecution = true;
		static int numThreads = 1;
		static bool doTiming = false;
        static bool doPOR = true; // if not specified, DoPOR is true

		public static void LoadCheckerConfiguration(string[] args)
		{
			LoadConfiguration(args);
			UpdateSpecialProperties();
		}

		public static void LoadCheckerConfiguration(string filename)
		{
			string[] tmp = new string[1];
			tmp[0] = filename;
			LoadCheckerConfiguration(tmp);
		}

		protected static void UpdateSpecialProperties()
		{
			doPrintExecution = GetNumericProperty("print_execution") == 1;
			doTiming = GetNumericProperty("timing") == 1;
            if(GetProperty("por") != null)
                doPOR = GetNumericProperty("por") == 1;

			numThreads = GetNumericProperty("num_thread");
			if(numThreads <= 0)
				numThreads = 1;			
		}

        public static bool DoPOR
        {
            get { return doPOR; }
        }

		public static bool DoPrintExecution
		{
			get { return doPrintExecution; }
		}

		public static int NumThreads 
		{
			get { return numThreads; }
		}

		public static bool DoTiming 
		{
			get { return doTiming; }
		}
	}
}
