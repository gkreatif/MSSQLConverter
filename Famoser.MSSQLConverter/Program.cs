using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Famoser.MSSQLConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                Console.WriteLine("Place to be converted file in this directory: " + Environment.CurrentDirectory);
                Console.WriteLine("Name the file mssql.slq");
                Console.WriteLine("Press Enter to confirm");
                Console.ReadKey();
                Console.WriteLine("Converting...");

                //from: INSERT [dbo].[meta_attribute] ([attribute_id], [entity_id], [logical_name], [physical_name], [attribute_type_id], [length], [precision], [scale], [is_visible], [is_nullable], [is_read_only], [is_system], [is_primary_key], [is_identity], [use_value_list], [attribute_of], [additional_data], [version], [is_multilanguage], [content_language_id]) VALUES (N'5d62eec4-bd8d-429e-979b-02e8bd03d3c1', N'a1d66796-5c4d-4c3f-8456-1034a47ce8b6', N'rechnung_id', N'rechnung_id', N'b4e787bf-9271-4c98-9816-fa597a17bdda', 50, 18, 0, 1, 0, 0, 1, 1, 1, 0, NULL, NULL, 1, 0, NULL)
                //to: INSERT [dbo].[meta_attribute] (`attribute_id`, `entity_id`, `logical_name`, `physical_name`, `attribute_type_id`, `length`, `precision`, `scale`, `is_visible`, `is_nullable`, `is_read_only`, `is_system`, `is_primary_key`, `is_identity`, `use_value_list`, `attribute_of`, `additional_data`, `version`, `is_multilanguage`, `content_language_id`) SELECT (N'5d62eec4-bd8d-429e-979b-02e8bd03d3c1', N'a1d66796-5c4d-4c3f-8456-1034a47ce8b6', N'rechnung_id', N'rechnung_id', N'b4e787bf-9271-4c98-9816-fa597a17bdda', 50, 18, 0, 1, 0, 0, 1, 1, 1, 0, NULL, NULL, 1, 0, NULL)
                //remove: GO

                var reslines = new List<string>();
                var filecounter = 0;
                var ignores = new List<string>()
                {
                    "GO",
                    "SET IDENTITY_INSERT"
                };

                var lines = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "mssql.sql")).ToList();
                for (int i = 0; i < lines.Count(); i++)
                {
                    if (ignores.Any(d => lines[i].StartsWith(d)) || string.IsNullOrEmpty(lines[i]))
                    {
                        lines.RemoveAt(i);
                        i--;
                    }
                    else if (lines[i].StartsWith("INSERT [dbo]."))
                    {
                        var newline = lines[i].Substring(("INSERT [dbo].").Count());
                        var table = newline.Substring(1, newline.IndexOf("]", StringComparison.Ordinal) - 1);
                        newline = newline.Substring(newline.IndexOf("]", StringComparison.Ordinal) + 1);
                        var columns = newline.Substring(0, newline.IndexOf(")", StringComparison.Ordinal) + 1);
                        columns = columns.Replace("[", "`");
                        columns = columns.Replace("]", "`");

                        newline = newline.Substring(newline.IndexOf(")", StringComparison.Ordinal) + 1);

                        if (i == 638)
                            "here".ToString();

                        //append additional lines;
                        for (int j = i + 1; j < lines.Count; j++)
                        {
                            if (ignores.Any(d => lines[j].StartsWith(d)) || lines[j].StartsWith("INSERT [dbo]."))
                                break;
                            else
                            {
                                newline += lines[j];
                                lines.RemoveAt(j--);
                            }
                        }

                        var res = "INSERT INTO " + table + columns + newline + ";";
                        lines[i] = res;
                        reslines.Add(res);

                        Console.Write("\rEdited line " + i + "/" + lines.Count);

                        if (reslines.Count == 100)
                        {
                            File.WriteAllLines("output.part" + filecounter++ + ".sql", reslines);
                            reslines.Clear();
                        }
                    }
                    else
                    {
                        Console.WriteLine("DANGER: Not recognised line");
                        Console.WriteLine(lines[i]);
                        Console.WriteLine();
                        Console.WriteLine();
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Finished. Please investigate all DANGER warnings (if any)");
                Console.ReadKey();

                File.WriteAllLines("output.sql", lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured :(");
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }
    }
}
