using CommandLine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Man2Reg
{
    class Program
    {
        public static string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string outputText;
        public static string value;

        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args).WithParsed(m =>
                {
                    string inputFile = m.ManFile;
                    string outputFile = m.OutPath;
                    bool print = m.Verbose;
                    Console.WriteLine($"Man2Reg {version}");
                    Console.WriteLine("Copyright (C) 2023 Empyreal96 & fadilfadz01");
                    Console.WriteLine("");
                    Console.WriteLine($"Source: {inputFile}");
                    Console.WriteLine($"Destination: {outputFile}");
                    Console.WriteLine("Reading manifest...");
                    outputText = "Windows Registry Editor Version 5.00\n\n";
                    foreach (var registryKeys in XElement.Load(inputFile).Elements())
                    {
                        if (registryKeys.Name == "{urn:schemas-microsoft-com:asm.v3}registryKeys")
                        {
                            foreach (var registryKey in registryKeys.Elements())
                            {
                                if (m.Verbose) Console.WriteLine($"[{registryKey.Attribute("keyName").Value}]");
                                outputText += $"[{registryKey.Attribute("keyName").Value}]\n";
                                foreach (var registryValue in registryKey.Elements())
                                {
                                    if (registryValue.Name == "{urn:schemas-microsoft-com:asm.v3}registryValue")
                                    {
                                        foreach (var registryAttributes in registryValue.Attributes())
                                        {
                                            if (registryAttributes.Name == "name")
                                            {
                                                if (m.Verbose) Console.Write($"\"{registryAttributes.Value}\"=");
                                                outputText += $"\"{registryAttributes.Value}\"=";
                                            }
                                            else
                                            {
                                                if (registryAttributes.Name == "value")
                                                {
                                                    value = registryAttributes.Value;
                                                }
                                                else if (registryAttributes.Name == "valueType")
                                                {
                                                    if (registryAttributes.Value == "REG_SZ")
                                                    {
                                                        if (m.Verbose) Console.WriteLine($"\"{value}\"");
                                                        outputText += $"\"{value}\"\n";
                                                    }
                                                    else if (registryAttributes.Value == "REG_EXPAND_SZ")
                                                    {
                                                        string result = StringToHex(value);
                                                        if (m.Verbose) Console.WriteLine($"hex(2):{result}");
                                                        outputText += $"hex(2):{result}\n";
                                                    }
                                                    else if (registryAttributes.Value == "REG_BINARY")
                                                    {
                                                        string result = StringToBytes(value);
                                                        if (m.Verbose) Console.WriteLine($"hex:{result}");
                                                        outputText += $"hex:{result}\n";
                                                    }
                                                    else if (registryAttributes.Value == "REG_DWORD")
                                                    {
                                                        string result = value.Replace("0x", "");
                                                        if (m.Verbose) Console.WriteLine($"dword:{result}");
                                                        outputText += $"dword:{result}\n";
                                                    }
                                                    else if (registryAttributes.Value == "REG_MULTI_SZ")
                                                    {
                                                        string result = StringToHex(value);
                                                        if (m.Verbose) Console.WriteLine($"hex(7):{result}");
                                                        outputText += $"hex(7):{result}\n";
                                                    }
                                                    else if (registryAttributes.Value == "REG_QWORD")
                                                    {
                                                        /*if (m.Verbose) Console.WriteLine($"hex(b):{value}");
                                                        OutputText += $"hex(b):{value}\n";*/
                                                    }

                                                }
                                            }
                                        }
                                        value = "";
                                    }
                                }
                                if (m.Verbose) Console.WriteLine("");
                                outputText += "\n";
                            }
                        }
                    }
                    Console.WriteLine("Writing registry...");
                    string fileName = Path.GetFileNameWithoutExtension(inputFile);
                    File.WriteAllText(outputFile, outputText);
                    Console.Write("Conversion completed.");
                    Console.ReadLine();
                });
                
            }
            catch (Exception ex)
            {
                var defaultColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                Console.ForegroundColor = defaultColor;
                Environment.Exit(0);
            }
        }

        public static string StringToBytes(string str)
        {
            var groups = str.Select((c, ix) => new { Char = c, Index = ix }).GroupBy(x => x.Index / 2).Select(g => String.Concat(g.Select(x => x.Char)));
            return string.Join(",", groups);
        }

        public static string StringToHex(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in $"{str.Replace("\"", "").Replace(",", "\0")}\0\0")
            {
                List<string> splittedParts = SplitInParts(string.Format("{0:X4}", (int)c), 2).ToList();
                splittedParts.Reverse();
                sb.AppendFormat(string.Join("", splittedParts));
            }
            var groups = sb.ToString().Select((c, ix) => new { Char = c, Index = ix }).GroupBy(x => x.Index / 2).Select(g => String.Concat(g.Select(x => x.Char)));
            return string.Join(",", groups);
        }

        public static IEnumerable<string> SplitInParts(string s, int partLength)
        {
            for (int i = 0; i < s.Length; i += partLength)
            {
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
            }
        }

        internal class Options
        {
            [Option('m', "manifest-file", HelpText = @"A path to the manifest file to convert.", Required = true)]
            public string ManFile { get; set; }

            [Option('r', "registry-file", HelpText = "A path to the registry file to output.", Required = true)]
            public string OutPath { get; set; }

            [Option('v', "verbose", HelpText = "show detailed progress messages", Required = false, Default = false)]
            public bool Verbose { get; set; }
        }
    }
}

