using System;
using mmchecker.vm;

namespace mmchecker
{
    /// <summary>
    /// Compare two top values on the stack and put true(1) if one is greater than the other
    /// ..., value1, value2 -> ..., result
    /// result = 1 iff value1 > value2
    /// </summary>
    public class CIL_cgt : CILInstruction
    {
        public CIL_cgt(string label)
            : base(label)
        {
        }

        public override bool IsThreadLocal()
        {
            return true;
        }

        public override Semantic GetSemantic()
        {
            return Semantic.NONE;
        }

        public override string ToString()
        {
            return label + ":  ceq";
        }

        public override CILInstruction Execute(ThreadState threadState)
        {
            // correctness check, only needed in debug
            if (CanExecute(threadState) == false)
                throw new Exception("This ceq instruction is not ready to execute");

            // gets the two values and compare them
            VMValue topOfStack = threadState.GetValue(threadState.ThreadStack.Peek());
            VMValue_int32 ret = (VMValue_int32)threadState.SystemState.Values.MakeValue(new CILVar_int32(""));
            if (topOfStack is VMValue_int32)
            {
                VMValue_int32 value2 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
                VMValue_int32 value1 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
                if (value1.Value > value2.Value)
                    ret.Value = 1;
                else
                    ret.Value = 0;
            }
            else
                throw new Exception("Unsupported data type for instruction cgt");

            ret.IsThreadLocal = true;
            ret.IsConcrete = true;
            threadState.ThreadStack.Push(ret.GUID);
            return threadState.CurrentMethod.GetNextInstruction(this);
        }

        public override bool CanExecute(ThreadState threadState)
        {
            // this can be considered as an "arithmetic operation", so can't be executed
            // until both necessary values are ready 
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
            VMValue v2 = threadState.GetValue(threadState.ThreadStack.Peek(1));
            return (v1.IsConcrete && v2.IsConcrete);
        }
    }
}
