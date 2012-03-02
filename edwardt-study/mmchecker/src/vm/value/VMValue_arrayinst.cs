using System;

namespace mmchecker.vm
{
	/// <summary>
	/// Array instance contains the actual data
	/// Right now we only supports arrays of int32
	/// </summary>
	public class VMValue_arrayinst : VMValue
	{
		int[] data; // keeping the guids of all elements
		CILVariable elementType;

		/// <summary>
		/// Private constructor to be used by Duplicate() only
		/// </summary>
		private VMValue_arrayinst(CILVariable elementType)
		{												 
			this.elementType = elementType;
		}

		public VMValue_arrayinst(int guid, int size, CILVariable elementType, VMValueManager valueManager) : base(guid)
		{
			this.elementType = elementType;
			data = new int[size];
			for(int i = 0; i < size; i++)
				data[i] = valueManager.MakeValue(elementType).GUID;
		}

		public int GetLength()
		{
			return data.Length;
		}

		public override void CopyFrom(VMValue other)
		{
			throw new Exception("Direct copy of arrayinst is not allowed");
		}

		public override void CopyInternalData(VMValue other)
		{
			base.CopyInternalData (other);
			VMValue_arrayinst oth = (VMValue_arrayinst)other;
			if(data == null)
				data = new int[oth.data.Length];
			for(int i = 0; i < data.Length; i++)
				data[i] = oth.data[i];
		}

		public override VMValue Duplicate()
		{
			VMValue_arrayinst ret = new VMValue_arrayinst(elementType);
			ret.CopyInternalData(this);
			return ret;
		}

		public override void TraverseGuids(StateSnapshot ss)
		{
			for(int i = 0; i < data.Length; i++)
				ss.Convert(data[i]);
		}

		public override void SaveData(StateSnapshot ss)
		{
			base.SaveData (ss);
			ss.WriteInt(data.Length);
			for(int i = 0; i < data.Length; i++)
				ss.WriteGuid(data[i]);
		}

		public int GetElement(int eleIndex)
		{
			return data[eleIndex];
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            int tmp = MaSProcess(tid);
            if (tmp != -2)
            {
                // check children with tid
                for (int i = 0; i < data.Length; i++)
                    if (data[i] != -1)
                    {
                        VMValue v = valueMan.GetValue(data[i]);
                        v.MarkAndSweep(tmp, valueMan);
                    }
            }
        }
    }
}