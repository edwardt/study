using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Summary description for Instruction.
	/// </summary>
	public abstract class CILInstruction
	{
		protected string label;
		CILMethod parentMethod;
		static int id_counter = 0; 
		int id;

		public enum Semantic
		{
			NONE = 0,
			READ = 1,
			WRITE = 2,
			VREAD = 3,
			VWRITE = 4,
			LOCK = 5,
			UNLOCK = 6,
			WAIT = 7,
			PULSE = 8
		}

		public CILInstruction(string label)
		{
			id = id_counter++;
			this.label = label;
		}

		public int ID 
		{
			get {return id;}
		}

		public string Label 
		{
			get { return label; }
			set { label = value; }
		}
		
		public CILMethod ParentMethod 
		{
			get { return parentMethod; }
			set { parentMethod = value; }
		}

		public abstract Semantic GetSemantic();

		public virtual CILInstruction Execute(ThreadState threadState)
		{
			Console.WriteLine(ToString());
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

		public virtual bool CanExecute(ThreadState threadState)
		{
			return true;
		}

		public abstract bool IsThreadLocal();

        /// <summary>
        /// The function returns whether an instruction is escaping in issuing stage
        /// All instructions that are splitted into issuing and completion stage 
        /// are not escaping at its escaping stage, so we return false as default to
        /// avoid rewriting this function for many.
        /// Whenever we are unsure, return true is safe and correct.
        /// </summary>
        /// <returns></returns>
        public virtual bool Escape()
        {
            return false;
        }
	}
}
