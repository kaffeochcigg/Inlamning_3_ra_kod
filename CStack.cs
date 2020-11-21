using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace Inlamning_3_ra_kod
{
    /* CLASS: CStack
     * PURPOSE: Is essentially a RPN-calculator with four registers X, Y, Z, T
     *   like the HP RPN calculators. Numeric values are entered in the entry
     *   string by adding digits and one comma. For test purposes the method
     *   RollSetX can be used instead. Operations can be performed on the
     *   calculator preferrably by using one of the methods
     *     1. BinOp - merges X and Y into X via an operation and rolls down
     *        the stack
     *     2. Unop - operates X and puts the result in X with overwrite
     *     3. Nilop - adds a known constant on the stack and rolls up the stack
     */
    public class CStack
    {
        public double X, Y, Z, T;
        public string entry;
        public string letPress;
        public double[] letNum = new double[8];
        // Hard coded filepath <<ADD YOUR OWN>>
        public string filePath = @"C:\Yourpath\molkfreecalc.clc";
        /* CONSTRUCTOR: CStack
         * PURPOSE: create a new stack and init X, Y, Z, T and the text entry.
         * Also looks for a file with saved parameters for X, Y, Z, T and for A-H.
         * PARAMETERS: --
         */
        public CStack()
        {
            entry = "";
            if (File.Exists(filePath))
            {
                string[] newLines = File.ReadAllLines(filePath);
                foreach (string word in newLines)
                {
                    string[] vars = word.Split(':');
                    switch (vars[0])
                    {
                        case "T": T = double.Parse(vars[1]); break;
                        case "Z": Z = double.Parse(vars[1]); break;
                        case "Y": Y = double.Parse(vars[1]); break;
                        case "X": X = double.Parse(vars[1]); break;
                        case "A": letNum[0] = double.Parse(vars[1]); break;
                        case "B": letNum[1] = double.Parse(vars[1]); break;
                        case "C": letNum[2] = double.Parse(vars[1]); break;
                        case "D": letNum[3] = double.Parse(vars[1]); break;
                        case "E": letNum[4] = double.Parse(vars[1]); break;
                        case "F": letNum[5] = double.Parse(vars[1]); break;
                        case "G": letNum[6] = double.Parse(vars[1]); break;
                        case "H": letNum[7] = double.Parse(vars[1]); break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                X = Y = Z = T = 0;
            }
        }
        /* METHOD: Exit
         * PURPOSE: called on exit, saves to file if filepath exists
         * PARAMETERS: --
         * RETURNS: --
         */
        public void Exit()
        {
            if (File.Exists(filePath))
            {
                string[] textToFile = new string[] { $"T: {T}", $"Z: {Z}", $"Y: {Y}", $"X: {X}",
                $"A: {letNum[0]}", $"B: {letNum[1]}", $"C: {letNum[2]}", $"D: {letNum[3]}",
                $"E: {letNum[4]}", $"F: {letNum[5]}", $"G: {letNum[6]}", $"H: {letNum[7]}" };
                File.WriteAllLines(filePath, textToFile);
            }
        }
        /* METHOD: StackString
         * PURPOSE: construct a string to write out in a stack view
         * PARAMETERS: --
         * RETURNS: the string containing the values T, Z, Y, X with newlines 
         *   between them
         */
        public string StackString()
        {
            return $"{T}\n{Z}\n{Y}\n{X}\n{entry}";
        }
        /* METHOD: VarString
         * PURPOSE: construct a string to write out in a variable list
         * PARAMETERS: --
         * RETURNS: the values stored in A-H (letNum[1-7])
         */
        public string VarString()
        {
            return $"{letNum[0]}\n{letNum[1]}\n{letNum[2]}\n{letNum[3]}\n{letNum[4]}" +
                $"\n{letNum[5]}\n{letNum[6]}\n{letNum[7]}\n{letPress}";
        }
        /* METHOD: SetX
         * PURPOSE: set X with overwrite
         * PARAMETERS: double newX - the new value to put in X
         * RETURNS: --
         */
        public void SetX(double newX)
        {
            X = newX;
        }
        /* METHOD: EntryAddNum
         * PURPOSE: add a digit to the entry string
         * PARAMETERS: string digit - the candidate digit to add at the end of the
         *   string
         * RETURNS: --
         * FAILS: if the string digit does not contain a parseable integer, nothing
         *   is added to the entry
         */
        public void EntryAddNum(string digit)
        {
            int val;
            if (int.TryParse(digit, out val))
            {
                entry = entry + val;
            }
        }
        /* METHOD: EntryAddComma
         * PURPOSE: adds a comma to the entry string
         * PARAMETERS: --
         * RETURNS: --
         * FAILS: if the entry string already contains a comma, nothing is added
         *   to the entry
         */
        public void EntryAddComma()
        {
            if (entry.IndexOf(",") == -1)
                entry = entry + ",";
        }
        /* METHOD: EntryChangeSign
         * PURPOSE: changes the sign of the entry string
         * PARAMETERS: --
         * RETURNS: --
         * FEATURES: if the first char is already a '-' it is exchanged for a '+',
         *   if it is a '+' it is changed to a '-', otherwise a '-' is just added
         *   first
         */
        public void EntryChangeSign()
        {
            char[] cval = entry.ToCharArray();
            if (cval.Length > 0)
            {
                switch (cval[0])
                {
                    case '+': cval[0] = '-'; entry = new string(cval); break;
                    case '-': cval[0] = '+'; entry = new string(cval); break;
                    default: entry = '-' + entry; break;
                }
            }
            else
            {
                entry = '-' + entry;
            }
        }
        /* METHOD: Enter
         * PURPOSE: converts the entry to a double and puts it into X
         * PARAMETERS: --
         * RETURNS: --
         * FEATURES: the entry is cleared after a successful operation
         */
        public void Enter()
        {
            if (entry != "")
            {
                RollSetX(double.Parse(entry));
                entry = "";
            }
        }
        /* METHOD: Drop
         * PURPOSE: drops the value of X, and rolls down
         * PARAMETERS: --
         * RETURNS: --
         * FEATURES: Z gets the value of T
         */
        public void Drop()
        {
            X = Y; Y = Z; Z = T;
        }
        /* METHOD: DropSetX
         * PURPOSE: replaces the value of X, and rolls down
         * PARAMETERS: double newX - the new value to assign to X
         * RETURNS: --
         * FEATURES: Z gets the value of T
         * NOTES: this is used when applying binary operations consuming
         *   X and Y and putting the result in X, while rolling down the
         *   stack
         */
        public void DropSetX(double newX)
        {
            X = newX; Y = Z; Z = T;
        }
        /* METHOD: BinOp
         * PURPOSE: evaluates a binary operation
         * PARAMETERS: string op - the binary operation retrieved from the
         *   GUI buttons
         * RETURNS: --
         * FEATURES: the stack is rolled down
         */
        public void BinOp(string op)
        {
            switch (op)
            {
                case "+": DropSetX(Y + X); break;
                case "−": DropSetX(Y - X); break;
                case "×": DropSetX(Y * X); break;
                case "÷": DropSetX(Y / X); break;
                case "yˣ": DropSetX(Math.Pow(Y, X)); break;
                case "ˣ√y": DropSetX(Math.Pow(Y, 1.0 / X)); break;
            }
        }
        /* METHOD: Unop
         * PURPOSE: evaluates a unary operation
         * PARAMETERS: string op - the unary operation retrieved from the
         *   GUI buttons
         * RETURNS: --
         * FEATURES: the stack is not moved, X is replaced by the result of
         *   the operation
         */
        public void Unop(string op)
        {
            switch (op)
            {
                // Powers & Logarithms:
                case "x²": SetX(X * X); break;
                case "√x": SetX(Math.Sqrt(X)); break;
                case "log x": SetX(Math.Log10(X)); break;
                case "ln x": SetX(Math.Log(X)); break;
                case "10ˣ": SetX(Math.Pow(10, X)); break;
                case "eˣ": SetX(Math.Exp(X)); break;

                // Trigonometry:
                case "sin": SetX(Math.Sin(X)); break;
                case "cos": SetX(Math.Cos(X)); break;
                case "tan": SetX(Math.Tan(X)); break;
                case "sin⁻¹": SetX(Math.Asin(X)); break;
                case "cos⁻¹": SetX(Math.Acos(X)); break;
                case "tan⁻¹": SetX(Math.Atan(X)); break;
            }
        }
        /* METHOD: Nilop
         * PURPOSE: evaluates a "nilary operation" (insertion of a constant)
         * PARAMETERS: string op - the nilary operation (name of the constant)
         *   retrieved from the GUI buttons
         * RETURNS: --
         * FEATURES: the stack is rolled up, X is preserved in Y that is preserved in
         *   Z that is preserved in T, T is erased
         */
        public void Nilop(string op)
        {
            switch (op)
            {
                case "π": RollSetX(Math.PI); break;
                case "e": RollSetX(Math.E); break;
            }
        }
        /* METHOD: Roll
         * PURPOSE: rolls the stack up
         * PARAMETERS: --
         * RETURNS: --
         */
        public void Roll()
        {
            double tmp = T;
            T = Z; Z = Y; Y = X; X = tmp;
        }
        /* METHOD: Roll
         * PURPOSE: rolls the stack up and puts a new value in X
         * PARAMETERS: double newX - the new value to put into X
         * RETURNS: --
         * FEATURES: T is dropped
         */
        public void RollSetX(double newX)
        {
            T = Z; Z = Y; Y = X; X = newX;
        }
        /* METHOD: SetAddress
         * PURPOSE: save button press in variable letPress
         * PARAMETERS: string name - name of letter to put in letPress
         * RETURNS: --
         * FEATURES: latest button press is saved
         */
        public void SetAddress(string name)
        {
            letPress = name;
        }
        /* METHOD: SetVar
         * PURPOSE: saves the X variable in A-H(letNum[0-7]) based on latest buttonpress
         * PARAMETERS: --
         * RETURNS: --
         */
        public void SetVar()
        {
            switch (letPress)
            {
                case "A": letNum[0] = X; break;
                case "B": letNum[1] = X; break;
                case "C": letNum[2] = X; break;
                case "D": letNum[3] = X; break;
                case "E": letNum[4] = X; break;
                case "F": letNum[5] = X; break;
                case "G": letNum[6] = X; break;
                case "H": letNum[7] = X; break;
            }

        }
        /* METHOD: GetVar
         * PURPOSE: sets the value of X to the latest pressed button when using the recall button
         * PARAMETERS: --
         * RETURNS: --
         */
        public void GetVar()
        {
            switch (letPress)
            {
                case "A": RollSetX(letNum[0]); break;
                case "B": RollSetX(letNum[1]); break;
                case "C": RollSetX(letNum[2]); break;
                case "D": RollSetX(letNum[3]); break;
                case "E": RollSetX(letNum[4]); break;
                case "F": RollSetX(letNum[5]); break;
                case "G": RollSetX(letNum[6]); break;
                case "H": RollSetX(letNum[7]); break;
            }
        }
    }
}
