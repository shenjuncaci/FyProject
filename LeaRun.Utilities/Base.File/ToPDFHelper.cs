using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Threading;
using Microsoft.Office.Interop;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Core;
using System.Xml;

namespace LeaRun.Utilities
{
    public static class ToPDFHelper
    {

        /// <summary> 
        /// 将word文档转换成PDF格式 
        /// </summary> 
        /// <param name="sourcePath"></param> 
        /// <param name="targetPath"></param> 
        /// <returns></returns> 
        public static bool ConvertWord2Pdf(string sourcePath, string targetPath)
        {
            try
            {
                bool result;
                Microsoft.Office.Interop.Word.WdExportFormat exportFormat = Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF;
                object paramMissing = Type.Missing;
                Microsoft.Office.Interop.Word.Application wordApplication = new Microsoft.Office.Interop.Word.Application();
                Microsoft.Office.Interop.Word.Document wordDocument = null;
                
                    sourcePath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\" + sourcePath;
                    targetPath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\" + targetPath;
                object paramSourceDocPath = sourcePath;
                    string paramExportFilePath = targetPath;
                    Microsoft.Office.Interop.Word.WdExportFormat paramExportFormat = exportFormat;
                    Microsoft.Office.Interop.Word.WdExportOptimizeFor paramExportOptimizeFor =
                            Microsoft.Office.Interop.Word.WdExportOptimizeFor.wdExportOptimizeForPrint;
                    Microsoft.Office.Interop.Word.WdExportRange paramExportRange = Microsoft.Office.Interop.Word.WdExportRange.wdExportAllDocument;
                    int paramStartPage = 0;
                    int paramEndPage = 0;
                    Microsoft.Office.Interop.Word.WdExportItem paramExportItem = Microsoft.Office.Interop.Word.WdExportItem.wdExportDocumentContent;
                    Microsoft.Office.Interop.Word.WdExportCreateBookmarks paramCreateBookmarks =
                            Microsoft.Office.Interop.Word.WdExportCreateBookmarks.wdExportCreateWordBookmarks;

                    wordDocument = wordApplication.Documents.Open(
                            ref paramSourceDocPath, ref paramMissing, ref paramMissing,
                            ref paramMissing, ref paramMissing, ref paramMissing,
                            ref paramMissing, ref paramMissing, ref paramMissing,
                            ref paramMissing, ref paramMissing, ref paramMissing,
                            ref paramMissing, ref paramMissing, ref paramMissing,
                            ref paramMissing);
                    if (wordDocument != null)
                        wordDocument.ExportAsFixedFormat(paramExportFilePath,
                                paramExportFormat, false,
                                paramExportOptimizeFor, paramExportRange, paramStartPage,
                                paramEndPage, paramExportItem, true,
                                true, paramCreateBookmarks, true,
                                true, false,
                                ref paramMissing);
                    result = true;
                    if (wordDocument != null)
                    {
                        wordDocument.Close(ref paramMissing, ref paramMissing, ref paramMissing);
                        wordDocument = null;
                    }
                    if (wordApplication != null)
                    {
                        wordApplication.Quit(ref paramMissing, ref paramMissing, ref paramMissing);
                        wordApplication = null;
                    }
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    return result;
                }
                catch (Exception ex)
                {
                string filepath = HttpContext.Current.Server.MapPath("\\Resource\\temp.xml");
                StreamReader str = new StreamReader(filepath, Encoding.UTF8);
                XmlDocument xml = new XmlDocument();
                xml.SelectSingleNode("xml").SelectSingleNode("log").InnerText = ex.Message;
                return false;
                }

            
            
            
            
        }

        /// <summary> 
        /// 将excel文档转换成PDF格式 
        /// </summary> 
        /// <param name="sourcePath"></param> 
        /// <param name="targetPath"></param> 
        /// <returns></returns> 
        public static bool ConvertExcel2Pdf(string sourcePath, string targetPath)
        {
            sourcePath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\" + sourcePath;
            targetPath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\" + targetPath;
            bool result;
            object missing = Type.Missing;

            Microsoft.Office.Interop.Excel.XlFixedFormatType targetType = Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF;
            Microsoft.Office.Interop.Excel.Application application = null;
            Microsoft.Office.Interop.Excel.Workbook workBook = null;
            try
            {
                application = new Microsoft.Office.Interop.Excel.Application();
                object target = targetPath;
                workBook = application.Workbooks.Open(sourcePath, missing, missing, missing, missing, missing,
                        missing, missing, missing, missing, missing, missing, missing, missing, missing);
                workBook.ExportAsFixedFormat(targetType, target, Microsoft.Office.Interop.Excel.XlFixedFormatQuality.xlQualityStandard, true, false, missing, missing, missing, missing);
                result = true;
            }
            catch(Exception ex)
            {
                string filepath = HttpContext.Current.Server.MapPath("\\Resource\\temp.xml");
                StreamReader str = new StreamReader(filepath, Encoding.UTF8);
                XmlDocument xml = new XmlDocument();
                xml.SelectSingleNode("xml").SelectSingleNode("log").InnerText = ex.Message;
                result = false;
            }
            finally
            {
                if (workBook != null)
                {
                    workBook.Close(true, missing, missing);
                    workBook = null;
                }
                if (application != null)
                {
                    application.Quit();
                    application = null;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return result;
        }
        /// <summary> 
        /// 将ppt文档转换成PDF格式 
        /// </summary> 
        /// <param name="sourcePath"></param> 
        /// <param name="targetPath"></param> 
        /// <returns></returns> 
        public static bool ConvertPowerPoint2Pdf(string sourcePath, string targetPath)
        {
            sourcePath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\" + sourcePath;
            targetPath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\" + targetPath;
            bool result;
            Microsoft.Office.Interop.PowerPoint.PpSaveAsFileType targetFileType = Microsoft.Office.Interop.PowerPoint.PpSaveAsFileType.ppSaveAsPDF;
            Microsoft.Office.Interop.PowerPoint.Application application = null;
            Microsoft.Office.Interop.PowerPoint.Presentation persentation = null;
            try
            {
                application = new Microsoft.Office.Interop.PowerPoint.Application();
                persentation = application.Presentations.Open(sourcePath, MsoTriState.msoTrue, MsoTriState.msoFalse, MsoTriState.msoFalse);
                persentation.SaveAs(targetPath, targetFileType, MsoTriState.msoTrue);
                result = true;
            }
            catch(Exception ex)
            {
                string filepath = HttpContext.Current.Server.MapPath("\\Resource\\temp.xml");
                StreamReader str = new StreamReader(filepath, Encoding.UTF8);
                XmlDocument xml = new XmlDocument();
                xml.SelectSingleNode("xml").SelectSingleNode("log").InnerText = ex.Message;
                result = false;
            }
            finally
            {
                if (persentation != null)
                {
                    persentation.Close();
                    persentation = null;
                }
                if (application != null)
                {
                    application.Quit();
                    application = null;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return result;
        }


    }
}
