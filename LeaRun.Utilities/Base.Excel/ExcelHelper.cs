using System.Data;
using System.IO;
using System.Web;


namespace LeaRun.Utilities
{
    public class ExcelHelper
    {
        public string Encoding = "UTF-8";
        System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;

        public void EcportExcel(DataTable dt, string fileName)
        {

            if (dt != null)
            {
                StringWriter sw = new StringWriter();
                CreateStringWriter(dt, ref sw);
                sw.Close();
                //response.Clear();
                //response.Buffer = true;
                //response.Charset = Encoding;
                ////this.EnableViewState = false;
                //response.AddHeader("Content-Disposition", "attachment; filename=" + fileName + ".xls");
                //response.ContentType = "application/ms-excel";

                //response.ContentEncoding = System.Text.Encoding.GetEncoding(Encoding)
                //response.ContentEncoding = System.Text.Encoding.UTF8;
                //response.Write(sw);
                //response.End();

                HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
                HttpContext.Current.Response.Charset = "Utf-8";
                HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName + ".xls", System.Text.Encoding.UTF8));
                //"<meta http-equiv=\"content-type\" content=\"application/ms-excel; charset=UTF-8\"/>"  
                //添加这段字符串以后解决部分中文有乱码的问题
                HttpContext.Current.Response.Write("<meta http-equiv=\"content-type\" content=\"application/ms-excel; charset=UTF-8\"/>"+sw.ToString());
                HttpContext.Current.Response.End();
            }

        }

        private void CreateStringWriter(DataTable dt, ref StringWriter sw)
        {
            string sheetName = "sheetName";

            sw.WriteLine("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
            sw.WriteLine("<head>");
            sw.WriteLine("<!--[if gte mso 9]>");
            sw.WriteLine("<xml>");
            sw.WriteLine(" <x:ExcelWorkbook>");
            sw.WriteLine(" <x:ExcelWorksheets>");
            sw.WriteLine(" <x:ExcelWorksheet>");
            sw.WriteLine(" <x:Name>" + sheetName + "</x:Name>");
            sw.WriteLine(" <x:WorksheetOptions>");
            sw.WriteLine(" <x:Print>");
            sw.WriteLine(" <x:ValidPrinterInfo />");
            sw.WriteLine(" </x:Print>");
            sw.WriteLine(" </x:WorksheetOptions>");
            sw.WriteLine(" </x:ExcelWorksheet>");
            sw.WriteLine(" </x:ExcelWorksheets>");
            sw.WriteLine("</x:ExcelWorkbook>");
            sw.WriteLine("</xml>");
            sw.WriteLine("<![endif]-->");
            sw.WriteLine("</head>");
            sw.WriteLine("<body>");
            sw.WriteLine("<table>");
            sw.WriteLine(" <tr>");
            string[] columnArr = new string[dt.Columns.Count];
            int i = 0;
            foreach (DataColumn columns in dt.Columns)
            {

                sw.WriteLine(" <td>" + columns.ColumnName + "</td>");
                columnArr[i] = columns.ColumnName;
                i++;
            }
            sw.WriteLine(" </tr>");

            foreach (DataRow dr in dt.Rows)
            {
                sw.WriteLine(" <tr>");
                foreach (string columnName in columnArr)
                {
                    sw.WriteLine(" <td style='vnd.ms-excel.numberformat:@'>" + dr[columnName] + "</td>");
                }
                sw.WriteLine(" </tr>");
            }
            sw.WriteLine("</table>");
            sw.WriteLine("</body>");
            sw.WriteLine("</html>");
            
        }
    }
}