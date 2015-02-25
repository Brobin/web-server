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
            // read the whole file into a string
            StreamReader reader = new StreamReader(stream);
            string output = ProcessScript(reader.ReadToEnd());

            // convert the output to a stream
            Stream outputStream = StringToStream(output);

            // use the processor to get the result
            var processor = new CscriptProcessor();
            return processor.ProcessScript(outputStream, requestParameters);
        }

        public string ProcessScript(string file)
        {
            StringBuilder result = new StringBuilder();
            int length = file.Length;
            int beginning = 0;

            for (int i = 0; i < length; i++ )
            {
                // check for a code or variable block
                Boolean codeBlock = (file.ElementAt(i) == '{');
                Boolean variableBlock = (i < length - 1 && file.Substring(i, 2) == "@{");

                if(codeBlock || variableBlock)
                {
                    // output the html up tot his point
                    string html = file.Substring(beginning, i - beginning);
                    result.Append(_WriteHtml(html));

                    // account for the @
                    if (variableBlock) i++;

                    // find the closing tag and split to get the code
                    int end = FindClosingTag(i, file);
                    string code = file.Substring(i + 1, end - i - 1);

                    // account for missing request parameters
                    Boolean request = (code.Contains("request["));
                    // add the try
                    if (request) result.Append("try{");

                    // append the code to the StringBuilder
                    if (codeBlock) result.Append(code);
                    else if (variableBlock) result.Append(_WriteVariable(code));

                    // add the catch
                    if (request) result.Append("} catch(Exception e) { wout.WriteLine(\"not found\"); }");

                    // fix up the variables to account for skipped characters
                    i = end;
                    beginning = i + 1;
                }
            }
            string htmlLeft = file.Substring(beginning);
            result.Append(_WriteHtml(htmlLeft));

            return result.ToString();
        }

        public int FindClosingTag(int i, string file)
        {
            int count = 1;
            int j = i;
            while (count > 0)
            {
                j++;
                if (file.ElementAt(j) == '{') count++;
                else if (file.ElementAt(j) == '}') count--;
            }
            return j;
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
