﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    class CWebTemplateProcessor : IScriptProcessor
    {
        public ScriptResult ProcessScript(Stream stream, IDictionary<string, string> requestParameters)
        {

            StreamReader reader = new StreamReader(stream);
            List<String> lines = new List<String>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                lines.Add(line);
            }
            string output = ProcessScript(lines);

            Stream outputStream = StringToStream(output);

            var processor = new CscriptProcessor();
            return processor.ProcessScript(outputStream, requestParameters);
        }

        public string ProcessScript(List<string> lines)
        {
            StringBuilder builder = new StringBuilder();
            // Try-Catch for request parameters
            // Loop through ech line of the template
            for(int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                // Print a varible
                if (line.Contains("request[")) builder.Append("try{");
                if (line.Contains("@{"))
                {
                    line = line.Replace("@", "");
                    var parts = line.Split(new char[] {'{', '}'});
                    builder.Append(_WriteHtml(parts[0]));
                    for (int j = 1; j < parts.Length; j++ )
                    {
                        builder.Append(_WriteVariable(parts[j]));
                        j++;
                        builder.Append(_WriteHtml(parts[j]));
                    }
                }
                // Code block
                else if (line.Contains("{"))
                {
                    // find closing tag
                    int closing = FindClosingTag(i, lines);
                    // append everything within the tags
                    for(i = i+1; i < closing; i++)
                    {
                        builder.Append(lines[i]);
                    }

                }
                // HTML output
                else
                {
                    builder.AppendLine(_WriteHtml(line));
                }
                if (line.Contains("request[")) builder.Append("}catch(Exception e){wout.WriteLine(\"not found\");}");
            }
            
            return builder.ToString();
        }

        public int FindClosingTag(int i, List<string> lines)
        {
            int otherBraces = 0;
            bool done = false;
            while(!done || otherBraces != 0)
            {
                i++;
                string line = lines[i];
                if(line.Contains("{"))
                {
                    otherBraces++;
                }
                else if (line.Contains("}"))
                {
                    if (otherBraces == 0) done = true;
                    else otherBraces--;
                }
            }
            return i;
        }

        public string _WriteHtml(string html)
        {
            string escapedHtml = html.Replace("\"", "\\\"");
            return String.Format("wout.WriteLine(\"{0}\");", escapedHtml);
        }

        public string _WriteVariable(string variable)
        {
            return String.Format("wout.WriteLine({0}.Replace(\"%20\",\" \"));", variable);
        }

        public Stream StringToStream(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
