using System;
using System.Collections;
using System.Diagnostics;

namespace mmchecker.vm
{
	class StateRecord
	{
		byte[] md5code;

		public StateRecord(byte[] md5code)
		{
			this.md5code = md5code;
		}

		public override bool Equals(object obj)
		{
			StateRecord other = (StateRecord)obj;
			if(md5code.Length != other.md5code.Length)
				return false;
			for(int i = 0; i < md5code.Length; i++)
				if(md5code[i] != other.md5code[i])
					return false;
			return true;
		}

		public override int GetHashCode()
		{
            // NOTE: this is very tricky and interesting, if we replace + by & 
            // then the code may run 2 times slower
			return (md5code[0] << 16) + (md5code[1] << 8) + md5code[2];
		}
	}
	/// <summary>
	/// Summary description for StateSet.
	/// </summary>
	public class StateSet
	{
		Hashtable stateHashes;
		DateTime startTime;

		public StateSet()
		{
            startTime = DateTime.Now;
			stateHashes = new Hashtable();
		}

		public int Count 
		{
			get { return stateHashes.Count; }
		}

		public int GetStateId(State s)
		{
			object tmp = stateHashes[new StateRecord(s.SnapshotSig)];
			if(tmp == null)
				return -1;
			else
				return (int)tmp;
		}

		public bool HasState(State s)
		{
			return GetStateId(s) >= 0;
		}

		public void AddState(State s)
		{
			if(HasState(s))
				return;
			else
				stateHashes.Add(new StateRecord(s.SnapshotSig), stateHashes.Count);
			if(stateHashes.Count % 10000 == 0)
				Console.WriteLine("count={0} {1}", stateHashes.Count, DateTime.Now.Subtract(startTime));
		}
	}
}