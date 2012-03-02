using System;
using System.IO;
using System.Collections.Generic;
using mmchecker.util;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CILParser.
	/// </summary>
	public class CILParser
	{
		StreamReader fin; // used by GetNextLine, one Parser instance can only parse one code at a time
		CILProgram program;
		CILMethod currentMethod;
		bool nextAccessIsVolatile = false;

		public CILParser()
		{
		}

		static private string RemoveComment(string str)
		{
			if(str.IndexOf("//") >= 0)
				return str.Substring(0, str.IndexOf("//"));
			else
				return str;
		}

		static private string RemoveLeadingSpace(string str)
		{
			if(str.Length < 1)
				return str;
			int i;
			for(i = 0; i < str.Length; i++)
				if(str[i] != ' ')
					return str.Substring(i);
			return "";
		}

		static private string RemoveTrailingSpace(string str)
		{
			if(str.Length < 1)
				return str;
			if(str[str.Length - 1] == ' ')
			{
				int i;
				for(i = str.Length - 1; i >= 0; i--)
					if(str[i] != ' ')
						return str.Substring(0, i + 1);
				return "";
			}else
				return str;
		}

		string GetNextLine()
		{
			while(true)
			{
				string str = fin.ReadLine();			
				if(str == null)
					return null;
				if(str.Length == 0)
					continue;

				str = RemoveComment(str);

				str = RemoveLeadingSpace(str);
				str = RemoveTrailingSpace(str);
				if(str.Length == 0)
					continue;
                
				return str;
			}
		}

		// right now just find } to exit this assembly description
		void ParseAssembly(string str)
		{
			string s1;
			while(true)
			{
				s1 = GetNextLine();
				if(s1.Equals("}"))
					return;
			}
		}

        void ParseStackReserve(string str)
        {
            // don't need to do anything, ignore
        }

		CILVariable ParseLocalVariable(string str)
		{
			// TODO: Need cleanup and reparse using Scanner
			// with better treatment for array declaration
			string[] ss = str.Split();
			CILVariable ret;
			switch(ss[0])
			{
				case "class":
					ret = new CILVar_object(ss[2], program.GetClass(ss[1]));
					break;
                case "bool":
				case "int32":
					ret = new CILVar_int32(ss[1]);
					break;
				case "int64":
					ret = new CILVar_int64(ss[1]);
					break;
				case "float64":
					ret = new CILVar_double(ss[1]);
					break;
				case "int32[]":
					ret = new CILVar_array(ss[1], new CILVar_int32(""));
					break;
				case "float64[]":
					ret = new CILVar_array(ss[1], new CILVar_double(""));
					break;
				default:
					throw new Exception("Unknown local variable type");
			}
			return ret;
		}

		CILMethod ParseMethod(CILClass parentclass, string str)
		{
			// TODO: Need cleanup and reparse using Scanner
			List<string> argNames = new List<string>();
			// count how much deep we are in a try block so that we
			// dont confuse } of the try-s as } of the method
			int try_counter = 0;
			int catch_counter;

			bool isStatic = false;

			CILScanner scanner = new CILScanner(fin, str);
			string st;
			string methodName = null;
			while(methodName == null)
			{
				st = scanner.NextToken();
				if(st == "static")
					isStatic = true;
				if((st == "int32") || (st == "int64") || (st == "void") || (st == "float64") || (st == "bool"))
					methodName = scanner.NextToken();
				else if(st == "class")
				{
					st = scanner.NextToken();
					methodName = scanner.NextToken();
				}							  									  
			}
			// skips "("
			st = scanner.NextToken();
			string methodSig = "(";
			int paramCount = 0;
			while(true)
			{
				st = scanner.NextToken();
				if(st == ")")
					break;
				if(st != ",")
				{
					if(st == "class")
					{						
						paramCount++;
						if(paramCount > 1)
							methodSig += ",";
						methodSig += scanner.NextToken();					
					}
					else
					{
						paramCount++;
						if(paramCount > 1)
							methodSig += ",";
						methodSig += st;
					}
					argNames.Add(scanner.NextToken());
				}
			}
			methodSig += ")";

			CILMethod themethod = parentclass.GetMethod(methodName, methodSig);
			themethod.SetArgumentNames(argNames);
			themethod.IsStatic = isStatic;
			currentMethod = themethod;

			// parse the method body
			bool done = false;
			str = GetNextLine(); // skips "{"
			while(done == false)
			{
				str = GetNextLine();
				string[] ss = str.Split(' ');
				switch(ss[0])
				{
					case ".maxstack":
						// TODO
						// can optimize stack allocation
						break;
					case ".try":
						// ignore exception handling
						// increase the try counter so that we know next }
						// is not end of the method
						try_counter++;
						GetNextLine(); // skip the { of the try block
						break;
					case "finally":
						try_counter++;
						GetNextLine(); // skip the { of the finally block
						break;
					case "catch":
						// ignore the whole catch block, find } to stop
						// first ignore the {, then count to find the end
						GetNextLine();
						catch_counter = 1;
						while(true)
						{
							string sc = GetNextLine();
							sc = sc.TrimStart();
							if(sc.Length > 1)
							{
								if(sc[0] == '{')
									catch_counter++;
								else if(sc[0] == '}')
									catch_counter--;
							}
							if(catch_counter == 0)
								break;
						}						
						break;
					case ".locals":
						bool endlocals = false;
						while(endlocals == false)
						{
							if(str.IndexOf("(") >= 0)
								str = str.Substring(str.IndexOf("(") + 1);
							string s2;
							if(str.IndexOf(",") >= 0)
								s2 = str.Substring(0, str.IndexOf(","));
							else if(str.IndexOf(")") >= 0)
							{
								s2 = str.Substring(0, str.IndexOf(")"));
								endlocals = true;
							}
							else
							{
								Console.WriteLine("Unknown local variable declaration {0}", str);
								throw new Exception("Unknown local variable declaration");
							}
							themethod.AddLocalVariable(ParseLocalVariable(s2));
							if(endlocals == false)
								str = GetNextLine();
						}
						break;
					case "}":
						if(try_counter == 0)
							done = true;
						else
							try_counter--;
						break;
					case ".entrypoint":
						program.EntryPoint = themethod;
						break;
					default:
						string label = str.Substring(0, str.IndexOf(":"));
						CILInstruction inst = ParseInstruction(label, str.Substring(10, str.Length - 10));
						if(inst != null)
							themethod.AddInstruction(inst);
						break;
				}
			}
			return themethod;
		}

		private int ParseInt32(string str)
		{
			if(str.StartsWith("0x"))
			{
				int ret = 0;
				for(int i = 2; i < str.Length; i++)
				{
					int x;
					if(('0' <= str[i]) && (str[i] <= '9'))
						x = str[i] - '0';
					else
						x = str[i] - 'A' + 10;
					ret = ret * 16 + x;
				}
				return ret;
			}
			else
			{
				return Int32.Parse(str);
			}
		}

		CILInstruction ParseInstruction(string label, string str)
		{
            if (str.StartsWith("ldc.i4"))
            {
                // ldc family
                if ((str[7] >= '0') && (str[7] <= '8'))
                {
                    return new CIL_ldc_i4(label, str[7] - '0');
                }
                else if ((str[7] == 'm') || (str[7] == 'M'))
                {
                    return new CIL_ldc_i4(label, -1);
                }
                else if (str[7] == ' ')
                {
                    // CHECK be careful with the range
                    return new CIL_ldc_i4(label, ParseInt32(str.Substring(11)));
                }
                else if (str[7] == 's')
                {
                    return new CIL_ldc_i4(label, ParseInt32(str.Substring(11)));
                }
                else
                {
                    throw new Exception("unknown ldc");
                }
            }
            else if (str.StartsWith("ldnull"))
            {
                return new CIL_ldnull(label);
            }
            else if (str.StartsWith("ldc.r8"))
                return ParseLdcR8(label, str);
            else if (str.StartsWith("newobj"))
                return ParseNewobj(label, str);
            else if (str.StartsWith("newarr"))
                return ParseNewarr(label, str);
            else if (str.StartsWith("ldftn"))
                return ParseLdftn(label, str);
            else if (str.StartsWith("callvirt"))
                return ParseCallvirt(label, str);
            else if (str.StartsWith("call "))
                return ParseCall(label, str);
            else if (str.StartsWith("stloc"))
                return ParseStloc(label, str);
            else if (str.StartsWith("ldloc"))
                return ParseLdloc(label, str);
            else if (str.StartsWith("br.s"))
                return ParseBr(label, str);
            else if (str.StartsWith("br "))
                return ParseBr(label, str);
            else if (str.StartsWith("brtrue "))
                return ParseBrtrue(label, str);
            else if (str.StartsWith("brtrue.s"))
                return ParseBrtrue(label, str);
            else if (str.StartsWith("brfalse "))
                return ParseBrfalse(label, str);
            else if (str.StartsWith("brfalse.s "))
                return ParseBrfalse(label, str);
            else if (str.StartsWith("blt "))
                return ParseBlt(label, str);
            else if (str.StartsWith("blt.s"))
                return ParseBlt(label, str);
            else if (str.StartsWith("bgt "))
                return ParseBgt(label, str);
            else if (str.StartsWith("bgt.s"))
                return ParseBgt(label, str);
            else if (str.StartsWith("bge "))
                return ParseBge(label, str);
            else if (str.StartsWith("bge.s"))
                return ParseBge(label, str);
            else if (str.StartsWith("bge.un.s")) // TODO: careful about .un
                return ParseBge(label, str);
            else if (str.StartsWith("ble "))
                return ParseBle(label, str);
            else if (str.StartsWith("ble.s"))
                return ParseBle(label, str);
            else if (str.StartsWith("ble.un.s"))
                return ParseBle(label, str);
            else if (str.StartsWith("beq "))
                return ParseBeq(label, str);
            else if (str.StartsWith("beq.s"))
                return ParseBeq(label, str);
            else if (str.StartsWith("ldarg"))
                return ParseLdarg(label, str);
            else if (str.StartsWith("starg"))
                return ParseStarg(label, str);
            else if (str.StartsWith("stfld"))
                return ParseStfld(label, str);
            else if (str.StartsWith("ldfld"))
                return ParseLdfld(label, str);
            else if (str.StartsWith("stelem"))
                return new CIL_stelem(label);
            else if (str.StartsWith("ldelem"))
                return new CIL_ldelem(label);
            else if (str.StartsWith("ldlen"))
                return new CIL_ldlen(label);
            else if (str.StartsWith("stsfld"))
                return ParseStsfld(label, str);
            else if (str.StartsWith("ldsfld"))
                return ParseLdsfld(label, str);
            else if (str.StartsWith("bne.un"))
                return ParseBneun(label, str);
            else if (str.StartsWith("leave.s"))
                return ParseLeave(label, str);
            else if (str.StartsWith("dup"))
                return new CIL_dup(label);
            else if (str.StartsWith("pop"))
                return new CIL_pop(label);
            else if (str.StartsWith("add"))
                return new CIL_add(label);
            else if (str.StartsWith("sub"))
                return new CIL_sub(label);
            else if (str.StartsWith("mul"))
                return new CIL_mul(label);
            else if (str.StartsWith("div"))
                return new CIL_div(label);
            else if (str.StartsWith("rem"))
                return new CIL_rem(label);
            else if (str.StartsWith("shr"))
                return new CIL_shr(label);
            else if (str.StartsWith("shl"))
                return new CIL_shl(label);
            else if (str.StartsWith("and"))
                return new CIL_and(label);
            else if (str.StartsWith("xor"))
                return new CIL_xor(label);
            else if (str.StartsWith("or"))
                return new CIL_or(label);
            else if (str.StartsWith("neg"))
                return new CIL_neg(label);
            else if (str.StartsWith("ceq"))
                return new CIL_ceq(label);
            else if (str.StartsWith("clt"))
                return new CIL_clt(label);
            else if (str.StartsWith("cgt"))
                return new CIL_cgt(label);
            else if (str.StartsWith("ret"))
                return new CIL_ret(label);
            else if (str.StartsWith("endfinally"))
                return new CIL_endfinally(label);
            else if (str.StartsWith("nop"))
                return new CIL_nop(label);
            else if (str.StartsWith("conv.r8"))
                return new CIL_conv_r8(label);
            else if (str.StartsWith("conv.i8"))
                return new CIL_conv_i8(label);
            else if (str.StartsWith("conv.i4"))
                return new CIL_conv_i4(label);
            else if (str.StartsWith("volatile."))
            {
                nextAccessIsVolatile = true;
                return null;
            }
            else
                throw new Exception("Unsupported instruction type " + str);
		}

		CIL_ldc_r8 ParseLdcR8(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken(); // skip the ldc.r8
			return new CIL_ldc_r8(label, Double.Parse(st.NextToken()));
		}

		CIL_newobj ParseNewobj(string label, string str)
		{
			Strtok st1 = new Strtok(str);
			if(st1.NextToken() != "newobj")
				throw new Exception("unrecognized newobj");
			if(st1.NextToken() != "instance")
				throw new Exception("unrecognized newobj");
			if(st1.NextToken() != "void")
				throw new Exception("unrecognized newobj");

			// get the classname, it's just before ::
			string classname = st1.NextToken(":");
			if(st1.NextToken(":(") != ".ctor")
				throw new Exception("unrecognized newobj");

			// find the constructor signature 
			string ctorsig = "";
			ctorsig += "(";
			int i;
			i = str.IndexOf("(") + 1;

			while(true)
			{
				if(i < str.Length)
				{
					if(str[i] == ')')
						break;
					ctorsig += str[i];
					i++;
				}
				else
				{
					i = 0;
					str = GetNextLine();
				}
			}
			ctorsig += ")";

			return new CIL_newobj(label, program.GetClass(classname), ctorsig);
		}

		CIL_newarr ParseNewarr(string label, string str)
		{
			CILScanner scanner = new CILScanner(fin, str);
			if(scanner.NextToken() != "newarr")
				throw new Exception("Unknown exception");
			string elementType = scanner.NextToken();
			switch(elementType)
			{					
				case "[mscorlib]System.Int32":
					return new CIL_newarr(label, new CILVar_int32(""));
				case "[mscorlib]System.Int64":
					return new CIL_newarr(label, new CILVar_int64(""));
				case "[mscorlib]System.Double":
					return new CIL_newarr(label, new CILVar_double(""));
				default:
					throw new Exception("Unknown array type");			
			}
		}

		CIL_ldftn ParseLdftn(string label, string str)
		{
			Strtok st1 = new Strtok(str);
			if(st1.NextToken() != "ldftn")
				throw new Exception("unrecognized ldftn");
			string tmp = st1.NextToken();
			if(tmp == "instance")
			{
				tmp = st1.NextToken();
				if(tmp != "void")
					throw new Exception("unrecognized ldftn");
			}else
				if(tmp != "void")
					throw new Exception("unrecognized ldftn");

			// get the classname, it's just before ::
			string classname = st1.NextToken(":");

			// find the constructor signature 
			string methodsig = st1.NextToken(":(");
			methodsig += "(";
			int i;
			i = str.IndexOf("(") + 1;

			while(true)
			{
				if(i < str.Length)
				{
					if(str[i] == ')')
						break;
					methodsig += str[i];
					i++;
				}
				else
				{
					i = 0;
					str = GetNextLine();
				}
			}
			methodsig += ")";

			CILClass theclass = program.GetClass(classname);
			CILMethod themethod = theclass.GetMethod(
				methodsig.Substring(0, methodsig.IndexOf("(")), 
				methodsig.Substring(methodsig.IndexOf("("), methodsig.Length - methodsig.IndexOf("(")));
			return new CIL_ldftn(label, themethod);
		}

		CIL_call ParseCall(string label, string str)
		{
			// TODO: Need clean up and reparse using Scanner
			CILScanner scanner = new CILScanner(fin, str);
			string st = scanner.NextToken();
			if(st != "call")
				throw new Exception("unrecognized call");

			string className = null;
			string methodName = null;
			while(className == null)
			{
				st = scanner.NextToken();
				if((st == "int32") || (st == "int64") || 
					(st == "float64") || 
					(st == "void") | (st == "bool") || 
					(st == "class"))
				{
					if(st == "class")
						scanner.NextToken();
					className = scanner.NextToken();
					scanner.NextToken();
					methodName = scanner.NextToken();
				}
			}

			string methodSig = "(";
			scanner.NextToken(); // skips "(";
			int paramCount = 0;
			while(true)
			{
				st = scanner.NextToken();
				if(st == ")")
					break;
				if(st != ",")
				{
					if(st == "class")
					{						
						paramCount++;
						if(paramCount > 1)
							methodSig += ",";
						methodSig += scanner.NextToken();					
					}
					else
					{
						paramCount++;
						if(paramCount > 1)
							methodSig += ",";
						methodSig += st;					
					}
				}
			}
			methodSig += ")";

			CILClass theclass = program.GetClass(className);
			CILMethod themethod = theclass.GetMethod(methodName, methodSig);				

			return new CIL_call(label, themethod);
		}

		CIL_callvirt ParseCallvirt(string label, string str)
		{
			// TODO: Need cleanup and reparse using Scanner
			CILScanner scanner = new CILScanner(fin, str);
			string st = scanner.NextToken();
			if(st != "callvirt")
				throw new Exception("unrecognized call");

			string className = null;
			string methodName = null;
			while(className == null)
			{
				st = scanner.NextToken();
				if((st == "int32") || (st == "int64") || (st == "void") || (st == "class"))
				{
					if(st == "class")
						scanner.NextToken();
					className = scanner.NextToken();
					scanner.NextToken();
					methodName = scanner.NextToken();
				}
			}

			string methodSig = "(";
			scanner.NextToken(); // skips "(";
			int paramCount = 0;
			while(true)
			{
				st = scanner.NextToken();
				if(st == ")")
					break;
				if(st != ",")
				{
					if(st == "class")
					{						
						paramCount++;
						if(paramCount == 2)
							methodSig += ",";
						methodSig += scanner.NextToken();					
					}
					else
						methodSig += st;					
				}
			}
			methodSig += ")";

			CILClass theclass = program.GetClass(className);
			CILMethod themethod = theclass.GetMethod(methodName, methodSig);				

			return new CIL_callvirt(label, themethod);
		}

		CIL_bne_un ParseBneun(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			return new CIL_bne_un(label, st.NextToken());
		}

		CIL_blt ParseBlt(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			return new CIL_blt(label, st.NextToken());
		}

		CIL_bgt ParseBgt(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			return new CIL_bgt(label, st.NextToken());
		}

		CIL_bge ParseBge(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			return new CIL_bge(label, st.NextToken());
		}

		CIL_ble ParseBle(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			return new CIL_ble(label, st.NextToken());
		}

		CIL_beq ParseBeq(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			return new CIL_beq(label, st.NextToken());
		}

		CIL_brtrue ParseBrtrue(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			return new CIL_brtrue(label, st.NextToken());
		}

		CIL_brfalse ParseBrfalse(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			return new CIL_brfalse(label, st.NextToken());
		}

		CIL_ldfld ParseLdfld(string label, string str)
		{
			CILScanner scanner = new CILScanner(fin, str);
			if(scanner.NextToken() != "ldfld")
				throw new Exception("parse error: ldfld expected");
			string fieldtype = scanner.NextToken();
			if(fieldtype == "class")
				fieldtype = "class " + scanner.NextToken();
			string classname = scanner.NextToken();
			if(classname == "modreq")
			{
				scanner.NextToken(); // skip "["
				scanner.NextToken(); // skip "mscorlib...IsVolatile"
				scanner.NextToken(); // skip "]"
				classname = scanner.NextToken();
			}
			scanner.NextToken(); // skip "::";
			string fieldname = scanner.NextToken();
			if(nextAccessIsVolatile)
			{
				nextAccessIsVolatile = false;
				return new CIL_ldfld(label, fieldtype, classname, fieldname, true);
			}
			else
				return new CIL_ldfld(label, fieldtype, classname, fieldname, false);
		}

		CIL_stfld ParseStfld(string label, string str)
		{
			CILScanner scanner = new CILScanner(fin, str);
			if(scanner.NextToken() != "stfld")
				throw new Exception("parse error: stfld expected");
			string fieldtype = scanner.NextToken();
			if(fieldtype == "class")
				fieldtype = "class " + scanner.NextToken();
			string classname = scanner.NextToken();
			if(classname == "modreq")
			{
				scanner.NextToken(); // skip "["
				scanner.NextToken(); // skip "mscorlib...IsVolatile"
				scanner.NextToken(); // skip "]"
				classname = scanner.NextToken();
			}
			scanner.NextToken(); // skip "::";
			string fieldname = scanner.NextToken();
			if(nextAccessIsVolatile)
			{
				nextAccessIsVolatile = false;
				return new CIL_stfld(label, fieldtype, classname, fieldname, true);
			}else
				return new CIL_stfld(label, fieldtype, classname, fieldname, false);
		}

		CIL_stsfld ParseStsfld(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			string fieldtype = st.NextToken();
			string subFieldType = null;
			if(fieldtype == "class")
				subFieldType = st.NextToken();
			string classname = st.NextToken(":");
			string fieldname = st.NextToken(":");
			CILClass classType = program.GetClass(classname);
			CILVariable field;
			if(fieldtype == "int32")
				field = new CILVar_int32(fieldname);
			else if(fieldtype == "int64")
				field = new CILVar_int64(fieldname);
			else if(fieldtype == "float64")
				field = new CILVar_double(fieldname);
			else if(fieldtype == "int32[]")
				field = new CILVar_array(fieldname, new CILVar_int32(""));
			else if(fieldtype == "int64[]")
				field = new CILVar_array(fieldname, new CILVar_int64(""));
			else if(fieldtype == "float64[]")
				field = new CILVar_array(fieldname, new CILVar_double(""));
			else if(fieldtype == "class")
				field = new CILVar_object(fieldname, program.GetClass(subFieldType));
			else
				throw new Exception("Not implemented yet");
			return new CIL_stsfld(label, classType, field);
		}

		CIL_ldsfld ParseLdsfld(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			string fieldtype = st.NextToken();
			string subFieldType = null;
			if(fieldtype == "class")
				subFieldType = st.NextToken();
			string classname = st.NextToken(":");
			string fieldname = st.NextToken(":");
			CILClass classType = program.GetClass(classname);
			CILVariable field;
			if(fieldtype == "int32")
				field = new CILVar_int32(fieldname);
			else if(fieldtype == "int64")
				field = new CILVar_int64(fieldname);
			else if(fieldtype == "float64")
				field = new CILVar_double(fieldname);
			else if(fieldtype == "int32[]")
				field = new CILVar_array(fieldname, new CILVar_int32(""));
			else if(fieldtype == "int64[]")
				field = new CILVar_array(fieldname, new CILVar_int64(""));
			else if(fieldtype == "float64[]")
				field = new CILVar_array(fieldname, new CILVar_double(""));
			else if(fieldtype == "class")
				field = new CILVar_object(fieldname, program.GetClass(subFieldType));
			else
				throw new Exception("Not implemented yet");
			return new CIL_ldsfld(label, classType, field);
		}

		CIL_br ParseBr(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
            return new CIL_br(label, st.NextToken());
		}

		CIL_leave ParseLeave(string label, string str)
		{
			Strtok st = new Strtok(str);
			st.NextToken();
			return new CIL_leave(label, st.NextToken());
		}

		CIL_stloc ParseStloc(string label, string str)
		{
			int index;

			if(str[5] == ' ')
			{
				index = ParseInt32(str.Substring(6));
			}
			else if(str[6] == 's')
			{
				string name = str.Substring(11);
				index = currentMethod.GetLocalVariableIndex(name);
			}
			else
			{
				index = str[6] - '0';
			}
			return new CIL_stloc(label, index);
		}

		CIL_ldloc ParseLdloc(string label, string str)
		{
			int index;

			if(str[5] == ' ')
			{
				index = ParseInt32(str.Substring(6));
			}
			else if(str[6] == 's')
			{
				string name = str.Substring(11);
				index = currentMethod.GetLocalVariableIndex(name);
			}
			else
			{
				index = str[6] - '0';
			}
			return new CIL_ldloc(label, index);
		}

		CIL_ldarg ParseLdarg(string label, string str)
		{
			int index;

			if(str[5] == ' ')
			{
				index = ParseInt32(str.Substring(6));
			}
			else if(str[6] == 's')
			{
				index = currentMethod.GetArgumentIndex(str.Substring(11));
				// the first arg is the object pointer
				if(currentMethod.IsStatic == false)
					index++;
			}
			else
			{
				index = str[6] - '0';
			}
			return new CIL_ldarg(label, index);
		}

		CIL_starg ParseStarg(string label, string str)
		{
			int index;

			if(str[5] == ' ')
			{
				index = ParseInt32(str.Substring(6));
			}
			else if(str[6] == 's')
			{
				index = currentMethod.GetArgumentIndex(str.Substring(11));
				// the first arg is the object pointer
				if(currentMethod.IsStatic == false)
					index++;
			}
			else
			{
				index = str[6] - '0';
			}
			return new CIL_starg(label, index);
		}

		void ParseClass(string str)
		{
			string[] classdetails = str.Split(' ');
			// the last word of str contains the classname
			CILClass theclass = program.GetClass(classdetails[classdetails.Length - 1]);
			bool done = false;

			while(done == false)
			{
				str = GetNextLine();
				string[] s2 = str.Split(' ');
				switch(s2[0])
				{
					case "{":
						// ignore
						// should have stopped processing extends if seen this {
						break;
					case "extends":
						// TODO
						// now only know if it extends one class
						theclass.AddParentClass(s2[1]);
						break;
					case ".field":
						bool isStatic = false;
						int fi = -1; // first place to check for type
						for(int i = 0; i < s2.Length; i++)
						{
							if(s2[i] == "static")
								isStatic = true;
							if(
								(s2[i] != "public") && (s2[i] != "private") && (s2[i] != "protected") && (s2[i] != "internal")
								&& (s2[i] != "static") && (s2[i] != ".field") && (fi == -1))
								fi = i;
						}
						
						if(s2[s2.Length - 1][0] == '\'')
							s2[s2.Length - 1] = s2[s2.Length - 1].Substring(1, s2[s2.Length - 1].Length - 2);
						switch(s2[fi])
						{
							case "int32":
								theclass.AddField(new CILClassField(new CILVar_int32(s2[s2.Length - 1]), isStatic));
								break;
							case "int64":
								theclass.AddField(new CILClassField(new CILVar_int64(s2[s2.Length - 1]), isStatic));
								break;
							case "float64":
								theclass.AddField(new CILClassField(new CILVar_double(s2[s2.Length - 1]), isStatic));
								break;
							case "int32[]":
								theclass.AddField(new CILClassField(new CILVar_array(s2[s2.Length - 1], new CILVar_int32("")), isStatic));
								break;	
							case "int64[]":
								theclass.AddField(new CILClassField(new CILVar_array(s2[s2.Length - 1], new CILVar_int64("")), isStatic));
								break;	
							case "float64[]":
								theclass.AddField(new CILClassField(new CILVar_array(s2[s2.Length - 1], new CILVar_double("")), isStatic));
								break;	
							default:
								theclass.AddField(new CILClassField(new CILVar_object(s2[s2.Length - 1], program.GetClass(s2[s2.Length - 2])), isStatic));
//								Console.WriteLine("Unknown field type {0}", s2[s2.Length - 2]);
								break;
						}
						break;
					case ".method":
						ParseMethod(theclass, str);
						break;
					case "}":
						done = true;
						break;
					default:
						Console.WriteLine("Unknown class symbol {0}", s2[0]);						
						break;
				}
			}

			theclass.IsInitialized = true;
		}

		void ParseModule(string str)
		{
			// do nothing
		}

		void ParseImageBase(string str)
		{
			// do nothing
		}

		void ParseSubsystem(string str)
		{
			// do nothing
		}

		void ParseFile(string str)
		{
			// do nothing
		}

		void ParseCorflags(string str)
		{
			// do nothing
		}

		public CILProgram Parse(string filename)
		{
			program = new CILProgram();
			fin = new StreamReader(filename);			
			string str, s1;
			while(true)
			{
				str = GetNextLine();
				if(str == null)
					break;
				
				s1 = str.Substring(0, str.IndexOf(" "));
				switch(s1)
				{
					case ".assembly":
						ParseAssembly(str);
						break;
                    case ".stackreserve":
                        ParseStackReserve(str);
                        break;
					case ".module":
						ParseModule(str);
						break;
					case ".imagebase":
						ParseImageBase(str);
						break;
					case ".subsystem":
						ParseSubsystem(str);
						break;
					case ".file":
						ParseFile(str);
						break;
					case ".corflags":
						ParseCorflags(str);
						break;
					case ".class":
						ParseClass(str);
						break;
					default:
						Console.WriteLine("Unknown symbol {0}", s1);						
						break;
				}
			}
			fin.Close();
			return program;
		}
	}
}
