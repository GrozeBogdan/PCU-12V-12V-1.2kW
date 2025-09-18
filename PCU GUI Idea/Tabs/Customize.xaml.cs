using PCU_GUI_Idea.Menu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Spreadsheet.Model;
using Excel = Microsoft.Office.Interop.Excel;
using static ThemePresets;
using Telerik.Windows.Documents.Spreadsheet.Model.ConditionalFormattings;
using Microsoft.Office.Interop.Excel;
using Telerik.Windows.Documents.Fixed.Model.Graphics;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Windows.Controls.Spreadsheet.Worksheets;
using PCU_GUI_Idea.Tabs.InstrumentsUC;
using PCU_GUI_Idea.Tabs.Converter_UserControls;
using Application = System.Windows.Application;
using PCU_GUI_Idea.Modules;

namespace PCU_GUI_Idea.Tabs
{
    /// <summary>
    /// Interaction logic for Customize.xaml
    /// </summary>
    public partial class Customize : UserControl
    {
        public Customize()
        {
            InitializeComponent();
        }
        private void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var radComboBox = sender as RadComboBox;
            RadComboBoxItem item = radComboBox.SelectedItem as RadComboBoxItem; 
            if (radComboBox != null && item != null)
            {
                var theme = Themes[item.Content.ToString()];
                ThemeManager.ApplyTheme(theme);
            }
            else
                MessageBox.Show("Nu merge daca nu alegi nimic");
        }
        private void SearchForDatabases(object sender, EventArgs e)
        {
            string directory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database";
            foreach(string file in Directory.GetFiles(directory)) 
            {
                if (databaseBox.Items.Contains(file.Replace(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database\\", "")) == false)
                {
                    if (file.Contains(".dbc") || file.Contains(".ldf"))
                    {
                        databaseBox.Items.Add(file.Replace(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database\\", ""));
                    }
                }
            }
        }
        private void LoadDatabase(object sender, SelectionChangedEventArgs e)
        {
            if(DbcParser.Messages != null || LdfParser.Frames != null)
            {
                DbcParser.Messages = null;
                LdfParser.Frames = null;
            }

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            RadComboBox radComboBox = sender as RadComboBox;
            if (radComboBox.SelectedItem.ToString().Contains(".dbc"))
                DbcParser.ParseDatabase(radComboBox.SelectedItem.ToString());
            else
                LdfParser.ParseDatabase(radComboBox.SelectedItem.ToString());
            ConvertorUCSelector.UpdateSignalBind(true);

        }

        private void Generate_Excel(object sender, RoutedEventArgs e)
        {
            string directory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Excel Templates/Excel Data";
            XlsxFormatProvider formatProvider = new XlsxFormatProvider();

            if (File.Exists(directory + "\\template.xlsx"))
            {
                using (Stream input = new FileStream(directory + "\\template.xlsx", FileMode.Open))
                {
                    this.excelTable.Workbook = formatProvider.Import(input);
                    input.Dispose();
                }

            }

            else
            {
                Excel.Application app = new Excel.Application
                {
                    Visible = true
                };
                Excel.Worksheet worksheet;
                string date = DateTime.Now.Date.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToString("HH-mm");

                // Creating the sheet
                Excel.Workbook workbook = app.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                worksheet = (Excel.Worksheet)workbook.Sheets[1];

                // Nameing the sheet as the date + hour
                worksheet.Name = date;

                Dynamic_Generated_Template_3D_Chart(worksheet, workbook, int.Parse(excel_Current.Text), int.Parse(excel_Frequency.Text));

                workbook.SaveAs(directory + "\\template.xlsx");

                workbook.Close();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                app.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(app);

                using (Stream input = new FileStream(directory + "\\template.xlsx", FileMode.Open))
                {
                    this.excelTable.Workbook = formatProvider.Import(input);
                }

                
            }
        }
        private void Dynamic_Generated_Template_3D_Chart(Excel.Worksheet sheet, Excel.Workbook workbook, int maxCurrent, int maxFrequency)
        {
            // Making the header and centering it
            Excel.Range mergeRange = sheet.Range["B2:E3"];
            mergeRange.Merge();
            sheet.Cells[2, 2].Style = workbook.Styles["Good"];
            sheet.Cells[2, 2].Font.Size = 16;
            sheet.Cells[2, 2].Font.Name = "Franklin Gothic Demi";
            sheet.Cells[2, 2] = "3D Chart";
            sheet.Cells[2, 2].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            sheet.Cells[2, 2].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

            mergeRange = sheet.Range["G2:H2"];
            mergeRange.Merge();
            sheet.Cells[2, 7].Style = workbook.Styles["Good"];
            sheet.Cells[2, 7].Font.Size = 16;
            sheet.Cells[2, 7].Font.Name = "Franklin Gothic Demi";
            sheet.Cells[2, 7] = "Voltage";
            sheet.Cells[2, 7].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            sheet.Cells[2, 7].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

            // supplyPort.WriteLine($"V{1}O?");
            // string voltage = supplyPort.ReadLine().Replace("V\r", "");

            sheet.Cells[3, 7].Font.Size = 16;
            sheet.Cells[3, 7].Font.Name = "Franklin Gothic Demi";
            // sheet.Cells[3, 7] = voltage;
            // Making the table

            // Freq/Current Cell
            sheet.Cells[5, 2].Style = workbook.Styles["Good"];
            sheet.Cells[5, 2].Font.Size = 11;
            sheet.Cells[5, 2].borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            sheet.Cells[5, 2].borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            sheet.Cells[5, 2].borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            sheet.Cells[5, 2].borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
            sheet.Cells[5, 2].borders[Excel.XlBordersIndex.xlDiagonalDown].LineStyle = Excel.XlLineStyle.xlContinuous;
            sheet.Cells[5, 2] = "           Frequency\r\nCurrent";
            sheet.Cells[5, 2].ColumnWidth = 14.5;
            sheet.Cells[5, 2].RowHeight = 30;

            //
            // Frequency row
            int j = 0;
            //for (int i = 3; i < int.Parse(col.Text) + 3; i++)
            for (int i = 3; i < maxFrequency/50 + 2; i++)
            {
                sheet.Cells[5, i].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Cells[5, i].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                sheet.Cells[5, i] = 100 + j;
                j = j + 50;
            }

            // Define starting cell in the specified column and row
            Excel.Range startCell = sheet.Cells[5, 3];
            Excel.Range lastCell = startCell.End[Excel.XlDirection.xlToRight];

            Excel.Range range = sheet.get_Range(startCell, lastCell);
            Excel.Borders borders = range.Borders;

            borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;

            // Add a 2-color scale to the specified range
            Excel.ColorScale colorScale = (Excel.ColorScale)range.FormatConditions.AddColorScale(2);

            // Set the minimum criteria (type and color)
            colorScale.ColorScaleCriteria[1].Type = Excel.XlConditionValueTypes.xlConditionValueLowestValue;
            colorScale.ColorScaleCriteria[1].FormatColor.Color = Excel.XlRgbColor.rgbSkyBlue;

            // Set the maximum criteria (type and color)
            colorScale.ColorScaleCriteria[2].Type = Excel.XlConditionValueTypes.xlConditionValueHighestValue;
            colorScale.ColorScaleCriteria[2].FormatColor.Color = Excel.XlRgbColor.rgbSlateBlue;

            //
            // Current row
            j = 0;
            //for (int i = 6; i < int.Parse(rows.Text) + 6; i++)
            for (int i = 6; i < maxCurrent + 6; i++)
            {
                sheet.Cells[i, 2].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Cells[i, 2].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                sheet.Cells[i, 2] = 1 + j;
                j++;
            }

            range = sheet.get_Range("B6", "B" + (sheet.UsedRange.Rows.Count + 1));
            borders = range.Borders;

            // Add borders
            borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;

            // Add a 2-color scale to the specified range
            colorScale = (Excel.ColorScale)range.FormatConditions.AddColorScale(2);

            // Set the minimum criteria (type and color)
            colorScale.ColorScaleCriteria[1].Type = Excel.XlConditionValueTypes.xlConditionValueLowestValue;
            colorScale.ColorScaleCriteria[1].FormatColor.Color = Excel.XlRgbColor.rgbYellow;

            // Set the maximum criteria (type and color)
            colorScale.ColorScaleCriteria[2].Type = Excel.XlConditionValueTypes.xlConditionValueHighestValue;
            colorScale.ColorScaleCriteria[2].FormatColor.Color = Excel.XlRgbColor.rgbOrange;

            //
            //
            // Values row
            startCell = sheet.Cells[6, 3];
            lastCell = sheet.Cells[sheet.UsedRange.Rows.Count + 1, sheet.UsedRange.Columns.Count + 1];

            range = sheet.get_Range(startCell, lastCell);
            borders = range.Borders;

            // Add borders
            borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;

            // Add a 3-color scale to the specified range
            Excel.ColorScale colorScale3 = (Excel.ColorScale)range.FormatConditions.AddColorScale(3);

            // Define the minimum criteria (type and color)
            colorScale3.ColorScaleCriteria[1].Type = Excel.XlConditionValueTypes.xlConditionValueLowestValue;
            colorScale3.ColorScaleCriteria[1].FormatColor.Color = Excel.XlRgbColor.rgbOrangeRed; // Green

            // Define the midpoint criteria (type, value, and color)
            colorScale3.ColorScaleCriteria[2].Type = Excel.XlConditionValueTypes.xlConditionValuePercentile;
            colorScale3.ColorScaleCriteria[2].Value = 50;
            colorScale3.ColorScaleCriteria[2].FormatColor.Color = Excel.XlRgbColor.rgbYellow; // Yellow

            // Define the maximum criteria (type and color)
            colorScale3.ColorScaleCriteria[3].Type = Excel.XlConditionValueTypes.xlConditionValueHighestValue;
            colorScale3.ColorScaleCriteria[3].FormatColor.Color = Excel.XlRgbColor.rgbGreen; // Red
        }

        private async void Converter_Efficency_Model(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            RP7945A_UC load = (RP7945A_UC)mainWindow.instruments_UC.deviceRight.Content;
            Telerik.Windows.Documents.Spreadsheet.Model.Workbook workbook = excelTable.Workbook;
            Telerik.Windows.Documents.Spreadsheet.Model.Worksheet sheet = (Telerik.Windows.Documents.Spreadsheet.Model.Worksheet)workbook.ActiveSheet ;

            CellRange usedRange = excelTable.ActiveWorksheet.UsedCellRange;
            CellRange workingRange = new CellRange(5, 2, usedRange.ToIndex.RowIndex, usedRange.ToIndex.ColumnIndex); // (5 , 2) -> C6 cell ; programmers count from 0;

            int increment = 1;
            
            for(int collumn = 2; collumn <= workingRange.ToIndex.ColumnIndex; collumn++) 
            {
                if (collumn == 3)
                    return;
                increment = 1;
                for (int rows = 5; rows <= workingRange.ToIndex.RowIndex; rows++)
                {
                    load.supplyCurrentCH1.Text = increment.ToString();
                    load.SendSupplyValue2(null,null);
                    await Task.Delay(3000);
                    sheet.Cells[rows, collumn].SetValue(Instruments.efficiency_excel);
                    increment++;
                }

                if (increment == 51)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        var test = increment - (i * 10 + 1) ;
                        if (test == 0)
                            test = 1;
                        load.supplyCurrentCH1.Text = test.ToString();
                        load.SendSupplyValue2(null, null);
                        await Task.Delay(2000);
                    }
                }
            }

            XlsxFormatProvider formatProvider = new XlsxFormatProvider();

            using (Stream output = new FileStream(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Excel Templates/Excel Data/test123.xlsx", FileMode.Create))
            {
                formatProvider.Export(excelTable.Workbook, output);
            }
        }
    }
}
