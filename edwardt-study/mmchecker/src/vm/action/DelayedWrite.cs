using System;
using mmchecker.util;

namespace mmchecker.vm.action
{
	/// <summary>
	/// This class represents an incomplete write action
    /// Any bytecode that has semantic write creates a DelayedWrite and append to the 
    /// thread's list of incomplete actions. The DelayedWrite will be completed later.
    /// Each DelayedWrite contains two addresses (guid): source and destination
	/// </summary>
	public class DelayedWrite : DelayedAction
	{
		protected int destValue;
		protected int srcValue;

		public DelayedWrite(int destValue, int srcValue, CILInstruction sourceInstruction)
		{
			this.destValue = destValue;
			this.srcValue = srcValue;
			this.sourceInstruction = sourceInstruction;
		}

		public int Source 
		{
			get { return srcValue; }
		}

		public int Destination
		{
			get { return destValue; }
		}

		public override DelayedAction Duplicate()
		{
			return new DelayedWrite(destValue, srcValue, SourceInstruction);
		}

		public override CILInstruction.Semantic GetSemantic()
		{
			return CILInstruction.Semantic.WRITE;
		}
	
		public override bool DoesConflict(DelayedAction da)
		{
			if(da is DelayedWrite)
			{
				DelayedWrite dw = (DelayedWrite)da;
				if((dw.Destination == this.srcValue) || (dw.Destination == this.destValue))
					return true;
				if((dw.Source == this.destValue))
					return true;
				return false;
			}
			else if(da is DelayedRead)
			{
				DelayedRead dr = (DelayedRead)da;
				if((dr.Destination == this.srcValue) || (dr.Destination == this.destValue))
					return true;
				if((dr.Source == this.destValue))
					return true;
				return false;
			}else
				return false;
		}

		public override bool CanComplete(ThreadState threadState)
		{
			// A DelayedWrite can always be completed if it is allowed
			return true;
		}

		public override void Complete(ThreadState threadState)
		{
			VMValue dest = threadState.GetValue(destValue);
			VMValue src = threadState.GetValue(srcValue);
			dest.CopyFrom(src);
			dest.IsConcrete = true;
			if(CheckerConfiguration.DoPrintExecution)
			{
				if(dest is VMValue_int32)
					Console.Write("Committing write of value {0}",((VMValue_int32)dest).Value);
				else if(dest is VMValue_double)
					Console.Write("Committing write of value {0}",((VMValue_double)dest).Value);
				else if(dest is VMValue_object)
					Console.Write("Committing write of {0}", ((VMValue_object)dest).ClassType);
				else
					Console.Write("Committing write of non-displayable value");
				Console.WriteLine(" from {0} in {1}", sourceInstruction, threadState.CurrentMethod.Name);
			}
		}

		public override void TakeSnapshot(StateSnapshot ss)
		{
			// write identifier 1 to indicate that this is a DelayedWrite
			ss.WriteInt(1);
			ss.WriteGuid(srcValue);
			ss.WriteGuid(destValue);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            valueMan.GetValue(srcValue).MarkAndSweep(tid, valueMan);
            valueMan.GetValue(destValue).MarkAndSweep(tid, valueMan);
        }

        public override bool Escape(VMValueManager valueMan)
        {
            return valueMan.GetValue(srcValue).Escape || valueMan.GetValue(destValue).Escape;
        }
    }
}
