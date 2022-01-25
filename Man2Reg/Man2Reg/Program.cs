using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Man2Reg
{
    class Program
    {
        public static string OutputText { get; set; }
        public static string file { get; set; }
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                //Usage
                Console.WriteLine("W10M Update Manifest to REG Converter by Empyreal96");
                Console.WriteLine("Use this tool to convert Registry Manifest files found in CBS Update Cabs");
                Console.WriteLine("");
                Console.WriteLine("Usage:");
                Console.WriteLine("man2reg.exe \"C:\\Path\\To\\Update.Manifest\"");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("CBS Registry Manifest to REG Converter");
                Console.WriteLine("");
                

                //
                string path = args[0];
                if (Directory.Exists("bin") == false)
                {
                    Directory.CreateDirectory("bin");
                }

                file = Path.GetFileName(path);
                if (File.Exists($"bin\\{file}.xml"))
                {
                    File.Delete($"bin\\{file}.xml");
                }
                File.Copy(path, $"bin\\{file}.xml");

                OutputText += "Windows Registry Editor Version 5.00\n\n";
                Console.WriteLine($"Reading Manifest: {file}");
                try
                {
                    XElement xelement = XElement.Load($"bin\\{file}.xml");
                    IEnumerable<XElement> registryKeys = xelement.Elements();
                    //Console.WriteLine(xelement.Element("assemblyIdentity").Attribute("name").Value);
                    Console.WriteLine($"Windows Registry Editor Version 5.00");
                   
                    foreach (var key in registryKeys)
                    {

                        var assemblyDecendents = key.Elements();
                        foreach (var Decendent in assemblyDecendents)
                        {

                            string registryKeyname = $"[{Decendent.Attribute("keyName").Value}]";
                            OutputText += $"\n{registryKeyname}\n";
                            var registryValue = Decendent.Descendants();
                            Console.WriteLine();
                            Console.WriteLine(registryKeyname);
                            
                            
                            foreach (var value in registryValue)
                            {

                                if (value.Attribute("name").Value.Contains("SD0") || value.Attribute("name").Value.Contains("SD1") || value.Attribute("name").Value.Contains("SD2"))
                                {

                                }
                                else
                                {
                                    
                                    
                                    if (value.Attribute("valueType").Value == "REG_SZ")
                                    {


                                        string regValue = value.Attribute("name").Value;
                                        string regData = value.Attribute("value").Value;
                                        Console.WriteLine($"\"{regValue}\"=\"{regData}\"");
                                        OutputText += $"\"{regValue}\"=\"{regData}\"\n";

                                    }

                                    if (value.Attribute("valueType").Value == "REG_DWORD")
                                    {
                                        string regValue = value.Attribute("name").Value;
                                        string regData = value.Attribute("value").Value.Replace("0x", "");
                                        Console.WriteLine($"\"{regValue}\"=dword={regData}");
                                        OutputText += $"\"{regValue}\"=dword={regData}\n";
                                    }
                                    if (value.Attribute("valueType").Value == "REG_QWORD")
                                    {
                                        string regValue = value.Attribute("name").Value;
                                        string regData = value.Attribute("value").Value;
                                        var qword = StringSplit(regData, 2);
                                        Console.WriteLine($"\"{regValue}\"=hex(b):{String.Join(",", qword)}");
                                        OutputText += $"\"{regValue}\"=hex(b):{String.Join(",", qword)}\n";

                                    }
                                }
                                //Console.WriteLine();
                            }




                        }
                    }
                } catch (Exception ex)
                {

                }


            }
            if (File.Exists($"{file}.reg"))
            {
                File.Delete($"{file}.reg");
            }
            //File.Create($"{file}.reg");
            File.AppendAllText($"{file}.reg", OutputText);
            Console.WriteLine("Done");
            Console.ReadLine();

        }
        static IEnumerable<string> StringSplit(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

    }
}

