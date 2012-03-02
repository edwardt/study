using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace mmchecker.vm
{
	/// <summary>
	/// Summary description for StateSnapshot.
	/// </summary>
	public class StateSnapshot
	{
		MemoryStream msData;
		BinaryWriter bwData;
		Hashtable valueMap;
		VMValueManager values;

        // ArrayList of guid type, which is int
		List<int> seenGuids;

		int nextguid;

		int dbg_stage;

		public StateSnapshot(VMValueManager values)
		{
			msData = new MemoryStream();
			bwData = new BinaryWriter(msData);
			valueMap = new Hashtable();
			this.values = values;
			seenGuids = new List<int>();
			nextguid = 0;
			dbg_stage = 1;
		}

		public void WriteString(string s)
		{
			bwData.Write(s.Length);
			bwData.Write(s);
		}

		public void WriteInt(int x)
		{
			bwData.Write(x);
		}

		public void WriteLong(long l)
		{
			bwData.Write(l);
		}

		public void WriteDouble(double d)
		{
			bwData.Write(d);
		}

		public int Convert(int guid)
		{
			if(guid == -1)
				return -1;
			if(valueMap.ContainsKey(guid) == false)
			{
				if(dbg_stage == 2)
				{
					Console.WriteLine("BUG: Not supposed to see new guids in stage 2 of StateSnapshot");
				}
				valueMap.Add(guid, nextguid++);
				seenGuids.Add(guid);
				
				// explore the value link to this guid
				VMValue v = values.GetValue(guid);
				v.TraverseGuids(this);
			}
			return (int)valueMap[guid];
		}

		public void WriteGuid(int guid)
		{
			WriteInt(Convert(guid));
		}

		public byte[] GetStoringData()
		{
			dbg_stage = 2;
			for(int i = 0; i < seenGuids.Count; i++)
			{
				VMValue v = values.GetValue(seenGuids[i]);
				v.SaveData(this);
			}

			byte[] ret = new byte[msData.Length];
			msData.Seek(0, SeekOrigin.Begin);
			msData.Read(ret, 0, ret.Length);
/*			for(int i = 0; i < ret.Length; i++)
				Console.Write("{0} ", ret[i]);
			Console.WriteLine();*/
			return ret;
		}
	}
}
