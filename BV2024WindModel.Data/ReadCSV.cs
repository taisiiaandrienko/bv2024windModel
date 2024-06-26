using System.Collections.Generic;
using BV2024WindModel.Abstractions;
using CsvHelper;
using System.Globalization;
using System;
using System.IO;
using System.Linq;


namespace BV2024WindModel.Data
{

    public static class ReadCSV
    {

        public static IEnumerable<Container> ReadFromCsv(string path)
        {

            var containers = new List<Container>();

            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                int lineIndex = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] fields = line.Split(',');

                    // Ensure there are enough fields in the current line
                    if (fields.Length >= 7 && lineIndex > 0)
                    {
                        float lcg = float.Parse(fields[1]);
                        float tcg = float.Parse(fields[2]);
                        float basis = float.Parse(fields[3]);
                        float length = float.Parse(fields[4]);
                        float width = float.Parse(fields[5]);
                        float height = float.Parse(fields[6]);

                        var container = new Container(lcg, tcg, basis, length, width, height);
                        containers.Add(container);
                    }
                    lineIndex++;
                }
            }

            /*
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Container>();
                if (records != null && records.Count() > 0)
                {
                    containers.AddRange(records);
                }
            }


            
           
            
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    string Name = fields[0];
                    var lcg = float.Parse(fields[1]);
                    var tcg = float.Parse(fields[2]);
                    var vcg = float.Parse(fields[3]);
                    var length = float.Parse(fields[4]);
                    var width = float.Parse(fields[5]);
                    var height = float.Parse(fields[6]);

                    var container = new Container(lcg, tcg, vcg, length, width, height);
                    containers.Add(container);

                }
            }
            */
            return containers;
        }
    }



}
