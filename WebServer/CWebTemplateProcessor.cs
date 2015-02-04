using System;
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
            builder.Append("try{");
            // Loop through ech line of the template
            for(int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                // Print a varible
                if (line.Contains("@{"))
                {
                    line = line.Replace("@", "");
                    var parts = line.Split(new char[] {'{', '}'});
                    builder.Append(_WriteHtml(parts[0]));
                    builder.Append(_WriteVariable(parts[1]));
                    builder.Append(_WriteHtml(parts[2]));
                }
                // Code block
                else if (line.Contains("{"))
                {
                    // Loop until the end of the block
                    while(!line.Contains("}"))
                    {
                        i++;
                        line = lines[i];
                        if (!line.Contains("}"))
                        {
                            builder.Append(line);
                        }
                    }
                }
                // HTML output
                else
                {
                    builder.AppendLine(_WriteHtml(line));
                }
            }
            builder.Append("}catch(Exception e){}");
            return builder.ToString();
        }

        public string _WriteHtml(string html)
        {
            string escapedHtml = html.Replace('"', '\"');
            return String.Format("wout.WriteLine(\"{0}\");", escapedHtml);
        }

        public string _WriteVariable(string variable)
        {
            return String.Format("wout.WriteLine({0});", variable);
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
