using System;
using System.Text;

namespace mmchecker.util
{
	/// <summary>
	/// Summary description for Strtok.
	/// </summary>
	public class Strtok
	{
		string str;
		int cursor;

		public Strtok(string str)
		{
			this.str = str;
			cursor = -1;
		}

		public int Cursor
		{
			get { return cursor; }
		}

		public string NextToken()
		{
			return NextToken(" ");
		}

		public string NextToken(string tokens)
		{
			cursor++;
			if(cursor >= str.Length)
				return null;

			// walk pass the preceding tokens
			while(cursor < str.Length)
			{
				bool found = false;
				for(int j = 0; j < tokens.Length; j++)
					if(str[cursor] == tokens[j])
					{
						found = true;
						break;
					}
				if(found == false)
					break;
				else
					cursor++;
			}
			
			// check if there is nothing left
			if(cursor >= str.Length)
				return null;

			StringBuilder sb = new StringBuilder();
			while(true)
			{
				sb = sb.Append(str[cursor]);
				cursor++;
				if(cursor >= str.Length)
					break;
				for(int j = 0; j < tokens.Length; j++)
					if(str[cursor] == tokens[j])
						goto finishtoken;
			}
			finishtoken:
			return sb.ToString();
		}
	}
}
