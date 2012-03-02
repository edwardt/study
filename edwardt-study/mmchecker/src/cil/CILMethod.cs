using System;
using System.Collections.Generic;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CILFunction.
	/// </summary>
	public class CILMethod
	{
		CILClass parentClass;
		string name;
		// TODO: parameter count and sig are not enough
		// need to parse to parameter types to match later
		string sig;
		int parameterCount;
		int id;
		string[] argNames;

		static int methodIdGenerator = 0;

		// contains CILVariable
		List<CILVariable> localVariables = new List<CILVariable>();

		// contains CILInstruction
		List<CILInstruction> instructions = new List<CILInstruction>();
		bool isStatic;

		public CILMethod(CILClass parentClass, string name, string sig)
		{
			this.id = methodIdGenerator++;
			this.parentClass = parentClass;
			this.name = name;
			this.sig = sig;

			if(sig == "()")
				parameterCount = 0;
			else
			{
				parameterCount = 1;
				for(int i = 0; i < sig.Length; i++)
					if(sig[i] == ',')
						parameterCount++;
			}
		}

		public CILClass ParentClass 
		{
			get { return parentClass; }
		}

		public string Name
		{
			get { return name; }
		}

		public string Sig 
		{
			get { return sig; }
		}

		public int ID 
		{
			get { return id; }
		}

		public bool IsStatic
		{
			get { return isStatic; }
			set { isStatic = value; }
		}

		public IEnumerator<CILVariable> GetLocalVariableEnumerator()
		{
			return localVariables.GetEnumerator();
		}

		public void AddInstruction(CILInstruction inst)
		{
			instructions.Add(inst);
			inst.ParentMethod = this;
		}

		public void AddLocalVariable(CILVariable variable)
		{
			localVariables.Add(variable);
		}

		public CILInstruction GetFirstInstruction()
		{
			if(instructions.Count == 0)
				return null;
			else
				return (CILInstruction)instructions[0];
		}

		public CILInstruction GetNextInstruction(CILInstruction thisInst)
		{
			int i;
			for(i = 0; i < instructions.Count; i++)
				if(((CILInstruction)instructions[i]).ID == thisInst.ID)
					break;
			if(i < instructions.Count)
				return (CILInstruction)instructions[i + 1];
			else
				throw new Exception("Cannot find the next instruction");
		}

		public int GetInstructionIndex(CILInstruction inst)
		{
			if(inst == null)
				return -1;
			for(int i = 0; i < instructions.Count; i++)
				if(((CILInstruction)instructions[i]).ID == inst.ID)
					return i;
			throw new Exception("The instruction is not from this method");
		}

		public CILInstruction GetInstruction(string label)
		{
			foreach(CILInstruction inst in instructions)
				if(inst.Label == label)
					return inst;
			throw new Exception("Should never branch to an unknown instruction label");
		}

		public override string ToString()
		{
			return "method " + name + " " + sig;
		}

		public string Print()
		{
			string ret = "   method " + Name + " " + sig + "\n" ;
			ret += "   {\n";
			foreach(CILVariable var in localVariables)
				ret = ret + "      .field " + var.ToString() + "\n";

			foreach(CILInstruction inst in instructions)
				ret = ret + "      " + inst.ToString() + "\n";
			ret += "   }\n\n";

			return ret;
		}

		public int GetLocalVariableIndex(string name)
		{
			for(int i = 0; i < localVariables.Count; i++)
				if(((CILVariable)localVariables[i]).Name == name)
					return i;
			return -1;
		}

		public int ParameterCount 
		{
			get { return parameterCount; }
		}

		public void SetArgumentNames(List<string> argNames)
		{
			this.argNames = new string[argNames.Count];
			for(int i = 0; i < argNames.Count;i++)
				this.argNames[i] = argNames[i];
		}

		/// <summary>
		/// Gets the index of the argument from a name
		/// It is called by parser when there are many arguments 
		/// that ldarg.number is not enough
		/// </summary>
		/// <param name="name">Name of the argument</param>
		/// <returns>The index of the argument, starting from 0</returns>
		public int GetArgumentIndex(string name)
		{
			for(int i = 0; i < argNames.Length; i++)
				if(argNames[i] == name)
					return i;
			throw new Exception("Unknown argument name");
		}

		/// <summary>
		/// Returns the number of CIL instructions in this method
		/// </summary>
		/// <returns>The number of instructions in this method</returns>
		public int GetInstructionCount()
		{
			return instructions.Count;
		}

		public bool InsertInstruction(int locationID, CILInstruction newInst)
		{
			int i;
			for(i = 0; i < instructions.Count; i++)
				if(((CILInstruction)instructions[i]).ID == locationID)
					break;
			if(i < instructions.Count)
			{
				string tmpLabel = ((CILInstruction)instructions[i]).Label;
				((CILInstruction)instructions[i]).Label = newInst.Label;
				newInst.Label = tmpLabel;
				instructions.Insert(i, newInst);
				newInst.ParentMethod = this;
				return true;
			}
			else
				return false;
		}

		public bool RemoveInstruction(int instID)
		{
			int i;
			for(i = 0; i < instructions.Count; i++)
				if(((CILInstruction)instructions[i]).ID == instID)
					break;
			if(i < instructions.Count)
			{
				((CILInstruction)instructions[i + 1]).Label = ((CILInstruction)instructions[i]).Label;
				instructions.RemoveAt(i);
				return true;
			}
			else
				return false;
		}

		public List<CILVariable> LocalVariables
		{
			get {return localVariables; }
		}

		public List<CILInstruction> Instructions
		{
			get {return instructions; }
		}

	}
}