using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Realboot
{
    public class ExpressionEvaluator
    {
        private StateManager memory;
    
        public ExpressionEvaluator(StateManager memoryRef)
        {
            this.memory = memoryRef;
        }
    
        public int Evaluate(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return 0;
    
            try
            {
                // cleanup
                expression = expression.Trim();
    
                // variable resolution
                expression = ResolveVariables(expression);
    
                // HEX numbers handling
                expression = Regex.Replace(expression, @"0x[0-9a-fA-F]+", new MatchEvaluator(HexEvaluator));
    
                // Shunting-yard algorithm to evaluate
                return MathParser.Evaluate(expression);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("[EVAL ERROR] Expr: {0} | Error: {1}", expression, e.Message));
                return 0;
            }
        }
    
        private string HexEvaluator(Match m)
        {
            return Convert.ToInt32(m.Value, 16).ToString();
        }
    
        private string ResolveVariables(string expr)
        {
            // Flag loop to replace all occurrences
            while (expr.Contains("$"))
            {
                Match match = Regex.Match(expr, @"\$(W|F|T)\(([^)]+)\)");
                if (!match.Success) break;
    
                string fullVar = match.Value;
                string type = match.Groups[1].Value;
                string name = match.Groups[2].Value;
    
                string val = "0";
                if (type == "W") val = memory.GetInt(name).ToString();
                else if (type == "F") val = memory.GetFlag(name) ? "1" : "0";
                // TODO : Handle $T variables
                expr = expr.Replace(fullVar, val);
            }
            return expr;
        }
    }
    
    
    public static class MathParser
    {
        private static Dictionary<string, int> precedence = new Dictionary<string, int>()
        {
            { "(", 0 }, { "+", 1 }, { "-", 1 }, 
            { "*", 2 }, { "/", 2 }, { "%", 2 },
            { "<<", 3 }, { ">>", 3 },
            { "&", 4 }, { "|", 4 } // base bitwise logic
        };
    
        public static int Evaluate(string expression)
        {
            List<string> rpn = ToRPN(expression);
            Stack<int> stack = new Stack<int>();
    
            foreach (string token in rpn)
            {
                if (IsNumber(token))
                {
                    stack.Push(int.Parse(token));
                }
                else
                {
                    if (stack.Count < 2) return 0; // Error handling
                    int b = stack.Pop();
                    int a = stack.Pop();
                    stack.Push(ApplyOp(token, a, b));
                }
            }
    
            return stack.Count > 0 ? stack.Pop() : 0;
        }
    
        // Reverse Polish Notation
        private static List<string> ToRPN(string expr)
        {
            List<string> output = new List<string>();
            Stack<string> ops = new Stack<string>();
    
            for (int i = 0; i < expr.Length; i++)
            {
                char c = expr[i];
    
                if (c == ' ') continue;
    
                if (char.IsDigit(c) || c == '-') // Number manager (negative included)
                {
                    // Vérifier si le '-' est un opérateur ou un signe négatif
                    if (c == '-' && (i > 0 && (char.IsDigit(expr[i-1]) || expr[i-1] == ')')))
                    {
                        // Minus operator
                        HandleOperator("-", output, ops);
                    }
                    else
                    {
                        // is negative number
                        string num = c.ToString();
                        while (i + 1 < expr.Length && char.IsDigit(expr[i + 1]))
                        {
                            num += expr[++i];
                        }
                        output.Add(num);
                    }
                }
                else if (c == '(')
                {
                    ops.Push("(");
                }
                else if (c == ')')
                {
                    while (ops.Count > 0 && ops.Peek() != "(")
                    {
                        output.Add(ops.Pop());
                    }
                    if (ops.Count > 0) ops.Pop();
                }
                else
                {
                    // Operator
                    string op = c.ToString();
                    
                    // Double operation (<<, >>)
                    if (i + 1 < expr.Length)
                    {
                        char next = expr[i + 1];
                        if ((c == '<' && next == '<') || (c == '>' && next == '>'))
                        {
                            op += next;
                            i++;
                        }
                    }
    
                    if (precedence.ContainsKey(op))
                    {
                        HandleOperator(op, output, ops);
                    }
                }
            }
    
            while (ops.Count > 0)
            {
                output.Add(ops.Pop());
            }
    
            return output;
        }
    
        // Operator handling in Shunting-yard
        private static void HandleOperator(string op, List<string> output, Stack<string> ops)
        {
            while (ops.Count > 0 && precedence.ContainsKey(ops.Peek()) && precedence[ops.Peek()] >= precedence[op])
            {
                output.Add(ops.Pop());
            }
            ops.Push(op);
        }
    
        // Check if string is a number
        private static bool IsNumber(string s)
        {
            int res;
            return int.TryParse(s, out res);
        }
    
        // Binary operations logic reimplementation
        private static int ApplyOp(string op, int a, int b)
        {
            if (op == "+") return a + b;
            if (op == "-") return a - b;
            if (op == "*") return a * b;
            if (op == "/") return b == 0 ? 0 : a / b;
            if (op == "%") return a % b;
            if (op == "<<") return a << b;
            if (op == ">>") return a >> b;
            if (op == "&") return a & b;
            if (op == "|") return a | b;
            return 0;
        }
    }
}
