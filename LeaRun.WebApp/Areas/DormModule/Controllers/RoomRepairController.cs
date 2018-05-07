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

namespace LeaRun.WebApp.Areas.DormModule.Controllers
{
    public class RoomRepairController : Controller
    {
        RepositoryFactory<DM_RoomRepair> repositoryfactory = new RepositoryFactory<DM_RoomRepair>();
        DM_RoomRepairBll RoomRepairBll = new DM_RoomRepairBll();
        //

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = RoomRepairBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
        public ActionResult SubmitForm(string KeyValue, DM_RoomRepair entity, string BuildFormJson, HttpPostedFileBase Filedata)
        {
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


                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.RoomRepairID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.RoomRepairID, ModuleId, isOpenTrans);
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
            DM_RoomRepair entity = DataFactory.Database().FindEntity<DM_RoomRepair>(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<DM_RoomRepair>(array, IsOk.ToString(), Message);
        }

        public string FinishThis(string ID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"update dm_roomrepair set RepairState='已完成',finishdt=getdate()  where roomrepairID='{0}' ",ID);
            RoomRepairBll.ExecuteSql(strSql);
            return "1";
        }

        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata)
        {
            try
            {

                Thread.Sleep(1000);////延迟500毫秒
                DM_RoomRepairPicture entity = new DM_RoomRepairPicture();
                

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

                string virtualPath = string.Format("~/Resource/Document/NetworkDisk/{0}/{1}{2}", "RoomRepair", fileGuid, FileEextension);
                //rapidentity.res_msfj = virtualPath;

                string fullFileName = this.Server.MapPath(virtualPath);
                //创建文件夹，保存文件
                string path = Path.GetDirectoryName(fullFileName);
                Directory.CreateDirectory(path);
                if (!System.IO.File.Exists(fullFileName))
                {
                    Filedata.SaveAs(fullFileName);
                    try
                    {
                        entity.Create();
                        entity.RoomRepairID = FolderId;
                        entity.PictureUrl = virtualPath;
                        DataFactory.Database().Insert<DM_RoomRepairPicture>(entity);
                    }
                    catch (Exception ex)
                    {
                        IsOk = ex.Message;
                        System.IO.File.Delete(virtualPath);
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



        public ActionResult GetExistsPicture(string RoomRepairID)
        {
            string sql = " select * from DM_RoomRepairPicture where RoomRepairID='{0}'  ";
            sql = string.Format(sql, RoomRepairID);
            DataTable dt = RoomRepairBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult DeleteFile(string KeyValue)
        {
            StringBuilder strsql = new StringBuilder();
            strsql.AppendFormat(@" delete from DM_RoomRepairPicture  where RoomRepairPictureID='{0}' ",KeyValue);
            RoomRepairBll.ExecuteSql(strsql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "删除成功。" }.ToString());

        }

        public ActionResult RepairDescripeList()
        {
            string sql = @"select a.Code,a.FullName from Base_DataDictionaryDetail a 
left join Base_DataDictionary b on a.DataDictionaryId=b.DataDictionaryId
where b.FullName='故障类型' order by a.SortCode ";
            DataTable dt = RoomRepairBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }








    }
}
