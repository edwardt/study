namespace SynchronizationConcept
{
	using System;
	using System.Threading;
		
	public class Dikjstra
	{
		// Static variables of a class are shared among all the thread
		// Each thread executes on a separate call  stack with its own  separate local variables
		// Despite “Application Domain” concept allows multiple programs to execute in a 
		// single hardware address space, AppDomain has no effect on how to use threads
		
		// In general, there are four major mechanisms: thread creation, mutual exclusion, 
		// waiting for events, and some arrangement for getting a thread out of 
		// an unwanted long-term wait
		// Use the “lock” statement, the “Monitor” class, and the “Interrupt” method
		
		// Thread”, giving  its constructor a “ThreadStart” delegate*, and calling 
		// the new thread’s “Start” method
		// “Join” method of a thread: this makes the calling thread wait until the
		// given thread terminates
		// fine to fork a thread but never have a corresponding call of “Join”
		public Dikjstra ()
		{
					
		}
	}
}

