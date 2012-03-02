using System;

namespace mmchecker.vm
{
	/// <summary>
	/// This data structure replaces the address of the function loaded by ldftn
    /// In real VM, it is the actual native address, in our VM we store the equivalent CILMethod
	/// </summary>
	public class VMValue_ftn : VMValue
	{
		CILMethod method;

		/// <summary>
		/// Private constructor to be used by Duplicate() only
		/// </summary>
		private VMValue_ftn()
		{
		}

		public VMValue_ftn(int guid, CILMethod method) : base(guid)
		{
			this.method = method;
		}

		public CILMethod Method 
		{
			get { return method; }
		}

		public override void CopyFrom(VMValue other)
		{
			this.method = ((VMValue_ftn)other).method;
		}

		public override void CopyInternalData(VMValue other)
		{
			base.CopyInternalData(other);
			method = ((VMValue_ftn)other).method;
		}

		public override VMValue Duplicate()
		{
			VMValue ret = new VMValue_ftn();
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
			ss.WriteInt(method.ID);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            MaSProcess(tid);
        }
    }
}