using System;
using System.IO;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CILScanner.
	/// </summary>
	public class CILScanner
	{
		StreamReader fin;
		string current;
		int cursor;

		public CILScanner(StreamReader fin)
		{
			this.fin = fin;
			current = "";
			cursor = 0;
		}

		public CILScanner(StreamReader fin, string current)
		{
			this.fin = fin;
			this.current = current + "\n"; 
			cursor = 0;
		}

		public bool IsSeparator(char c)
		{
			return ((c == '(') || (c == ')') || (c == ',') || (c == ':'));
		}

		public bool IsTokenChar(char c)
		{
			return (
				(('a' <= c) && (c <= 'z')) ||
				(('A' <= c) && (c <= 'Z')) ||
				(('0' <= c) && (c <= '9')) ||
				(c == '_') ||
				('[' == c) || (c == ']') ||
				(c == '.'));
		}

		public string NextToken()
		{
			string ret = "";
			char c = NextChar();
			while((IsSeparator(c) == false) && (IsTokenChar(c) == false))
				c = NextChar();
			if(IsSeparator(c))
			{
				ret += c;
				if((c == '(') || (c == ')'))
					return ret;
				while(IsSeparator(PeekChar()))
					ret += NextChar();
				return ret;
			}
			else if(IsTokenChar(c))
			{
				ret += c;
				while(IsTokenChar(PeekChar()))
					ret += NextChar();
				return ret;
			}
			return null;
		}

		private char PeekChar()
		{
			while(cursor >= current.Length)
			{
				current = GetNextLine();
				cursor = 0;
			}
			return current[cursor];
		}

		private char NextChar()
		{
			while(cursor >= current.Length)
			{
				current = GetNextLine();
				cursor = 0;
			}
			return current[cursor++];
		}

		private	string GetNextLine()
		{
			while(true)
			{
				string str = fin.ReadLine();			
				if(str == null)
					return null;
				if(str.Length == 0)
					continue;

				str = RemoveComment(str);

				if(str.Length == 0)
					continue;
            
				return str + "\n";
			}
		}

		static private string RemoveComment(string str)
		{
			if(str.IndexOf("//") >= 0)
				return str.Substring(0, str.IndexOf("//"));
			else
				return str;
		}
	}
}
