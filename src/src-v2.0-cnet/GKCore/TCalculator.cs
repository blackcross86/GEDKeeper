using GKSys;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GKCore
{
	public class TCalculator : MarshalByRefObject, IDisposable
	{

		public class ECalculate : Exception
		{
			public ECalculate()
			{
			}

			public ECalculate(string message) : base(message)
			{
			}

			public ECalculate(string message, Exception innerException) : base(message, innerException)
			{
			}
		}

		public class TNamedVar
		{
			public string Name;
			public double Value;
			public void Free()
			{
				TObjectHelper.Free(this);
			}
		}

		internal enum TCalcCBType : byte
		{
			ctGetValue,
			ctSetValue,
			ctFunction
		}

		internal enum TToken : byte
		{
			tkEOF,
			tkERROR,
			tkASSIGN,
			tkLBRACE,
			tkRBRACE,
			tkNUMBER,
			tkIDENT,
			tkSEMICOLON,
			tkPOW,
			tkINV,
			tkNOT,
			tkMUL,
			tkDIV,
			tkMOD,
			tkPER,
			tkADD,
			tkSUB,
			tkLT,
			tkLE,
			tkEQ,
			tkNE,
			tkGE,
			tkGT,
			tkOR,
			tkXOR,
			tkAND
		}

		internal double fvalue;
		internal string svalue;
		internal string FExpression;
		internal int FPtr;
		internal TCalculator.TToken FToken;
		internal TObjectList FVars;
		public static readonly int C_NOERROR;
		public static readonly int C_ERR_BRACKETS;
		public static readonly int C_ERR_EMPTY_EXPR;
		public static readonly int C_ERR_UNKNOWN_FUNC;
		public static readonly int C_ERR_FUNCTION_ERR;
		public static readonly int C_ERR_DEV_BY_ZERO;
		public static readonly int C_ERR_CANNOT_SOLVE;
		public static readonly int C_ERR_BAD_NUMBER;
		protected internal bool Disposed_;

		/*public double Vars[string Name]
		{
			get { return this.GetVar(Name); }
			set { this.SetVar(Name, Value); }
		}*/

		internal void RaiseError([In] string Msg)
		{
			throw new TCalculator.ECalculate(Msg);
		}
		internal double tofloat(bool B)
		{
			double Result;
			if (B)
			{
				Result = 1.0;
			}
			else
			{
				Result = 0.0;
			}
			return Result;
		}
		internal double fmod(double x, double y)
		{
			return (x - BDSSystem.Int((x / y)) * y);
		}
		internal string CalcErrorToStr(ushort Error)
		{
			string Result;
			if ((int)Error == TCalculator.C_NOERROR)
			{
				Result = "No error";
			}
			else
			{
				Result = "Unknown error";
			}
			return Result;
		}
		internal bool DefCalcProc(TCalculator.TCalcCBType ctype, [In] string S, ref double V)
		{
			bool Result = true;
			if (ctype != TCalculator.TCalcCBType.ctGetValue)
			{
				if (ctype != TCalculator.TCalcCBType.ctSetValue)
				{
					if (ctype == TCalculator.TCalcCBType.ctFunction)
					{
						if (BDSSystem.WStrCmp(S, "round") == 0)
						{
							V = ((double)checked((long)Math.Round(V)));
						}
						else
						{
							if (BDSSystem.WStrCmp(S, "trunc") == 0)
							{
								V = ((double)BDSSystem.Trunc(V));
							}
							else
							{
								if (BDSSystem.WStrCmp(S, "int") == 0)
								{
									V = BDSSystem.Int(V);
								}
								else
								{
									if (BDSSystem.WStrCmp(S, "frac") == 0)
									{
										V = BDSSystem.Frac(V);
									}
									else
									{
										if (BDSSystem.WStrCmp(S, "sin") == 0)
										{
											V = Math.Sin(V);
										}
										else
										{
											if (BDSSystem.WStrCmp(S, "cos") == 0)
											{
												V = Math.Cos(V);
											}
											else
											{
												if (BDSSystem.WStrCmp(S, "tan") == 0)
												{
													V = Math.Tan(V);
												}
												else
												{
													if (BDSSystem.WStrCmp(S, "atan") == 0)
													{
														V = Math.Atan(V);
													}
													else
													{
														if (BDSSystem.WStrCmp(S, "ln") == 0)
														{
															V = Math.Log(V);
														}
														else
														{
															if (BDSSystem.WStrCmp(S, "exp") == 0)
															{
																V = Math.Exp(V);
															}
															else
															{
																if (BDSSystem.WStrCmp(S, "sign") == 0)
																{
																	if (V > (double)0f)
																	{
																		V = 1.0;
																	}
																	else
																	{
																		if (V < (double)0f)
																		{
																			V = -1.0;
																		}
																	}
																}
																else
																{
																	if (BDSSystem.WStrCmp(S, "sgn") == 0)
																	{
																		if (V > (double)0f)
																		{
																			V = 1.0;
																		}
																		else
																		{
																			if (V < (double)0f)
																			{
																				V = 0.0;
																			}
																		}
																	}
																	else
																	{
																		if (BDSSystem.WStrCmp(S, "xsgn") == 0)
																		{
																			if (V < (double)0f)
																			{
																				V = 0.0;
																			}
																		}
																		else
																		{
																			Result = false;
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				else
				{
					Result = false;
				}
			}
			else
			{
				if (BDSSystem.WStrCmp(S, "pi") == 0)
				{
					V = 3.1415926535897931;
				}
				else
				{
					if (BDSSystem.WStrCmp(S, "e") == 0)
					{
						V = 2.718281828;
					}
					else
					{
						Result = false;
					}
				}
			}
			return Result;
		}
		internal bool Callback(TCalculator.TCalcCBType ctype, [In] string Name, ref double Res)
		{
			bool Result = this.DefCalcProc(ctype, Name, ref Res);
			if (!Result)
			{
				Result = true;
				if (ctype != TCalculator.TCalcCBType.ctGetValue)
				{
					if (ctype != TCalculator.TCalcCBType.ctSetValue)
					{
						if (ctype == TCalculator.TCalcCBType.ctFunction)
						{
							Result = false;
						}
					}
					else
					{
						this.SetVar(Name, Res);
					}
				}
				else
				{
					Res = this.GetVar(Name);
				}
			}
			return Result;
		}
		internal double GetVar([In] string Name)
		{
			int arg_0F_0 = 0;
			int num = this.FVars.Count - 1;
			int i = arg_0F_0;
			if (num >= i)
			{
				num++;
				TCalculator.TNamedVar V;
				while (true)
				{
					V = (this.FVars[i] as TCalculator.TNamedVar);
					if (VCLUtils.CompareStr(V.Name, Name) == 0)
					{
						break;
					}
					i++;
					if (i == num)
					{
						goto IL_49;
					}
				}
				return V.Value;
			}
			IL_49:
			throw new TCalculator.ECalculate("Unknown function or variable \"" + Name + "\".");
		}
		internal void SetVar([In] string Name, double Value)
		{
			TCalculator.TNamedVar V = null;
			int arg_11_0 = 0;
			int num = this.FVars.Count - 1;
			int i = arg_11_0;
			if (num >= i)
			{
				num++;
				while (VCLUtils.CompareStr((this.FVars[i] as TCalculator.TNamedVar).Name, Name) != 0)
				{
					i++;
					if (i == num)
					{
						goto IL_54;
					}
				}
				V = (this.FVars[i] as TCalculator.TNamedVar);
			}
			IL_54:
			if (V == null)
			{
				V = new TCalculator.TNamedVar();
				V.Name = Name;
				this.FVars.Add(V);
			}
			V.Value = Value;
		}
		internal void lex()
		{
			while (this.FExpression[this.FPtr - 1] != '\0' && this.FExpression[this.FPtr - 1] <= ' ')
			{
				this.FPtr++;
			}
			this.FToken = TCalculator.TToken.tkEOF;
			if (this.FExpression[this.FPtr - 1] != '\0')
			{
				int s_pos = this.FPtr;
				this.FToken = TCalculator.TToken.tkNUMBER;
				if (this.FExpression[this.FPtr - 1] == '$')
				{
					this.FPtr++;
					while (true)
					{
						char c2 = this.FExpression[this.FPtr - 1];
						if (c2 < '0' || (c2 >= ':' && (c2 < 'A' || (c2 >= 'I' && (c2 < 'a' || c2 >= 'i')))))
						{
							break;
						}
						this.FPtr++;
					}
					if (TCalculator._lex_ConvertNumber(this, s_pos, this.FPtr, 16))
					{
						return;
					}
				}
				else
				{
					char c3 = this.FExpression[this.FPtr - 1];
					if (c3 >= '0' && c3 < ':')
					{
						if (this.FExpression[this.FPtr - 1] == '0')
						{
							this.FPtr++;
							char c4 = this.FExpression[this.FPtr - 1];
							if (c4 == 'X' || c4 == 'x')
							{
								this.FPtr++;
								s_pos = this.FPtr;
								while (true)
								{
									char c5 = this.FExpression[this.FPtr - 1];
									if (c5 < '0' || (c5 >= ':' && (c5 < 'A' || (c5 >= 'I' && (c5 < 'a' || c5 >= 'i')))))
									{
										break;
									}
									this.FPtr++;
								}
								if (TCalculator._lex_ConvertNumber(this, s_pos, this.FPtr, 16))
								{
									return;
								}
								goto IL_B09;
							}
							else
							{
								char c6 = this.FExpression[this.FPtr - 1];
								if (c6 == 'B' || c6 == 'b')
								{
									this.FPtr++;
									s_pos = this.FPtr;
									while (true)
									{
										char c7 = this.FExpression[this.FPtr - 1];
										if (c7 < '0' || c7 >= '2')
										{
											break;
										}
										this.FPtr++;
									}
									if (TCalculator._lex_ConvertNumber(this, s_pos, this.FPtr, 2))
									{
										return;
									}
									goto IL_B09;
								}
							}
						}
						while (true)
						{
							char c8 = this.FExpression[this.FPtr - 1];
							if (c8 < '0' || (c8 >= ':' && (c8 < 'A' || (c8 >= 'G' && (c8 < 'a' || c8 >= 'g')))))
							{
								break;
							}
							this.FPtr++;
						}
						char c9 = this.FExpression[this.FPtr - 1];
						if (c9 == 'H' || c9 == 'h')
						{
							if (TCalculator._lex_ConvertNumber(this, s_pos, this.FPtr, 16))
							{
								this.FPtr++;
								return;
							}
						}
						else
						{
							char c10 = this.FExpression[this.FPtr - 1];
							if (c10 == 'B' || c10 == 'b')
							{
								if (TCalculator._lex_ConvertNumber(this, s_pos, this.FPtr, 2))
								{
									this.FPtr++;
									return;
								}
							}
							else
							{
								if (TCalculator._lex_ConvertNumber(this, s_pos, this.FPtr, 10))
								{
									if (this.FExpression[this.FPtr - 1] == '`')
									{
										this.fvalue = (this.fvalue * 3.1415926535897931 / 180.0);
										this.FPtr++;
										double frac = 0.0;
										while (true)
										{
											char c11 = this.FExpression[this.FPtr - 1];
											if (c11 < '0' || c11 >= ':')
											{
												break;
											}
											frac = (frac * 10.0 + (double)((int)this.FExpression[this.FPtr - 1] - 48));
											this.FPtr++;
										}
										this.fvalue = (this.fvalue + frac * 3.1415926535897931 / 180.0 / 60.0);
										if (this.FExpression[this.FPtr - 1] == '`')
										{
											this.FPtr++;
											frac = 0.0;
											while (true)
											{
												char c12 = this.FExpression[this.FPtr - 1];
												if (c12 < '0' || c12 >= ':')
												{
													break;
												}
												frac = (frac * 10.0 + (double)((int)this.FExpression[this.FPtr - 1] - 48));
												this.FPtr++;
											}
											this.fvalue = (this.fvalue + frac * 3.1415926535897931 / 180.0 / 60.0 / 60.0);
										}
										this.fvalue = this.fmod(this.fvalue, 6.2831853071795862);
										return;
									}
									if (this.FExpression[this.FPtr - 1] == '.')
									{
										this.FPtr++;
										double frac = 1.0;
										while (true)
										{
											char c13 = this.FExpression[this.FPtr - 1];
											if (c13 < '0' || c13 >= ':')
											{
												break;
											}
											frac = (frac / 10.0);
											this.fvalue = (this.fvalue + frac * (double)((int)this.FExpression[this.FPtr - 1] - 48));
											this.FPtr++;
										}
									}
									char c14 = this.FExpression[this.FPtr - 1];
									if (c14 != 'E' && c14 != 'e')
									{
										return;
									}
									this.FPtr++;
									int exp = 0;
									char sign = this.FExpression[this.FPtr - 1];
									char c15 = this.FExpression[this.FPtr - 1];
									if (c15 == '+' || c15 == '-')
									{
										this.FPtr++;
									}
									char c16 = this.FExpression[this.FPtr - 1];
									if (c16 >= '0' && c16 < ':')
									{
										while (true)
										{
											char c17 = this.FExpression[this.FPtr - 1];
											if (c17 < '0' || c17 >= ':')
											{
												break;
											}
											exp = exp * 10 + (int)this.FExpression[this.FPtr - 1] - 48;
											this.FPtr++;
										}
										if (exp == 0)
										{
											this.fvalue = 1.0;
											return;
										}
										if (sign == '-')
										{
											if (exp > 0)
											{
												do
												{
													this.fvalue = (this.fvalue * 10.0);
													exp--;
												}
												while (exp > 0);
												return;
											}
											return;
										}
										else
										{
											if (exp > 0)
											{
												do
												{
													this.fvalue = (this.fvalue / 10.0);
													exp--;
												}
												while (exp > 0);
												return;
											}
											return;
										}
									}
								}
							}
						}
					}
					else
					{
						char c18 = this.FExpression[this.FPtr - 1];
						if (c18 >= 'A' && (c18 < '[' || c18 == '_' || (c18 >= 'a' && c18 < '{')))
						{
							this.svalue = BDSSystem.WStrFromWChar(this.FExpression[this.FPtr - 1]);
							this.FPtr++;
							while (true)
							{
								char c19 = this.FExpression[this.FPtr - 1];
								if (c19 < '0' || (c19 >= ':' && (c19 < 'A' || (c19 >= '[' && c19 != '_' && (c19 < 'a' || c19 >= '{')))))
								{
									break;
								}
								string text = this.svalue;
								if (((text != null) ? text.Length : 0) >= 32)
								{
									break;
								}
								this.svalue += this.FExpression[this.FPtr - 1];
								this.FPtr++;
							}
							this.FToken = TCalculator.TToken.tkIDENT;
							return;
						}
						char c = this.FExpression[this.FPtr - 1];
						this.FPtr++;
						switch (c)
						{
							case '!':
							{
								this.FToken = TCalculator.TToken.tkNOT;
								if (this.FExpression[this.FPtr - 1] == '=')
								{
									this.FPtr++;
									this.FToken = TCalculator.TToken.tkNE;
									return;
								}
								return;
							}
							case '"':
							case '#':
							case '$':
							case '\'':
							case ',':
							case '.':
							case '0':
							case '1':
							case '2':
							case '3':
							case '4':
							case '5':
							case '6':
							case '7':
							case '8':
							case '9':
							case ':':
							{
								IL_898:
								if (c == '^')
								{
									this.FToken = TCalculator.TToken.tkXOR;
									return;
								}
								if (c == '|')
								{
									this.FToken = TCalculator.TToken.tkOR;
									return;
								}
								if (c != '~')
								{
									this.FToken = TCalculator.TToken.tkERROR;
									this.FPtr--;
									return;
								}
								this.FToken = TCalculator.TToken.tkINV;
								return;
							}
							case '%':
							{
								this.FToken = TCalculator.TToken.tkMOD;
								if (this.FExpression[this.FPtr - 1] == '%')
								{
									this.FPtr++;
									this.FToken = TCalculator.TToken.tkPER;
									return;
								}
								return;
							}
							case '&':
							{
								this.FToken = TCalculator.TToken.tkAND;
								return;
							}
							case '(':
							{
								this.FToken = TCalculator.TToken.tkLBRACE;
								return;
							}
							case ')':
							{
								this.FToken = TCalculator.TToken.tkRBRACE;
								return;
							}
							case '*':
							{
								this.FToken = TCalculator.TToken.tkMUL;
								if (this.FExpression[this.FPtr - 1] == '*')
								{
									this.FPtr++;
									this.FToken = TCalculator.TToken.tkPOW;
									return;
								}
								return;
							}
							case '+':
							{
								this.FToken = TCalculator.TToken.tkADD;
								return;
							}
							case '-':
							{
								this.FToken = TCalculator.TToken.tkSUB;
								return;
							}
							case '/':
							{
								this.FToken = TCalculator.TToken.tkDIV;
								return;
							}
							case ';':
							{
								this.FToken = TCalculator.TToken.tkSEMICOLON;
								return;
							}
							case '<':
							{
								this.FToken = TCalculator.TToken.tkLT;
								if (this.FExpression[this.FPtr - 1] == '=')
								{
									this.FPtr++;
									this.FToken = TCalculator.TToken.tkLE;
									return;
								}
								if (this.FExpression[this.FPtr - 1] == '>')
								{
									this.FPtr++;
									this.FToken = TCalculator.TToken.tkNE;
									return;
								}
								return;
							}
							case '=':
							{
								this.FToken = TCalculator.TToken.tkASSIGN;
								if (this.FExpression[this.FPtr - 1] == '=')
								{
									this.FPtr++;
									this.FToken = TCalculator.TToken.tkEQ;
									return;
								}
								return;
							}
							case '>':
							{
								this.FToken = TCalculator.TToken.tkGT;
								if (this.FExpression[this.FPtr - 1] == '=')
								{
									this.FPtr++;
									this.FToken = TCalculator.TToken.tkGE;
									return;
								}
								if (this.FExpression[this.FPtr - 1] == '<')
								{
									this.FPtr++;
									this.FToken = TCalculator.TToken.tkNE;
									return;
								}
								return;
							}
						}
						//goto IL_898;
						//alert!!! HOW IS IT?????!!!!
					}
				}
				IL_B09:
				this.FToken = TCalculator.TToken.tkERROR;
			}
		}
		internal void term(ref double R)
		{
			TCalculator.TToken fToken = this.FToken;
			if (fToken != TCalculator.TToken.tkLBRACE)
			{
				if (fToken != TCalculator.TToken.tkNUMBER)
				{
					if (fToken != TCalculator.TToken.tkIDENT)
					{
						this.RaiseError("Syntax error.");
					}
					else
					{
						string st = this.svalue.ToLower();
						this.lex();
						if (this.FToken == TCalculator.TToken.tkLBRACE)
						{
							this.lex();
							this.expr6(ref R);
							if (this.FToken == TCalculator.TToken.tkRBRACE)
							{
								this.lex();
							}
							else
							{
								this.RaiseError("Syntax error");
							}
							if (!this.Callback(TCalculator.TCalcCBType.ctFunction, st, ref R))
							{
								this.RaiseError("Unknown function or variable \"" + st + "\".");
							}
						}
						else
						{
							if (this.FToken == TCalculator.TToken.tkASSIGN)
							{
								this.lex();
								this.expr6(ref R);
								if (!this.Callback(TCalculator.TCalcCBType.ctSetValue, st, ref R))
								{
									this.RaiseError("Unknown function or variable \"" + st + "\".");
								}
							}
							else
							{
								if (!this.Callback(TCalculator.TCalcCBType.ctGetValue, st, ref R))
								{
									this.RaiseError("Unknown function or variable \"" + st + "\".");
								}
							}
						}
					}
				}
				else
				{
					R = this.fvalue;
					this.lex();
				}
			}
			else
			{
				this.lex();
				this.expr6(ref R);
				if (this.FToken == TCalculator.TToken.tkRBRACE)
				{
					this.lex();
				}
				else
				{
					this.RaiseError("Syntax error");
				}
			}
		}
		internal void expr1(ref double R)
		{
			this.term(ref R);
			if (this.FToken == TCalculator.TToken.tkPOW)
			{
				this.lex();
				double V = 0.0;
				this.term(ref V);
				R = Math.Pow(R, V);
			}
		}
		internal void expr2(ref double R)
		{
			TCalculator.TToken fToken = this.FToken;
			if (fToken >= TCalculator.TToken.tkINV && (fToken < TCalculator.TToken.tkMUL || (fToken >= TCalculator.TToken.tkADD && fToken < TCalculator.TToken.tkLT)))
			{
				TCalculator.TToken oldt = this.FToken;
				this.lex();
				this.expr2(ref R);
				if (oldt != TCalculator.TToken.tkINV)
				{
					if (oldt != TCalculator.TToken.tkNOT)
					{
						if (oldt == TCalculator.TToken.tkSUB)
						{
							R = (-R);
						}
					}
					else
					{
						R = this.tofloat(BDSSystem.Trunc(R) <= (long)((ulong)0));
					}
				}
				else
				{
					R = ((double)(~BDSSystem.Trunc(R)));
				}
			}
			else
			{
				this.expr1(ref R);
			}
		}
		internal void expr3(ref double R)
		{
			this.expr2(ref R);
			while (true)
			{
				TCalculator.TToken fToken = this.FToken;
				if (fToken < TCalculator.TToken.tkMUL || fToken >= TCalculator.TToken.tkADD)
				{
					break;
				}
				TCalculator.TToken oldt = this.FToken;
				this.lex();
				double V = 0.0;
				this.expr2(ref V);
				if (oldt != TCalculator.TToken.tkMUL)
				{
					if (oldt != TCalculator.TToken.tkDIV)
					{
						if (oldt != TCalculator.TToken.tkMOD)
						{
							if (oldt == TCalculator.TToken.tkPER)
							{
								R = (R * V / 100.0);
							}
						}
						else
						{
							R = ((double)(BDSSystem.Trunc(R) % BDSSystem.Trunc(V)));
						}
					}
					else
					{
						R = (R / V);
					}
				}
				else
				{
					R = (R * V);
				}
			}
		}
		internal void expr4(ref double R)
		{
			this.expr3(ref R);
			while (true)
			{
				TCalculator.TToken fToken = this.FToken;
				if (fToken < TCalculator.TToken.tkADD || fToken >= TCalculator.TToken.tkLT)
				{
					break;
				}
				TCalculator.TToken oldt = this.FToken;
				this.lex();
				double V = 0.0;
				this.expr3(ref V);
				if (oldt != TCalculator.TToken.tkADD)
				{
					if (oldt == TCalculator.TToken.tkSUB)
					{
						R = (R - V);
					}
				}
				else
				{
					R = (R + V);
				}
			}
		}
		internal void expr5(ref double R)
		{
			this.expr4(ref R);
			while (true)
			{
				TCalculator.TToken fToken = this.FToken;
				if (fToken < TCalculator.TToken.tkLT || fToken >= TCalculator.TToken.tkOR)
				{
					break;
				}
				TCalculator.TToken oldt = this.FToken;
				this.lex();
				double V = 0.0;
				this.expr4(ref V);
				switch (oldt)
				{
					case TCalculator.TToken.tkLT:
					{
						R = this.tofloat(R < V);
						break;
					}
					case TCalculator.TToken.tkLE:
					{
						R = this.tofloat(R <= V);
						break;
					}
					case TCalculator.TToken.tkEQ:
					{
						R = this.tofloat(R == V);
						break;
					}
					case TCalculator.TToken.tkNE:
					{
						R = this.tofloat(R != V);
						break;
					}
					case TCalculator.TToken.tkGE:
					{
						R = this.tofloat(R >= V);
						break;
					}
					case TCalculator.TToken.tkGT:
					{
						R = this.tofloat(R > V);
						break;
					}
				}
			}
		}
		internal void expr6(ref double R)
		{
			this.expr5(ref R);
			while (true)
			{
				TCalculator.TToken fToken = this.FToken;
				if (fToken < TCalculator.TToken.tkOR || fToken >= (TCalculator.TToken)26)
				{
					break;
				}
				TCalculator.TToken oldt = this.FToken;
				this.lex();
				double V = 0.0;
				this.expr5(ref V);
				if (oldt != TCalculator.TToken.tkOR)
				{
					if (oldt != TCalculator.TToken.tkXOR)
					{
						if (oldt == TCalculator.TToken.tkAND)
						{
							R = ((double)(BDSSystem.Trunc(R) & BDSSystem.Trunc(V)));
						}
					}
					else
					{
						R = ((double)(BDSSystem.Trunc(R) ^ BDSSystem.Trunc(V)));
					}
				}
				else
				{
					R = ((double)(BDSSystem.Trunc(R) | BDSSystem.Trunc(V)));
				}
			}
		}
		internal void start(ref double R)
		{
			this.expr6(ref R);
			while (this.FToken == TCalculator.TToken.tkSEMICOLON)
			{
				this.lex();
				this.expr6(ref R);
			}
			if (this.FToken != TCalculator.TToken.tkEOF)
			{
				this.RaiseError("Syntax error");
			}
		}

		public TCalculator()
		{
			TPersistentHelper.Create(this);
			this.FVars = new TObjectList(true);
		}

		public void Dispose()
		{
			if (!this.Disposed_)
			{
				this.ClearVars();
				this.FVars.Free();
				this.Disposed_ = true;
			}
		}

		public void ClearVars()
		{
			this.FVars.Clear();
		}

		public double Calc(string aExpression)
		{
			this.FExpression = aExpression + "\0";
			this.FPtr = 1;
			this.lex();
			double Result = 0.0;
			this.start(ref Result);
			return Result;
		}

		public void Free()
		{
			TObjectHelper.Free(this);
		}

		static TCalculator()
		{
			TCalculator.C_ERR_BAD_NUMBER = 7;
			TCalculator.C_ERR_CANNOT_SOLVE = 6;
			TCalculator.C_ERR_DEV_BY_ZERO = 5;
			TCalculator.C_ERR_FUNCTION_ERR = 4;
			TCalculator.C_ERR_UNKNOWN_FUNC = 3;
			TCalculator.C_ERR_EMPTY_EXPR = 2;
			TCalculator.C_ERR_BRACKETS = 1;
			TCalculator.C_NOERROR = 0;
		}

		private static bool _lex_ConvertNumber([In] TCalculator Self, int first, int last, ushort @base)
		{
			Self.fvalue = 0.0;
			if (first < last)
			{
				do
				{
					byte c = (byte)((int)Self.FExpression[first - 1] - 48);
					if (c > 9)
					{
						c -= 7;
						if (c > 15)
						{
							c -= 32;
						}
					}
					if ((ushort)c >= @base)
					{
						break;
					}
					Self.fvalue = (Self.fvalue * @base + c);
					first++;
				}
				while (first < last);
			}
			return first == last;
		}
	}
}
