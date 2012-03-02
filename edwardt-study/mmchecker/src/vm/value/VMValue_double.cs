using System;

namespace mmchecker.vm
{
	/// <summary>
	/// The class contains the value of double type
	/// </summary>
	public class VMValue_double : VMValue
	{
		double value;

		/// <summary>
		/// Private constructor to be used by Duplicate() only
		/// </summary>
		private VMValue_double()
		{
		}

		public VMValue_double(int guid, double value) : base(guid)
		{
			this.value = value;
		}

		public double Value 
		{
			get { return value; }
			set { this.value = value; }
		}

		public override void CopyFrom(VMValue other)
		{
			this.value = ((VMValue_double)other).value;
		}

		public override void CopyInternalData(VMValue other)
		{
			base.CopyInternalData (other);
			value = ((VMValue_double)other).value;
		}


		public override VMValue Duplicate()
		{
			VMValue ret = new VMValue_double();
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
			ss.WriteDouble(value);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            MaSProcess(tid);
        }
    }
}