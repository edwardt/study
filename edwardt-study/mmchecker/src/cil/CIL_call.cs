using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// CIL instruction for calling a static function
	/// </summary>
	public class CIL_call : CILInstruction
	{					 
		// The method to be called
		CILMethod theMethod;

		public CIL_call(string label, CILMethod theMethod) : base(label)
		{
			this.theMethod = theMethod;
		}

		public CILMethod TheMethod
		{
			get { return theMethod; }
		}

		public override bool IsThreadLocal()
		{
			if(theMethod.ParentClass.Name == "[mscorlib]System.Threading.Thread")
			{
				// only Thread.MemoryBarrier() is supported at the moment
				// it is non-local because it has only one stage (issuing = completion)
				// which is a full fence
				if(theMethod.Name == "MemoryBarrier")
				{
					return false;
				}
				else
				{
					throw new Exception("Not supported function " + theMethod.ParentClass.Name + "::" + theMethod.Name);
				}
			}
			else if(theMethod.ParentClass.Name == "[mscorlib]System.Threading.Monitor")
			{
				// Enter and Exit's issuing stage is local
				if((theMethod.Name == "Enter") ||
					(theMethod.Name == "Exit") ||					
					(theMethod.Name == "Pulse") ||
					(theMethod.Name == "PulseAll"))
					return true;
				else if(theMethod.Name == "Wait")
					return false;
				else
					throw new Exception("Not supported function of [mscorlib]System.Threading.Monitor");
			}else // ordinary function calls are local because they are just jumping
				return true;
		}

        public override bool Escape()
        {
            if (theMethod.ParentClass.Name == "[mscorlib]System.Threading.Thread")
            {
                return true;
            }
            else if (theMethod.ParentClass.Name == "[mscorlib]System.Threading.Monitor")
            {
                return true;
            }
            else // ordinary function calls are not escaping because they are just jumping
                return false;
        }

        public override Semantic GetSemantic()
		{
			// its semantic is a full fence, but because it is much easier and efficient
			// to implement by completing all incomplete bytecodes before it, then execute
			// it, we set this as NONE
			return Semantic.NONE;
		}

		public override string ToString()
		{
			return label + ":  call       " + theMethod.Name;
		}
		
		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			if(theMethod.ParentClass.Name == "[mscorlib]System.Object")
			{
				if(theMethod.Name == ".ctor")
					return true;
			} 
			else if(theMethod.ParentClass.Name == "[mscorlib]System.Threading.Thread")
			{
				if(theMethod.Name == "MemoryBarrier")
				{
					// effectively blocks all instructions from completing out of order with 
					// this barrier
					return threadState.pendingActions.Count == 0;
				}
				else
				{
					throw new Exception("Not supported function " + theMethod.ParentClass.Name + "::" + theMethod.Name);
				}
			}
			else if(theMethod.ParentClass.Name == "[mscorlib]System.Threading.Monitor")
			{
				if(theMethod.Name == "Enter")
				{
					// Can execute enter a lock first if the ld is completed
					// so that the pointer is concrete
					// and then the lock on that must either be open
					// or locked by the same thread
					VMValue_object v = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Peek());
					if(v.IsConcrete == false)
						return false;
					// TODO: This should crash the vm because we call lock on a
					// null pointer, now we block the execution by saying that
					// this instruction is not yet ready to be executed
					if(v.ValueGUID == -1)
						return false;
					// we can start the lock even if we don't own the lock now
					// we only need to own the lock when the instruction completes
					return true;
				}
				else if((theMethod.Name == "Exit") ||
					(theMethod.Name == "Wait") ||
					(theMethod.Name == "Pulse") ||
					(theMethod.Name == "PulseAll"))
				{
					// these wait/pulse instructions requires an object pointer to issue
					VMValue v = threadState.GetValue(threadState.ThreadStack.Peek());
					return v.IsConcrete;
				}
				else
					throw new Exception("Not supported function of [mscorlib]System.Threading.Monitor");
			}
			else if(theMethod.ParentClass.Name == "MMC")
			{
				if(theMethod.Name == "Report")
				{
					// requires a number on the stack to execute
					VMValue v = threadState.GetValue(threadState.ThreadStack.Peek());
					return v.IsConcrete;
				}
			}
			else if(theMethod.ParentClass.Name == "[mscorlib]System.Math")
			{
				// math operations, require the approriate number of numbers on the stack
				switch(theMethod.Name)
				{
					case "Abs":
					case "Sin":
					case "Cos":
						return threadState.GetValue(threadState.ThreadStack.Peek()).IsConcrete;
					case "Pow":
						return (
							threadState.GetValue(threadState.ThreadStack.Peek()).IsConcrete
							&& threadState.GetValue(threadState.ThreadStack.Peek(1)).IsConcrete);
					default:
						throw new Exception("Not supported function from [mscorlib]System.Math");
				}
			}
			return true;			
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			VMValue_double v, vnew, vpower, vbase;
			VMValue_int32 vint;

			if(CanExecute(threadState) == false)
				throw new Exception("Something's wrong, executing assertion when value is not ready");
			if(theMethod.ParentClass.Name == "[mscorlib]System.Object")
			{
				if(theMethod.Name == ".ctor")
					return threadState.CurrentMethod.GetNextInstruction(this);
			} 
			else if(theMethod.ParentClass.Name == "MMC")
			{
				// this is the checker's assertion class
				if(theMethod.Name == "Report")
				{
					vint = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
					threadState.SystemState.Report(vint.Value);
				}
				return threadState.CurrentMethod.GetNextInstruction(this);
			}
			else if(theMethod.ParentClass.Name == "[mscorlib]System.Threading.Thread")
			{
				// just go pass this instruction because the CanExecute() has made sure 
				// that there is incomplete bytecodes
				if(theMethod.Name == "MemoryBarrier")
					return threadState.CurrentMethod.GetNextInstruction(this);
				else
					throw new Exception("Not supported function " + theMethod.ParentClass.Name + "::" + theMethod.Name);
			}
			else if(theMethod.ParentClass.Name == "[mscorlib]System.Threading.Monitor")
			{
				if(theMethod.Name == "Enter")
				{
					VMValue_object obj = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Pop());
					DelayedLock dl = new DelayedLock(obj.ValueGUID, this);
					threadState.AddPendingAction(dl);
					return threadState.CurrentMethod.GetNextInstruction(this);
				}
				else if(theMethod.Name == "Exit")
				{
					VMValue_object obj = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Pop());
					DelayedUnlock du = new DelayedUnlock(obj.ValueGUID, this);
					threadState.AddPendingAction(du);
					return threadState.CurrentMethod.GetNextInstruction(this);
				}	
				else if(theMethod.Name == "Wait")
				{
					VMValue_object obj = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Pop());
					DelayedWait du = new DelayedWait(obj.ValueGUID, this, 0);
					threadState.AddPendingAction(du);

					// set this thread to waiting
					threadState.syncState = ThreadState.SyncState.WAITING;
					threadState.waitingOn = obj.ValueGUID;

					// release the lock
					VMValue_objectinst objinst = (VMValue_objectinst)threadState.GetValue(obj.ValueGUID);
					du.lockCount = objinst.HoldingLockCount;
					objinst.HoldingLockCount = 0;
					objinst.HoldingLockThreadID = -1;

					// TODO: Now wait always return 1, change to reflect the result of the call
					VMValue_int32 theConstant = (VMValue_int32)threadState.SystemState.Values.MakeValue(new CILVar_int32(""));
					theConstant.Value = 1;
					theConstant.IsThreadLocal = true;
					theConstant.IsConcrete = true;			
					threadState.ThreadStack.Push(theConstant.GUID);

					return threadState.CurrentMethod.GetNextInstruction(this);
				}	
				else if(theMethod.Name == "Pulse")
				{
					VMValue_object obj = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Pop());
					DelayedPulse du = new DelayedPulse(obj.ValueGUID, this, false);
					threadState.AddPendingAction(du);
					return threadState.CurrentMethod.GetNextInstruction(this);
				}	
				else if(theMethod.Name == "PulseAll")
				{
					VMValue_object obj = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Pop());
					DelayedPulse du = new DelayedPulse(obj.ValueGUID, this, true);
					threadState.AddPendingAction(du);
					return threadState.CurrentMethod.GetNextInstruction(this);
				}else
					throw new Exception("Function " + theMethod.Name + " of class Monitor is not supported");
			}
			else if(theMethod.ParentClass.Name == "[mscorlib]System.Math")
			{
				// arithmetic instructions, just do the mathematics associated with each
				switch(theMethod.Name)
				{
					case "Abs":
						v = (VMValue_double)threadState.GetValue(threadState.ThreadStack.Pop());
						vnew = (VMValue_double)threadState.SystemState.Values.MakeValue(v);
						vnew.IsThreadLocal = true;
						vnew.IsConcrete = true;
						vnew.Value = Math.Abs(v.Value);
						threadState.ThreadStack.Push(vnew.GUID);
						break;
					case "Sin":
						v = (VMValue_double)threadState.GetValue(threadState.ThreadStack.Pop());
						vnew = (VMValue_double)threadState.SystemState.Values.MakeValue(v);
						vnew.IsThreadLocal = true;
						vnew.IsConcrete = true;
						vnew.Value = Math.Sin(v.Value);
						threadState.ThreadStack.Push(vnew.GUID);
						break;
					case "Cos":
						v = (VMValue_double)threadState.GetValue(threadState.ThreadStack.Pop());
						vnew = (VMValue_double)threadState.SystemState.Values.MakeValue(v);
						vnew.IsThreadLocal = true;
						vnew.IsConcrete = true;
						vnew.Value = Math.Cos(v.Value);
						threadState.ThreadStack.Push(vnew.GUID);
						break;
					case "Pow":
						vpower = (VMValue_double)threadState.GetValue(threadState.ThreadStack.Pop());
						vbase = (VMValue_double)threadState.GetValue(threadState.ThreadStack.Pop());
						vnew = (VMValue_double)threadState.SystemState.Values.MakeValue(vpower);
						vnew.IsThreadLocal = true;
						vnew.IsConcrete = true;
						vnew.Value = Math.Pow(vbase.Value, vpower.Value);
						threadState.ThreadStack.Push(vnew.GUID);
						break;
					default:
						throw new Exception("Not supported function of [mscorlib]System.Math");
				}
				return threadState.CurrentMethod.GetNextInstruction(this);
			}
			return threadState.CallFunction(theMethod);
		}		
	}
}
