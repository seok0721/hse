using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using FileExtensionContracts;

namespace Client.Service.File
{
    public class MSPowerPointReader : IFileReader
    {
        public string FilePath { get; set; }
        private PowerPoint.Application powerPointApp = null;

        private PowerPoint.Presentations multiPresentations = null;
        private PowerPoint.Presentation presentation = null;

        public MSPowerPointReader(string filePath)
        {
            this.FilePath = filePath;
        }

        public string Read()
        {
            try
            {
                powerPointApp = new PowerPoint.Application();
                //powerPointApp.Visible = MsoTriState.msoFalse;
                multiPresentations = powerPointApp.Presentations;

                MsoTriState trueState = MsoTriState.msoTrue;
                MsoTriState falseState = MsoTriState.msoFalse;
                string filePath = (string)FilePath;

                presentation = multiPresentations.Open(filePath,
                                                       trueState,
                                                       falseState,
                                                       falseState);

                string result = string.Empty;

                for (int i = 0; i < presentation.Slides.Count; i++)
                {
                    foreach (var item in presentation.Slides[i + 1].Shapes)
                    {
                        var shape = (PowerPoint.Shape)item;
                        if (shape.HasTextFrame == MsoTriState.msoTrue && shape.TextFrame.HasText == MsoTriState.msoTrue)
                        {
                            var textRange = shape.TextFrame.TextRange;
                            result += textRange.Text;
                        }
                    }
                }

                powerPointApp.Quit();

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
