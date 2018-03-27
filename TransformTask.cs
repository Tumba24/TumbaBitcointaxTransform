using System;
using System.Collections.Generic;
using System.IO;

namespace Tumba.BitcointaxTransform
{
    public class TransformTask
    {
        public InputType Input { get; set; }
        public OutputType Output { get; set; }
        public string InputFile { get; set; }
        public string OutputFile { get; set; }

        public TransformTask() { }

        public static bool TryCreateFromArgs(string[] args, out TransformTask task, out string errorMessage)
        {
            task = null;

            if (args.Length != 4)
            {
                errorMessage = string.Format("Invalid number of args!  Expected {0} Found {1}", 4, args.Length);
                return false;
            }

            InputType input;
            if (!Enum.TryParse<InputType>(args[0], true, out input))
            {
                errorMessage = string.Format("Invalid input type! Found {0}", args[0]);
                return false;
            }

            OutputType output;
            if (!Enum.TryParse<OutputType>(args[1], true, out output))
            {
                errorMessage = string.Format("Invalid output type! Found {0}", args[1]);
                return false;
            }

            task = new TransformTask();
            task.Input = input;
            task.Output = output;
            task.InputFile = args[2];
            task.OutputFile = args[3];

            errorMessage = null;
            return true;
        }

        public bool TryReadImport(out List<TradingOutputRecord> records, out string errorMessage)
        {
            ITradingInputParser inputParser = null;
            records = new List<TradingOutputRecord>();

            try
            {
                switch (Input)
                {
                    case InputType.Uphold:
                    default:
                    {
                        inputParser = new UpholdTradingParser();
                        break;
                    }
                }

                FileInfo fInfo = new FileInfo(InputFile);
                if (!fInfo.Exists)
                {
                    errorMessage = string.Format("Input file not found! File: {0}", InputFile);
                    return false;
                }

                string data = File.ReadAllText(fInfo.FullName);
                inputParser.SetData(data);

                bool endOfData = false;
                while (!endOfData)
                {
                    TradingOutputRecord record;
                    if (!inputParser.TryReadNext(out endOfData, out record, out errorMessage))
                    {
                        return false;
                    }

                    if (record != null)
                    {
                        records.Add(record);
                    }
                }
            }
            finally
            {
                if (inputParser != null)
                {
                    inputParser.Dispose();
                }
            }

            errorMessage = null;
            return true;
        }

        public bool TransformInputToOutput(out string errorMessage)
        {
            List<TradingOutputRecord> records;
            if (!TryReadImport(out records, out errorMessage))
            {
                return false;
            }

            FileInfo outputFile = new FileInfo(OutputFile);
            if (outputFile.Exists)
            {
                outputFile.Delete();
            }

            if (!outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
            }

            using (FileStream fStream = outputFile.Create())
            using (StreamWriter writer = new StreamWriter(fStream))
            {
                writer.WriteLine(TradingOutputRecord.CSV_HEADER);

                foreach (TradingOutputRecord record in records)
                {
                    writer.WriteLine(record.ToCSVLine());
                }
            }

            errorMessage = null;
            return true;
        }
    }
}