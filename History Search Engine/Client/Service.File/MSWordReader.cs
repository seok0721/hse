using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;

namespace Client.Service.File
{
    public class MSWordReader : MSOfficeReader
    {
        public MSWordReader(string filePath)
        {
            this.FilePath = filePath;
        }

        override public string Read()
        {
            try
            {
                Word.Application app = new Word.Application();

                object missingValue = System.Reflection.Missing.Value;
                object readOnly = true;
                object filePath = FilePath;

                Word.Document docs = app.Documents.Open(ref filePath,
                                                   ref missingValue,
                                                   ref missingValue,
                                                   ref readOnly, // 4 
                                                   ref missingValue,
                                                   ref missingValue,
                                                   ref missingValue,
                                                   ref missingValue, // 8
                                                   ref missingValue,
                                                   ref missingValue,
                                                   ref missingValue,
                                                   ref missingValue, // 12
                                                   ref missingValue,
                                                   ref missingValue,
                                                   ref missingValue,
                                                   ref missingValue);
                docs.Activate();

                string totalText = "";

                for (int i = 0; i < docs.Paragraphs.Count; i++)
                {
                    totalText += "\r\n" + docs.Paragraphs[i + 1].Range.Text.ToString();
                }

                docs.Close(missingValue,missingValue,missingValue);
                app.Quit(missingValue,missingValue,missingValue);

                return totalText;
            }
            catch (Exception e)
            {
                // not handle the exception in this method
                throw e;
            }
        }
    }
}
