using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emtron2Pi
{
    internal class Program
    {
        static string CleanUpUnits(string units)
        {
            // Replace common unit abbreviations with their full forms or symbols
            units = units.Replace("deg", "\xB0"); // Degree symbol
            if (units == "[g]") units = "[G]";
            if (units == "[C]") units = "[\xB0\x43]"; // degrees Celsius symbol
            return units;
        }

        static void Main(string[] args)
        {
            //Check argumants and open files
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Emtron2Pi <data_file>");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File not found: " + args[0]);
                return;
            }

            string infile = args[0];
            string outfile = args[0] + ".txt";

            // Read all lines from the CSV file
            var lines = File.ReadAllLines(infile);

            // Split each line into fields
            var rows = lines.Select(line => line.Split(',')).ToList();

            // Transpose rows to columns
            int columnCount = rows[0].Length;
            var columns = new List<List<string>>();

            for (int col = 0; col < columnCount; col++)
            {
                var column = new List<string>();
                foreach (var row in rows)
                {
                    if (row.Length < columnCount) continue; //skip the timestamp row
                    column.Add(row[col]);
                }
                columns.Add(column);
            }

            // Write the Pi file header
            var writer = new StreamWriter(outfile, false, Encoding.GetEncoding(1252));      //Important to use the 1252 encoding to match Pi Toolbox ASCII format      
            // File header

            writer.WriteLine("PiToolboxVersionedASCIIDataSet");
            writer.WriteLine("Version\t2");
            writer.WriteLine();
            writer.WriteLine("{OutingInformation}");
            writer.WriteLine($"CarName\tEmtron");
            writer.WriteLine("FirstLapNumber\t0");

            // Cycle through and create channel blocks
            for (int i = 2; i < columns.Count; i++) //skip the row with the timestamp
            {
                //extract units and name from string

                string fullChannelName = columns[i][0];
                int startIdx = fullChannelName.IndexOf('[');
                int endIdx = fullChannelName.IndexOf(']', startIdx + 1);
                string units = (startIdx != -1 && endIdx != -1 && endIdx > startIdx)
                    ? fullChannelName.Substring(startIdx, endIdx - startIdx + 1)
                    : "[user]";

                //fix units representation
                units = CleanUpUnits(units);

                string channelName = fullChannelName.Split('[')[0].Trim();

                writer.WriteLine();
                writer.WriteLine("{ChannelBlock}");
                writer.WriteLine($"Time\t{channelName}{units}");

                TimeSpan correctedTime = TimeSpan.Zero;

                for (int j = 1; j < columns[i].Count; j++)
                {
                    writer.WriteLine($"{columns[0][j]}\t{columns[i][j]}");
                }
            }
            writer.Close();
        }
    }
}
