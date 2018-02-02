﻿using LeaRun.Business;
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

namespace LeaRun.WebApp.Areas.VPModule.Controllers
{
    public class RiskDownFollowController : Controller
    {
        RepositoryFactory<VP_RiskDownFollow> repositoryfactory = new RepositoryFactory<VP_RiskDownFollow>();
        VP_RiskDownFollowBll RiskDownFollowBll = new VP_RiskDownFollowBll();
        VP_RiskDownFollowFileBll RiskDownFollowFileBll = new VP_RiskDownFollowFileBll();
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
                DataTable ListData = RiskDownFollowBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, VP_RiskDownFollow entity, string BuildFormJson, HttpPostedFileBase Filedata)
        {
            //定义三个基础数据，fema的矩阵，用二维数组的形式来表示，
            //因为行的序号原因，数据和实际的矩阵行需要上下颠倒
            //第一个，横坐标S，纵坐标D
            int[,] detectionZone = new int[,] {
                { 3,3,3,3,3,3,3,3,3,3},
                { 3,3,3,3,3,3,3,3,3,3},
                { 3,3,3,3,3,3,3,3,3,3},
                { 3,3,3,3,3,3,3,3,2,2},
                { 3,3,3,3,3,3,3,3,2,2},
                { 3,3,3,3,3,3,2,2,1,1},
                { 3,3,3,3,2,2,2,2,1,1},
                { 3,2,2,2,2,2,1,1,1,1},
                { 3,2,1,1,1,1,1,1,1,1},
                { 3,2,1,1,1,1,1,1,1,1}
            };
            //第二个，横坐标S，纵坐标O
            int[,] SeverityZone = new int[,]
            {
                { 3,3,3,3,3,3,3,3,3,3},
                { 3,3,3,3,3,3,2,2,1,1},
                { 3,3,3,3,3,3,2,2,1,1},
                { 3,3,3,3,2,2,1,1,1,1},
                { 3,3,2,2,2,2,1,1,1,1},
                { 3,2,2,2,1,1,1,1,1,1},
                { 3,2,2,2,1,1,1,1,1,1},
                { 3,2,1,1,1,1,1,1,1,1},
                { 3,1,1,1,1,1,1,1,1,1},
                { 3,1,1,1,1,1,1,1,1,1}
            };
            //第三个，横坐标SeverityZone，纵坐标detectionZone
            int[,] PriorityLevel = new int[,]
            {
                { 1,1,2},
                { 1,2,3},
                { 2,2,3}
            };


            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    if (KeyValue == ManageProvider.Provider.Current().UserId)
                    {
                        throw new Exception("无权限编辑信息");
                    }


                    entity.Modify(KeyValue);
                    if(entity.BeforeD!=0&&entity.BeforeS!=0&&entity.BeforeO!=0)
                    {
                        entity.BeforeRPN = entity.BeforeD * entity.BeforeS * entity.BeforeO;
                        entity.BeforePriorityLevel = PriorityLevel[detectionZone[entity.BeforeD - 1, entity.BeforeS - 1]-1, SeverityZone[entity.BeforeO - 1, entity.BeforeS - 1]-1];
                    }

                    if (entity.AfterD != 0 && entity.AfterS != 0 && entity.AfterO != 0)
                    {
                        entity.AfterRPN = entity.AfterD * entity.AfterS * entity.AfterO;
                        entity.AfterPriorityLevel = PriorityLevel[detectionZone[entity.AfterD - 1, entity.AfterS - 1]-1, SeverityZone[entity.AfterO - 1, entity.AfterS - 1]-1];
                    }
                    if(entity.RealFinishDt!=null)
                    {
                        entity.FinishState = "已完成";
                    }


                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();
                    if (entity.BeforeD != 0 && entity.BeforeS != 0 && entity.BeforeO != 0)
                    {
                        entity.BeforeRPN = entity.BeforeD * entity.BeforeS * entity.BeforeO;
                        entity.BeforePriorityLevel = PriorityLevel[detectionZone[entity.BeforeD - 1, entity.BeforeS - 1] - 1, SeverityZone[entity.BeforeO - 1, entity.BeforeS - 1] - 1];
                    }

                    if (entity.AfterD != 0 && entity.AfterS != 0 && entity.AfterO != 0)
                    {
                        entity.AfterRPN = entity.AfterD * entity.AfterS * entity.AfterO;
                        entity.AfterPriorityLevel = PriorityLevel[detectionZone[entity.AfterD - 1, entity.AfterS - 1] - 1, SeverityZone[entity.AfterO - 1, entity.AfterS - 1] - 1];
                    }
                    if (entity.RealFinishDt != null)
                    {
                        entity.FinishState = "已完成";
                    }


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.FollowID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.FollowID, ModuleId, isOpenTrans);
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
            VP_RiskDownFollow entity = DataFactory.Database().FindEntity<VP_RiskDownFollow>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }


        [HttpPost]
        public ActionResult Delete(string KeyValue)
        {
            try
            {
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
            Base_SysLogBll.Instance.WriteLog<VP_RiskDownFollow>(array, IsOk.ToString(), Message);
        }

        public ActionResult Uploadify()
        {
            return View();
        }

        public ActionResult GetExistsList(string FollowID)
        {
            string sql = @" select a.*,case when (b.ResponseBy='{1}' or (exists (select * from Base_ObjectUserRelation where ObjectId='05883a74-6515-4bab-8ec6-3022aee9a1d8' and UserId='{1}')) and b.FinishState='进行中' ) then 1 else 0 end as canDel 
from VP_RiskDownFollowFile a 
left join VP_RiskDownFollow b on a.followid=b.followid

where 1=1 and a.FollowID='{0}' ";
            sql = string.Format(sql, FollowID,ManageProvider.Provider.Current().UserId);
            DataTable dt = RiskDownFollowBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata)
        {
            try
            {
                Thread.Sleep(1000);////延迟500毫秒
                VP_RiskDownFollowFile entity = new VP_RiskDownFollowFile();
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

                string virtualPath = string.Format("~/Resource/Document/NetWorkDisk/RiskDownFile/{0}{1}", fileGuid, FileEextension);
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
                        entity.FollowID = FolderId;
                        entity.FileName = Filedata.FileName;
                        //entity.FilePath = virtualPath;
                        entity.FileSize = filesize.ToString();
                        entity.FileExtensions = FileEextension;
                        string _FileType = "";
                        string _Icon = "";
                        this.DocumentType(FileEextension, ref _FileType, ref _Icon);
                        entity.Icon = _Icon;
                        entity.FileType = _FileType;
                        IsOk = DataFactory.Database().Insert<VP_RiskDownFollowFile>(entity).ToString();
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
            VP_RiskDownFollowFile entity = RiskDownFollowFileBll.Repository().FindEntity(KeyValue);
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
                VP_RiskDownFollowFile entity = RiskDownFollowFileBll.Repository().FindEntity(NetworkFileId);
                RiskDownFollowFileBll.Repository().Delete(NetworkFileId);
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

        public ActionResult YearListJson()
        {
            string sql = " select distinct year(CreateDt) as year from VP_RiskDownFollow where 1=1  ";
            DataTable dt = RiskDownFollowBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }




    }
}