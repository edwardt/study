using System;

namespace mmchecker.vm
{
	/// <summary>
	/// This class stores the value for data of type int32
	/// </summary>
	public class VMValue_int32 : VMValue
	{
		int value;

		/// <summary>
		/// Private constructor to be used by Duplicate() only
		/// </summary>
		private VMValue_int32()
		{
		}

		public VMValue_int32(int guid, int value) : base(guid)
		{
			this.value = value;
		}

		public int Value 
		{
			get { return value; }
			set { this.value = value; }
		}

		public override void CopyFrom(VMValue other)
		{
			this.value = ((VMValue_int32)other).value;
		}

		public override void CopyInternalData(VMValue other)
		{
			base.CopyInternalData (other);
			value = ((VMValue_int32)other).value;
		}


		public override VMValue Duplicate()
		{
			VMValue ret = new VMValue_int32();
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
			ss.WriteInt(value);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            MaSProcess(tid);
        }
    }
}