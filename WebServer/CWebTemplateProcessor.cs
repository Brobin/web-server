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
            //Read each line in
            // Figure out if its html or cscript
                // if its html, simply print
                // if its cscript print, wout(value)
                    // find closing brackets
                // if cscript statement, write it
                    // find opening and closing brackets
            // Write that all to a stream
            // Pass that stream to cscript processor

            // Read in every line of the cscript

            StreamReader reader = new StreamReader(stream);
            StringBuilder builder = new StringBuilder();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                builder.AppendLine(_WriteHtml(line));
            }
            string output = builder.ToString();

            Stream outputStream = StringToStream(output);

            var processor = new CscriptProcessor();
            return processor.ProcessScript(outputStream, requestParameters);
        }

        public string _WriteHtml(string html)
        {
            string escapedHtml = html.Replace('"', '\"');
            return String.Format("wout.WriteLine(\"{0}\");", escapedHtml);
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
