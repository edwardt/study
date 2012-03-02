using System;

namespace mmchecker.vm
{
	/// <summary>
	/// This class stores an instance of System.Threading.Thread class, it is a subclass
    /// of class VMValue_objectinst.
    /// It stores a threadStartGUID which is a guid to a VMValue_threadstart object
	/// </summary>
	public class VMValue_thread : VMValue_objectinst
	{
		private int threadStartGUID;

		/// <summary>
		/// Private constructor to be used by Duplicate() only
		/// </summary>
		private VMValue_thread()
		{
		}

		public VMValue_thread(int guid, VMValueManager valueFactory, int threadStartGUID)
			: base(guid, valueFactory, new CILClass("[mscorlib]System.Threading.Thread"))
		{
			this.threadStartGUID = threadStartGUID;
		}

		public int ThreadStartGUID
		{
			get { return threadStartGUID; }
            set { threadStartGUID = value; }
		}

		public override void CopyInternalData(VMValue other)
		{
			base.CopyInternalData (other);
			threadStartGUID = ((VMValue_thread)other).threadStartGUID;
		}

		public override VMValue Duplicate()
		{
			VMValue_thread ret = new VMValue_thread();
			ret.CopyInternalData(this);
			return ret;
		}

		/// <summary>
		/// Called during TakeSnapshot in State so that all guids
		/// are reached and renamed
		/// This function is to be overriden by subclasses to traverse
		/// to guids linked to them
		/// </summary>
		public override void TraverseGuids(StateSnapshot ss)
		{
			ss.Convert(threadStartGUID);
		}

		public override void SaveData(StateSnapshot ss)
		{
			base.SaveData(ss);
			ss.WriteGuid(threadStartGUID);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            int tmp = MaSProcess(tid);
            if (tmp != -2)
            {
                VMValue v = valueMan.GetValue(threadStartGUID);
                if (v != null)
                    v.MarkAndSweep(tmp, valueMan);
            }
        }
	}
}
