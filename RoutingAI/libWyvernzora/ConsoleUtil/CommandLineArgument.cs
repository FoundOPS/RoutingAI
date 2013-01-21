using System;

namespace libWyvernzora.ConsoleUtil
{
    /// <summary>
    /// Represents one command line argument or option
    /// </summary>
    public sealed class CommandLineArgument
    {
        /// <summary>
        /// Represents type of command line argument
        /// </summary>
        public enum ArgumentType
        {
            /// <summary>
            /// Argument is an unnamed string passed via command line
            /// <c>program.exe argument</c>
            /// </summary>
            Argument,
            /// <summary>
            /// Option is a named switch passed via command line
            /// Option may contain arguments of its own
            /// <c>program.exe /option:arg0,arg1...</c>
            /// </summary>
            Option
        }

        private readonly String name;
        private readonly String[] args;
        private readonly ArgumentType type;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the option or contents of the argument</param>
        /// <param name="args">Arguments of the option; empty array for arguments</param>
        /// <param name="type">Type of the CommandLineArgument</param>
        public CommandLineArgument(String name, String[] args, ArgumentType type)
        {
            this.name = name;
            this.args = args;
            this.type = type;
        }

        /// <summary>
        /// Gets name of the option or contents of the argument
        /// </summary>
        public String Name
        { get { return name; } }
        /// <summary>
        /// Gets arguments of the option or an empty array if this is an argument
        /// </summary>
        public String[] Arguments
        { get { return args; } }
        /// <summary>
        /// Gets the type ot this CommandLineArgument object
        /// </summary>
        public ArgumentType Type
        { get { return type; } }
    }
}