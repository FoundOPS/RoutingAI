using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace libWyvernzora.ConsoleUtil
{
    /// <summary>
    /// Represents a collection of command line arguments
    /// </summary>
    public class CommandLineArguments
    {
        private CommandLineArgument[] m_args;

        /// <summary>
        /// Gets the number of command line arguments contained in this object
        /// </summary>
        public Int32 Count
        { get { return m_args.Length; } }
        /// <summary>
        /// Gets the specified command line argument
        /// </summary>
        /// <param name="i">Index of the argument to retrieve</param>
        /// <returns>CommandLineArgument object</returns>
        public CommandLineArgument this[int i]
        { get { return m_args[i]; } }

        /// <summary>
        /// Constructor.
        /// Creates CommandLineArguments object from Environment.CommandLine
        /// </summary>
        public CommandLineArguments()
            : this(System.Environment.CommandLine) { }
        /// <summary>
        /// Constructor.
        /// Creates CommandLineArgument from a string
        /// </summary>
        /// <param name="cmdLine"></param>
        public CommandLineArguments(String cmdLine)
        {
            List<String> argv = new List<string>();
            Int32 nextStart = 0;
            Boolean suppressFirst = false;

            foreach (Match m in Regex.Matches(cmdLine, "(\"[^\"]*\"|([^\" ])+)+", RegexOptions.ExplicitCapture))
            {
                if (m.Index != nextStart && !cmdLine.Substring(nextStart, m.Index - nextStart).All((char c) => { return c == ' '; }))
                    throw new InvalidOperationException();

                nextStart = m.Index + m.Length;

                if (!suppressFirst)
                {
                    suppressFirst = true;
                    continue;
                }

                if (m.Success)
                    argv.Add(DescapeQuotes(m.Value));
                else
                    throw new InvalidOperationException();

            }

            if (cmdLine.Length != nextStart && !cmdLine.Substring(nextStart).All((char c) => { return c == ' '; }))
                throw new InvalidOperationException();

            List<CommandLineArgument> args = new List<CommandLineArgument>();

            foreach (string arg in argv)
            {
                if (arg.StartsWith("/") || arg.StartsWith("-"))
                {
                    String narg = arg.Substring(1);
                    String name;
                    String opt;
                    Int32 index = narg.IndexOf(":");
                    if (index >= 0)
                    {
                        name = DescapeQuotes(narg.Substring(0, index));
                        opt = DescapeQuotes(narg.Substring(index + 1));
                    }
                    else
                    {
                        name = DescapeQuotes(narg);
                        opt = "";
                    }

                    args.Add(new CommandLineArgument(name, (from s in opt.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                            select s.Trim()).ToArray(), CommandLineArgument.ArgumentType.Option));
                }
                else
                    args.Add(new CommandLineArgument(arg, new string[0], CommandLineArgument.ArgumentType.Argument));
            }

            m_args = args.ToArray();
        }

        private static String DescapeQuotes(String str)
        {
            if (str == "") return "";
            if (str.StartsWith("\"") && str.EndsWith("\""))
                return str.Substring(1, str.Length - 2).Replace("\"\"", "\"");
            else
                return str;
        }
    }
}
