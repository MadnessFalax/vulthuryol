using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace vm
{
    public class VirtualMachine
    {
        private Stack<object> stack = new Stack<object>();
        private List<string[]> code = new List<string[]>();
        Dictionary<string, object> memory = new Dictionary<string, object>();
        CultureInfo provider = new CultureInfo("en-US");
        Dictionary<int, int> labels = new Dictionary<int, int>();

        public VirtualMachine(string code)
        {
            var tmp = code.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split(" ")).ToList();
            foreach (var instruction in tmp)
            {
                if (instruction.Length > 3) 
                {
                    var sb = new StringBuilder();
                    for (int i = 2; i < instruction.Length; i++)
                    {
                        sb.Append(instruction[i]);
                        sb.Append(' ');
                    }
                    sb.Length--;
                    instruction[2] = sb.ToString();
                }
            }
            this.code = tmp;

            for (int i = 0; i < this.code.Count; i++)
            {
                var instruction = this.code[i];
                if (instruction[0].Equals("LABEL"))
                {
                    labels[Convert.ToInt32(instruction[1])] = i - 1;
                }
            }
        }
        public void Run()
        {
            int index = 0;
            while (index < code.Count)
            {
                var instruction = code[index];

                if (instruction[0].Equals("PUSH"))
                {
                    if (instruction[1] == "I") stack.Push(int.Parse(instruction[2]));
                    else if (instruction[1] == "F") stack.Push(float.Parse(instruction[2], provider));
                    else if (instruction[1] == "S") 
                    {
                        if (instruction[2].Length <= 3)
                        {
                            stack.Push("");
                        }
                        else
                        {
                            stack.Push(instruction[2].Substring(1, instruction[2].Length - 2));
                        }
                    } 
                    else if (instruction[1] == "B") stack.Push(instruction[2] == "TRUE" ? true : false);
                }
                else if (instruction[0].Equals("POP"))
                {
                    stack.Pop();
                }
                else if (instruction[0].Equals("PRINT"))
                {
                    var count = int.Parse(instruction[1]);
                    var values = new List<object>();
                    for (int i = count - 1; i >= 0; i--)
                    {
                        values.Add(stack.Pop());
                    }
                    values.Reverse();
                    foreach (var value in values)
                    {
                        Console.Write(value);
                        // Console.Write(" ");
                    }
                    Console.Write("\n");
                }
                else if (instruction[0].Equals("SAVE"))
                {
                    var value = stack.Pop();
                    memory[instruction[1]] = value;
                }
                else if (instruction[0].Equals("LOAD"))
                {
                    stack.Push(memory[instruction[1]]);
                }
                else if (instruction[0].Equals("READ"))
                {
                    var val = Console.ReadLine();
                    if (instruction[1].Equals("B"))
                    {
                        stack.Push(bool.Parse(val));
                    }
                    else if (instruction[1].Equals("F"))
                    {
                        stack.Push(float.Parse(val, provider));
                    }
                    else if (instruction[1].Equals("I"))
                    {
                        stack.Push(int.Parse(val));
                    }
                    else if (instruction[1].Equals("S"))
                    {
                        stack.Push(val);
                    }
                }
                else if (instruction[0].Equals("ITOF"))
                {
                    var val = stack.Pop();
                    if (val is int i)
                        stack.Push((float)i);
                }
                else if (instruction[0].Equals("NOT"))
                {
                    var val = (bool)stack.Pop();
                    stack.Push(!val);
                }
                else if (instruction[0].Equals("UMINUS"))
                {
                    var val = stack.Pop();
                    if (val is float f)
                        stack.Push(0 - f);
                    else if (val is int i)
                        stack.Push(0 - i);
                }
                else if (instruction[0].Equals("FJMP"))
                {
                    var val = stack.Pop();
                    stack.Push(val);
                    if (!Convert.ToBoolean(val))
                    {
                        index = labels[Convert.ToInt32(instruction[1])];
                    }

                }
                else if (instruction[0].Equals("JMP"))
                {
                    index = labels[Convert.ToInt32(instruction[1])];
                }
                else if (instruction[0].Equals("LABEL"))
                {

                }
                else
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    if (instruction[0].Equals("ADD"))
                    {
                        if (left is float l)
                            stack.Push(l + (float)right);
                        else if (right is float r)
                            stack.Push((float)left + r);
                        else
                            stack.Push((int)left + (int)right);
                    }
                    else if (instruction[0].Equals("SUB"))
                    {
                        if (left is float l)
                            stack.Push(l - (float)right);
                        else if (right is float r)
                            stack.Push((float)left - r);
                        else
                            stack.Push((int)left - (int)right);
                    }
                    else if (instruction[0].Equals("MUL"))
                    {
                        if (left is float l_float && right is int r_int)
                        {
                            stack.Push((float)(l_float * r_int));
                        }
                        else if (left is int l_int && right is float r_float)
                        {
                            stack.Push((float)(l_int * r_float));
                        }
                        else if (left is float l && right is float r)
                        {
                            stack.Push(l * r);
                        }
                        else if (left is int l_i && right is int r_i )
                            stack.Push(l_i * r_i);
                    }
                    else if (instruction[0].Equals("DIV"))
                    {
                        if (left is float l)
                            stack.Push(l / (float)right);
                        else if (right is float r)
                            stack.Push((float)left / r);
                        else
                            stack.Push((int)left / (int)right);
                    }
                    else if (instruction[0].Equals("CONCAT"))
                    {
                        stack.Push((string) left + (string) right); 
                    }
                    else if (instruction[0].Equals("MOD"))
                    {
                        stack.Push((int) left % (int)right);
                    }
                    else if (instruction[0].Equals("EQ"))
                    {
                        if (left is float l)
                            stack.Push(right.Equals(l));
                        else if (right is float r)
                            stack.Push(left.Equals(r));
                        else if (left is string s)
                            stack.Push(right.Equals(s));
                        else if (left is int i)
                            stack.Push(right.Equals(i));
                    }
                    else if (instruction[0].Equals("LT"))
                    {
                        if (left is float l)
                            stack.Push(l < (float)right);
                        else if (right is float r)
                            stack.Push((float)left < r);
                        else
                            stack.Push((int)left < (int)right);
                    }
                    else if (instruction[0].Equals("GT"))
                    {
                        if (left is float l)
                            stack.Push(l > (float)right);
                        else if (right is float r)
                            stack.Push((float)left > r);
                        else
                            stack.Push((int)left > (int)right);
                    }
                    else if (instruction[0].Equals("OR"))
                    {
                        if (left is bool l)
                            stack.Push(l || (bool)right);
                    }
                    else if (instruction[0].Equals("AND"))
                    {
                        if (left is bool l)
                            stack.Push(l && (bool)right);
                    }
                }

                index++;

            }
        }
    }
}
