using FileExtensionContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Client.Service.File
{
    public class MSExcelReader : IFileReader
    {
        public string FilePath { get; set; }
        private Excel.Application excelApp = null;

        private Excel.Workbook workBook = null;
        private Excel._Worksheet objectSheet = null;

        public MSExcelReader(string filePath)
        {
            this.FilePath = filePath;
        }

        public string Read()
        {
            try
            {
                excelApp = new Excel.Application();
                excelApp.Visible = false;

                object missingValue = System.Reflection.Missing.Value;
                object readOnly = true;

                workBook = excelApp.Workbooks.Open(FilePath,
                                                   missingValue,
                                                   readOnly,
                                                   missingValue, 
                                                   missingValue,
                                                   missingValue,
                                                   missingValue,
                                                   missingValue,    
                                                   missingValue,
                                                   missingValue,
                                                   missingValue,
                                                   missingValue,
                                                   missingValue,
                                                   missingValue,
                                                   missingValue);

                objectSheet = (Excel.Worksheet)excelApp.ActiveSheet;

                // Get full range which used.
                Excel.Range usedRange = objectSheet.UsedRange;
                string result = string.Empty;

                for (int i = 1; i <= usedRange.Rows.Count; i++)
                {
                    for (int j = 1; j <= usedRange.Columns.Count; j++)
                    {
                        result += usedRange.Cells[i, j].Text;
                    }
                }

                excelApp.Quit();

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
