using System;

namespace mmchecker.vm
{
	/// <summary>
	/// This class stores the value for data of type int64
	/// </summary>
	public class VMValue_int64 : VMValue
	{
		long value;

		/// <summary>
		/// Private constructor to be used by Duplicate() only
		/// </summary>
		private VMValue_int64()
		{
		}

		public VMValue_int64(int guid, long value) : base(guid)
		{
			this.value = value;
		}

		public long Value 
		{
			get { return value; }
			set { this.value = value; }
		}

		public override void CopyFrom(VMValue other)
		{
			this.value = ((VMValue_int64)other).value;
		}

		public override void CopyInternalData(VMValue other)
		{
			base.CopyInternalData (other);
			value = ((VMValue_int64)other).value;
		}


		public override VMValue Duplicate()
		{
			VMValue ret = new VMValue_int64();
			ret.CopyInternalData(this);
			return ret;
		}

		/// <summary>
		/// Called during TakeSnapshot in State to dump the data of this value
		/// </summary>
		/// <param name="ss"></param>
		public override void SaveData(StateSnapshot ss)
		{
			base.SaveData(ss);
			ss.WriteLong(value);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            MaSProcess(tid);
        }
    }
}
