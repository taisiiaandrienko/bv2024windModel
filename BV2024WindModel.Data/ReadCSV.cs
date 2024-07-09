﻿using System.Collections.Generic;
using System.IO;
using BV2024WindModel.Abstractions;


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
                        string position = fields[0];
                        float lcg = float.Parse(fields[1]);
                        float tcg = float.Parse(fields[2]);
                        float basis = float.Parse(fields[3]);
                        float length = float.Parse(fields[4]);
                        float width = float.Parse(fields[5]);
                        float height = float.Parse(fields[6]);

                        var container = new Container(position, lcg, tcg, basis, length, width, height);
                        containers.Add(container);
                    }
                    lineIndex++;
                }
            }
            return containers;
        }
    }

}
