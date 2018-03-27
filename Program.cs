using System;

namespace Tumba.BitcointaxTransform
{
    public class Program
    {
        public const string USAGE = @"inputType [uphold] outputType [trading|income] inputFile outputFile";

        public static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine(USAGE);
                Environment.Exit(1);
                return;
            }

            TransformTask task;
            string errorMessage;

            if (!TransformTask.TryCreateFromArgs(args, out task, out errorMessage))
            {
                Console.WriteLine(USAGE);
                Console.WriteLine("[Error] [ExitCode 1] TryCreateFromArgs failed with error: {0}", errorMessage);
                Environment.Exit(1);
                return;
            }

            if (!task.TransformInputToOutput(out errorMessage))
            {
                Console.WriteLine(USAGE);
                Console.WriteLine("[Error] [ExitCode 2] TransformInputToOutput failed with error: {0}", errorMessage);
                Environment.Exit(2);
                return;
            }
            
            Console.WriteLine("[Success] [ExitCode 0] {0} file successfully transformed and saved to: {1}", task.Output, task.OutputFile);
        }
    }
}
