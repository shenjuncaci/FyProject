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

namespace LeaRun.WebApp.Areas.FYModule.Controllers
{
    public class ProblemTrackController : Controller
    {
        RepositoryFactory<FY_ProblemTrack> repositoryfactory = new RepositoryFactory<FY_ProblemTrack>();
        RepositoryFactory<FY_ProblemTrackDetail> detailrepository = new RepositoryFactory<FY_ProblemTrackDetail>();
        FY_ProblemTrackBll TrackBll = new FY_ProblemTrackBll();
        FY_ProblemTrackFileBll TrackFileBll = new FY_ProblemTrackFileBll();
        //
        // GET: /FYModule/Process/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = TrackBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
                var JsonData = new
                {
                    total = jqgridparam.total,
                    page = jqgridparam.page,
                    records = jqgridparam.records,
                    costtime = CommonHelper.TimerEnd(watch),
                    rows = ListData,
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }



        public ActionResult Form()
        {
            ViewData["UserID"] = ManageProvider.Provider.Current().UserId;
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, FY_ProblemTrack entity, string BuildFormJson,
            HttpPostedFileBase Filedata, string DetailForm)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    //if (KeyValue == ManageProvider.Provider.Current().UserId)
                    //{
                    //    throw new Exception("无权限编辑信息");
                    //}



                    entity.Modify(KeyValue);


                    database.Update(entity, isOpenTrans);

                    int index = 1;
                    List<FY_ProblemTrackDetail> DetailList = DetailForm.JonsToList<FY_ProblemTrackDetail>();
                    foreach (FY_ProblemTrackDetail entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.Progress))
                        {
                            if (!string.IsNullOrEmpty(entityD.ProblemDID))
                            {
                                entityD.Modify(entityD.ProblemDID);
                                database.Update(entityD, isOpenTrans);
                                index++;
                            }
                            else
                            {
                                entityD.Create();
                                entityD.ProblemID = entity.ProblemID;

                                database.Insert(entityD, isOpenTrans);
                                index++;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(entityD.ProblemDID))
                            {
                                detailrepository.Repository().Delete(entityD.ProblemDID);

                            }
                        }
                    }

                }
                else
                {
                    //entity.DepartMentID = ManageProvider.Provider.Current().DepartmentId;
                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    int index = 1;
                    List<FY_ProblemTrackDetail> DetailList = DetailForm.JonsToList<FY_ProblemTrackDetail>();
                    foreach (FY_ProblemTrackDetail entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.Progress))
                        {
                            if (!string.IsNullOrEmpty(entityD.ProblemDID))
                            {
                                entityD.Modify(entityD.ProblemDID);
                                database.Update(entityD, isOpenTrans);
                                index++;
                            }
                            else
                            {
                                entityD.Create();
                                entityD.ProblemID = entity.ProblemID;

                                database.Insert(entityD, isOpenTrans);
                                index++;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(entityD.ProblemDID))
                            {
                                detailrepository.Repository().Delete(entityD.ProblemDID);

                            }
                        }
                    }

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.ProblemID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ProblemID, ModuleId, isOpenTrans);
                database.Commit();
                return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
            }
            catch (Exception ex)
            {
                database.Rollback();
                database.Close();
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetForm(string KeyValue)
        {
            FY_ProblemTrack entity = DataFactory.Database().FindEntity<FY_ProblemTrack>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        public ActionResult CusName()
        {
            string sql = " select fy_cus_id,fy_cus_name from FY_CUS where 1=1 ";
            DataTable dt = TrackBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        [HttpPost]
        public ActionResult Delete(string KeyValue)
        {
            try
            {
                //添加一步验证，如果竞争对手ID在竞争对手明细表中已存在，则不能删除
               

                var Message = "删除失败。";
                int IsOk = 0;

                IsOk = repositoryfactory.Repository().Delete(KeyValue);
                if (IsOk > 0)
                {
                    Message = "删除成功。";
                }

                WriteLog(IsOk, KeyValue, Message);
                return Content(new JsonMessage { Success = true, Code = IsOk.ToString(), Message = Message }.ToString());
            }
            catch (Exception ex)
            {
                WriteLog(-1, KeyValue, "操作失败：" + ex.Message);
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        public void WriteLog(int IsOk, string KeyValue, string Message = "")
        {
            string[] array = KeyValue.Split(',');
            Base_SysLogBll.Instance.WriteLog<FY_ProblemTrack>(array, IsOk.ToString(), Message);
        }

        public ActionResult GetDetailList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = TrackBll.GetDetailList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public string FinishIt(string ProblemID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" update FY_ProblemTrack set Status='已完成',FinshDt=GETDATE() where  ProblemID='{0}' and Status!='已完成' ", ProblemID);
            TrackBll.ExecuteSql(strSql);
            return "0";
        }

        public ActionResult Uploadify()
        {
            return View();
        }

        //多文件上传，每次insert
        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata)
        {
            try
            {
                Thread.Sleep(1000);////延迟500毫秒
                FY_ProblemTrackFile entity = new FY_ProblemTrackFile();
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

                string virtualPath = string.Format("~/Resource/Document/NetWorkDisk/ProblemTrack/{0}{1}", fileGuid, FileEextension);
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
                        entity.FilePath = virtualPath ;
                        entity.ProblemID = FolderId;
                        entity.FileName = Filedata.FileName;
                        //entity.FilePath = virtualPath;
                        entity.FileSize = filesize.ToString();
                        entity.FileExtensions = FileEextension;
                        string _FileType = "";
                        string _Icon = "";
                        this.DocumentType(FileEextension, ref _FileType, ref _Icon);
                        entity.Icon = _Icon;
                        entity.FileType = _FileType;
                        IsOk = DataFactory.Database().Insert<FY_ProblemTrackFile>(entity).ToString();
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


        public ActionResult GetFileList(string keywords, string CompanyId, string DepartmentId,
            JqGridParam jqgridparam, string ParameterJson, string SkillID)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = TrackBll.GetFileList(keywords, ref jqgridparam, ParameterJson, SkillID);
                var JsonData = new
                {
                    total = jqgridparam.total,
                    page = jqgridparam.page,
                    records = jqgridparam.records,
                    costtime = CommonHelper.TimerEnd(watch),
                    rows = ListData,
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public void Download(string KeyValue)
        {
            FY_ProblemTrackFile entity = TrackFileBll.Repository().FindEntity(KeyValue);
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
                FY_ProblemTrackFile entity = TrackFileBll.Repository().FindEntity(NetworkFileId);
                TrackFileBll.Repository().Delete(NetworkFileId);
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

        public ActionResult DelOrDownload()
        {
            return View();
        }
    }
}
