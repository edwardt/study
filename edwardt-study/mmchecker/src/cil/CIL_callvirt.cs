using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// CIL instruction for calling a non-static member function
	/// </summary>
	public class CIL_callvirt : CILInstruction
	{
		// the method to be called
		CILMethod theMethod;

		public CIL_callvirt(string label, CILMethod theMethod) : base(label)
		{
			this.theMethod = theMethod;
		}

		public CILMethod TheMethod
		{
			get { return theMethod; }
		}

		public override bool IsThreadLocal()
		{
			// Only Thread.* now is non local, the others are method calls
			if(theMethod.ParentClass.Name == "[mscorlib]System.Threading.Thread")
				return false;
			else
				return true;
		}

        public override bool Escape()
        {
            // Only Thread.* now is escaping, the others are method calls
            if (theMethod.ParentClass.Name == "[mscorlib]System.Threading.Thread")
                return true;
            else
                return false;
        }

        public override Semantic GetSemantic()
		{
			return Semantic.NONE;
		}

		public override string ToString()
		{
			return label + ":  callvirt   " + theMethod.ParentClass.Name + "::" + theMethod.Name;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This instruction is not ready to start");

			if(theMethod.ParentClass.Name == "[mscorlib]System.Threading.Thread")
			{
				if(theMethod.Name == "Start")
				{
					// Gets the needed details and then add the thread to global datastructure
					// The new thread is ready to be executed in the next step
					// TODO: Do POR on initial bytecodes of the thread to reduce one step
					VMValue_object threadptr = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Pop());
					VMValue_thread threadobj = (VMValue_thread)threadState.GetValue(threadptr.ValueGUID);
					ThreadState newThread = new ThreadState(threadState.SystemState, threadobj);
                    // no need to refer back 
                    // TODO: we may need the details later, but it causes problems for dynamic escape analysis
                    // by letting the original thread keep the pointer to the object
                    threadobj.ThreadStartGUID = -1;

                    threadState.SystemState.AddThread(newThread);
					return threadState.CurrentMethod.GetNextInstruction(this);
				}
				else if(theMethod.Name == "Join")
				{
					// Only need to pop the thread id, because the CanExecute() already
					// make sure the thread that this thread must join has already ended
					threadState.ThreadStack.Pop();
					return threadState.CurrentMethod.GetNextInstruction(this);
				}else
					throw new Exception("Not supported yet");
			}
			else
			{
				return threadState.CallFunction(theMethod);	
			}
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			// now the obj is concrete, we only check for locking instructions
			if(theMethod.ParentClass.Name == "[mscorlib]System.Threading.Thread")
			{
                if (threadState.GetValue(threadState.ThreadStack.Peek()).IsConcrete == false)
                    return false;
                if (theMethod.Name == "Start")
				{
                    // Ensure that every actions are complete before starting a new thread
                    // This is reasonable since starting a thread is a very "complex" requiring 
                    // many synchronization actions so these incomplete actions should be completed
                    // on the way. 
                    if (threadState.pendingActions.Count != 0)
                        return false;
					return true;
				}
				else if(theMethod.Name == "Join")
				{
					// Checks whether the thread that this thread is waiting to join has ended or not
					// No instruction after this join() is allowed to be issued before this join() is executed.
					VMValue_object threadptr = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Peek());
					VMValue_thread th = (VMValue_thread)threadState.GetValue(threadptr.ValueGUID);
					ThreadState tstate = (ThreadState)threadState.SystemState.GetThreadByID(th.GUID);
					return tstate.HasEnded();
				}
				else
					throw new Exception("Not supported yet");
			}
			else
			{
                // for callvirt, we need the pointer to the object be concrete
                if (threadState.GetValue(threadState.ThreadStack.Peek(theMethod.ParameterCount)).IsConcrete == false)
                    return false;				
				return true;
			}
		}
	}
}