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
    public class ProblemActionController : Controller
    {
        RepositoryFactory<FY_ProblemAction> repositoryfactory = new RepositoryFactory<FY_ProblemAction>();
        FY_ProblemActionBll PostBll = new FY_ProblemActionBll();
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
                DataTable ListData = PostBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult FormNew()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, FY_ProblemAction entity, string BuildFormJson, HttpPostedFileBase Filedata)
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
                    string GetReciverSql = " select Email from base_user where userid='" + entity.ResponseBy + "'  ";
                    DataTable dt = PostBll.GetDataTable(GetReciverSql);
                    if (dt.Rows.Count > 0)
                    {
                        //把发送邮件功能写到unitity的静态类中，以后直接调用
                        MailHelper.SendEmail(dt.Rows[0][0].ToString(), "您好，您的分层审核有一项不合格，请注意登录系统查看");
                    }
                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.ActionID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ActionID, ModuleId, isOpenTrans);
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
            FY_ProblemAction entity = DataFactory.Database().FindEntity<FY_ProblemAction>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }


        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata)
        {
            try
            {

                Thread.Sleep(1000);////延迟500毫秒
                Base_NetworkFile entity = new Base_NetworkFile();
                FY_ProblemAction PAentity = DataFactory.Database().FindEntity<FY_ProblemAction>(FolderId);

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

                string virtualPath = string.Format("~/Resource/Document/NetworkDisk/{0}/{1}/{2}{3}", "ProblemAction", uploadDate, fileGuid, FileEextension);
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
                       
                        PAentity.AttachPath = virtualPath;


                        DataFactory.Database().Update<FY_ProblemAction>(PAentity);
                    }
                    catch (Exception ex)
                    {
                        //IsOk = ex.Message;
                        //System.IO.File.Delete(virtualPath);
                    }
                }
                var JsonData = new
                {
                    Status = IsOk,
                    NetworkFile = PAentity,
                };
                return Content(JsonData.ToJson());


            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }


        public ActionResult Approve(string ActionID, string Tag,string ActionContent,string CauseAnaly)
        {
            
            StringBuilder strSql = new StringBuilder();
            FY_ProblemAction entity = DataFactory.Database().FindEntity<FY_ProblemAction>(ActionID);
            if(ManageProvider.Provider.Current().UserId==entity.CreateBy)
            {
                if (Tag == "Yes")
                {
                    strSql.AppendFormat(@"update FY_ProblemAction set ProblemState='已完成',RealCompleteDate=getdate(),ActionContent='{1}',CauseAnaly='{2}' where
                   ActionID='{0}'", ActionID,ActionContent,CauseAnaly);
                }
                else
                {
                    strSql.AppendFormat(@"update FY_ProblemAction set ProblemState='退回' where
                   ActionID='{0}'", ActionID);
                }
            }
            if(ManageProvider.Provider.Current().UserId==entity.ResponseBy&&entity.ProblemState!= "待审")
            {
                strSql.AppendFormat(@"update FY_ProblemAction set ProblemState='待审',ActionContent='{1}',CauseAnaly='{2}' where
                   ActionID='{0}'", ActionID, ActionContent, CauseAnaly);

                string GetReciverSql = " select Email from base_user where userid='" + entity.CreateBy + "'  ";
                DataTable dt = PostBll.GetDataTable(GetReciverSql);
                if (dt.Rows.Count > 0)
                {
                    //把发送邮件功能写到unitity的静态类中，以后直接调用
                    MailHelper.SendEmail(dt.Rows[0][0].ToString(), "您好，您有一个分层审核的问题项需要审核，请注意登录系统查看");
                }
            }
            PostBll.ExecuteSql(strSql);


            return Content(new JsonMessage { Success = true, Code = 1.ToString(),  Message = "操作成功。" }.ToString());
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
            Base_SysLogBll.Instance.WriteLog<FY_ProblemAction>(array, IsOk.ToString(), Message);
        }

        //按需求导出excel
        public void ExcelExport(string condition)
        {
            ExcelHelper ex = new ExcelHelper();
            //NpoiHelper ex2 = new NpoiHelper();
            string sql = @" select a.ProblemState as 状态,a.ProblemDescripe as 问题描述,a.CauseAnaly as 原因分析,a.ActionContent as 对策措施,
b.RealName as 责任人,c.RealName as 创建人 ,CreateDt as 创建日期,
(select top 1 FullName from Base_Department where DepartmentId=b.DepartmentId) as 责任人部门,
(select top 1 FullName from Base_Department where DepartmentId=c.DepartmentId) as 创建人部门,
a.Plandate as 计划完成日期,a.RealCompletedate  as 实际完成日期
from FY_ProblemAction a 
left join Base_User b on a.ResponseBy=b.UserId 
left join Base_User c on a.CreateBy=c.UserId where 1=1 ";
            sql = sql + condition;
            DataTable ListData = PostBll.GetDataTable(sql);
            //ex.EcportExcel(ListData, "快速反应导出");

            MemoryStream ms = NpoiHelper.RenderDataTableToExcel(ListData) as MemoryStream;

            /*情况1：在Asp.NET中，输出文件流，浏览器自动提示下载*/
            Response.AddHeader("Content-Disposition", string.Format("attachment; filename=download.xls"));
            Response.BinaryWrite(ms.ToArray());
            ms.Close();
            ms.Dispose();
        }




    }
}
