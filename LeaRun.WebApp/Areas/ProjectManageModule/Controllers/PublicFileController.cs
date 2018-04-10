using LeaRun.Business;
using LeaRun.DataAccess;
using LeaRun.Entity;
using LeaRun.Repository;
using LeaRun.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;

namespace LeaRun.WebApp.Areas.ProjectManageModule.Controllers
{
    public class PublicFileController : Controller
    {
        //
        // GET: /ProjectManageModule/PublicFile/
        RepositoryFactory<PM_PublicFile> repositoryfactory = new RepositoryFactory<PM_PublicFile>();
        PM_PublicFileBll FileBll = new PM_PublicFileBll();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetExistsList()
        {
            int canDel = 0;
            if (ManageProvider.Provider.Current().ObjectId.IndexOf("15342978-8090-4244-96ec-947472346fff")>-1)
            {
                canDel = 1;
            }

            string sql = @" select *,"+canDel+ " as canDel from PM_PublicFile where 1=1 ";
            
            //sql = string.Format(sql, FollowID, ManageProvider.Provider.Current().UserId);
            DataTable dt = FileBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult SubmitUploadify(HttpPostedFileBase Filedata)
        {
            try
            {
                Thread.Sleep(1000);////延迟500毫秒
                PM_PublicFile entity = new PM_PublicFile();
                string IsOk = "";
                //没有文件上传，直接返回
                if (Filedata == null || string.IsNullOrEmpty(Filedata.FileName) || Filedata.ContentLength == 0)
                {
                    return HttpNotFound();
                }
                //获取文件完整文件名(包含绝对路径)
                //文件存放路径格式：/Resource/Document/NetworkDisk/{日期}/{guid}.{后缀名}
                //例如：/Resource/Document/Email/20130913/43CA215D947F8C1F1DDFCED383C4D706.jpg
                string fileGuid = CommonHelper.GetGuid;
                long filesize = Filedata.ContentLength;
                string FileEextension = Path.GetExtension(Filedata.FileName);
                string uploadDate = DateTime.Now.ToString("yyyyMMdd");
                //string UserId = ManageProvider.Provider.Current().UserId;

                string virtualPath = string.Format("~/Resource/Document/NetWorkDisk/ProjectManageFile/{0}{1}", fileGuid, FileEextension);
                string fullFileName = this.Server.MapPath(virtualPath);
                //创建文件夹，保存文件
                string path = Path.GetDirectoryName(fullFileName);
                Directory.CreateDirectory(path);
                if (!System.IO.File.Exists(fullFileName))
                {
                    Filedata.SaveAs(fullFileName);
                    try
                    {

                        //文件信息写入数据库
                        entity.Create();
                        entity.FilePath = virtualPath;
                        
                        entity.FileName = Filedata.FileName;
                        //entity.FilePath = virtualPath;
                        entity.FileSize = filesize.ToString();
                        entity.FileExtensions = FileEextension;
                        string _FileType = "";
                        string _Icon = "";
                        this.DocumentType(FileEextension, ref _FileType, ref _Icon);
                        entity.Icon = _Icon;
                        entity.FileType = _FileType;
                        IsOk = DataFactory.Database().Insert<PM_PublicFile>(entity).ToString();
                    }
                    catch (Exception ex)
                    {
                        IsOk = ex.Message;
                        //System.IO.File.Delete(virtualPath);
                    }
                }

               

                var JsonData = new
                {
                    Status = IsOk,
                    NetworkFile = entity,
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public void DocumentType(string Eextension, ref string FileType, ref string Icon)
        {
            string _FileType = "";
            string _Icon = "";
            switch (Eextension)
            {
                case ".docx":
                    _FileType = "word文件";
                    _Icon = "doc";
                    break;
                case ".doc":
                    _FileType = "word文件";
                    _Icon = "doc";
                    break;
                case ".xlsx":
                    _FileType = "excel文件";
                    _Icon = "xls";
                    break;
                case ".xls":
                    _FileType = "excel文件";
                    _Icon = "xls";
                    break;
                case ".pptx":
                    _FileType = "ppt文件";
                    _Icon = "ppt";
                    break;
                case ".ppt":
                    _FileType = "ppt文件";
                    _Icon = "ppt";
                    break;
                case ".txt":
                    _FileType = "记事本文件";
                    _Icon = "txt";
                    break;
                case ".pdf":
                    _FileType = "pdf文件";
                    _Icon = "pdf";
                    break;
                case ".zip":
                    _FileType = "压缩文件";
                    _Icon = "zip";
                    break;
                case ".rar":
                    _FileType = "压缩文件";
                    _Icon = "rar";
                    break;
                case ".png":
                    _FileType = "png图片";
                    _Icon = "png";
                    break;
                case ".gif":
                    _FileType = "gif图片";
                    _Icon = "gif";
                    break;
                case ".jpg":
                    _FileType = "jpg图片";
                    _Icon = "jpeg";
                    break;
                case ".mp3":
                    _FileType = "mp3文件";
                    _Icon = "mp3";
                    break;
                case ".html":
                    _FileType = "html文件";
                    _Icon = "html";
                    break;
                case ".css":
                    _FileType = "css文件";
                    _Icon = "css";
                    break;
                case ".mpeg":
                    _FileType = "mpeg文件";
                    _Icon = "mpeg";
                    break;
                case ".pds":
                    _FileType = "pds文件";
                    _Icon = "pds";
                    break;
                case ".ttf":
                    _FileType = "ttf文件";
                    _Icon = "ttf";
                    break;
                case ".swf":
                    _FileType = "swf文件";
                    _Icon = "swf";
                    break;
                default:
                    _FileType = "其他文件";
                    _Icon = "new";
                    //return "else.png";
                    break;
            }
            FileType = _FileType;
            Icon = _Icon;
        }



        public void Download(string KeyValue)
        {
            PM_PublicFile entity = FileBll.Repository().FindEntity(KeyValue);
            string filename = Server.UrlDecode(entity.FileName);//返回客户端文件名称
            string filepath = Server.UrlDecode(entity.FilePath);//文件虚拟路径
            if (FileDownHelper.FileExists(filepath))
            {
                FileDownHelper.DownLoadold(filepath, filename);
            }
        }

        public ActionResult DeleteFile(string NetworkFileId)
        {
            try
            {
                PM_PublicFile entity = FileBll.Repository().FindEntity(NetworkFileId);
                FileBll.Repository().Delete(NetworkFileId);
                string FilePath = this.Server.MapPath(entity.FilePath);
                if (System.IO.File.Exists(FilePath))
                    System.IO.File.Delete(FilePath);
                return Content(new JsonMessage { Success = true, Code = "1", Message = "删除成功。" }.ToString());
            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }


        public string CheckFileName(string FileID)
        {
            string sql = "select * from PM_PublicFile where FileID='" + FileID + "'";
            DataTable dt = FileBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["FileExtensions"].ToString() == ".pdf")
                {
                    if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                    {
                        return dt.Rows[0]["FileID"].ToString() + ".pdf";
                    }
                    else
                    {
                        DirFileHelper.CopyFile(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                        return dt.Rows[0]["FileID"].ToString() + ".pdf";
                    }
                }
                else
                {
                    //将word转换为pdf再返回路径
                    if (dt.Rows[0]["FileExtensions"].ToString() == ".doc" || dt.Rows[0]["FileExtensions"].ToString() == ".docx")
                    {
                        if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                        {
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                        else
                        {
                            ToPDFHelper.ConvertWord2Pdf(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                    }
                    else if (dt.Rows[0]["FileExtensions"].ToString() == ".xls" || dt.Rows[0]["FileExtensions"].ToString() == ".xlsx")
                    {
                        if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                        {
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                        else
                        {
                            ToPDFHelper.ConvertExcel2Pdf(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                    }
                    else if (dt.Rows[0]["FileExtensions"].ToString() == ".ppt" || dt.Rows[0]["FileExtensions"].ToString() == ".pptx")
                    {
                        if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                        {
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                        else
                        {
                            ToPDFHelper.ConvertPowerPoint2Pdf(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
            else
            {
                return "0";
            }
        }
    }
}
