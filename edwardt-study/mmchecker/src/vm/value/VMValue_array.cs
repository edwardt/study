using System;

namespace mmchecker.vm
{
	/// <summary>
	/// The array class contains the (guid) pointer to an array instance, VMValue_arayinst
	/// </summary>
	public class VMValue_array : VMValue
	{
		int arrayInstGuid;

		/// <summary>
		/// Private constructor to be used by Duplicate() only
		/// </summary>
		private VMValue_array()
		{
		}

		public VMValue_array(int guid, int arrayInstGuid) : base(guid)
		{
			this.arrayInstGuid = arrayInstGuid;
		}

		public int ArrayInstGuid 
		{
			get { return arrayInstGuid;}
			set { arrayInstGuid = value; }
		}

		public override void CopyFrom(VMValue other)
		{
			arrayInstGuid = ((VMValue_array)other).arrayInstGuid;
		}

		public override void CopyInternalData(VMValue other)
		{
			base.CopyInternalData (other);
			arrayInstGuid = ((VMValue_array)other).arrayInstGuid;
		}

		public override VMValue Duplicate()
		{
			VMValue_array ret = new VMValue_array();
			ret.CopyInternalData(this);
			return ret;
		}

		public override void TraverseGuids(StateSnapshot ss)
		{
			ss.Convert(arrayInstGuid);
		}

		public override void SaveData(StateSnapshot ss)
		{
			base.SaveData (ss);
			ss.WriteGuid(arrayInstGuid);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            int tmp = MaSProcess(tid);
            if (tmp != -2)
            {
                // check children with tid
                VMValue v = valueMan.GetValue(arrayInstGuid);
                if (v != null)
                    v.MarkAndSweep(tmp, valueMan);
            }
        }
    }
}
