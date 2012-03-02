using System;
using System.Collections;
using System.IO;
using mmchecker.util;

namespace mmchecker.util
{
	/// <summary>
	/// General purpose configuration class to store configurations 
	/// The configurations can be loaded from a file, and/or supplied
	/// by command line switches
	/// </summary>
	public class Configuration
	{
		// Keeps the properties in pair of (name, value)
		static Hashtable properties = new Hashtable();

		// Numeric properties are detected and kept in this for efficiency
		static Hashtable numericProperties = new Hashtable();

		/// <summary>
		/// Load the configurations from the parsed command line
		/// If the first argument is not a switch (starting with -)
		/// then the first argument is assumed to be a configuration file
		/// and configuration is loaded from it. The following 
		/// switches are processed to modify or add properties.
		/// </summary>
		/// <param name="args"></param>
		public static void LoadConfiguration(string[] args)
		{
			// load the configuration from the file if filename is supplied
			if(args[0][0] != '-')
				LoadConfiguration(args[0]);

			// process the switches
			for(int i = 0; i < args.Length; i++)
				if(args[i][0] == '-')
				{
					string st = args[i].Substring(1);
					string[] tokens = st.Split('=');
					if(properties.ContainsKey(tokens[0]))
						properties.Remove(tokens[0]);
					if(tokens.Length == 1)
						properties.Add(tokens[0], "1");
					else
						properties.Add(tokens[0], tokens[1]);
				}
			numericProperties.Clear();
			IDictionaryEnumerator iter = properties.GetEnumerator();
			while(iter.MoveNext())
			{
				try
				{
					int v = Int32.Parse((string)iter.Value);
					numericProperties.Add(iter.Key, v);
				}
				catch(FormatException)
				{
				}
			}
		}

		static void LoadConfiguration(string filename)
		{
			StreamReader fin = new StreamReader(filename);
			while(true)
			{
				string s = fin.ReadLine();
				if(s == null)
					break;
				s.Trim();
				if(s.Length < 1)
					continue;
				if(s.StartsWith("#"))
					continue;
				Strtok st = new Strtok(s);
				string key = st.NextToken("=");
				string value = st.NextToken("\n");
				properties.Add(key, value);
			}
			fin.Close();
		}

		public static string GetProperty(string key)
		{
			return (string)properties[key];
		}

		public static int GetNumericProperty(string key)
		{
			if(numericProperties[key] != null)
				return (int)numericProperties[key];
			else
				return -1;
		}
	}
}
