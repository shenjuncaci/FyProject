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
    public class FiveMOneEController : Controller
    {
        RepositoryFactory<FY_5M1E> repositoryfactory = new RepositoryFactory<FY_5M1E>();
        FY_5M1EBLL Bll = new FY_5M1EBLL();
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
                DataTable ListData = Bll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
        public ActionResult SubmitForm(string KeyValue, FY_5M1E entity, string BuildFormJson, HttpPostedFileBase Filedata)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    entity.Modify(KeyValue);
                    database.Update(entity, isOpenTrans);

                }
                else
                {
                    entity.Create();
                    database.Insert(entity, isOpenTrans);
                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.ID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ID, ModuleId, isOpenTrans);
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
            FY_5M1E entity = DataFactory.Database().FindEntity<FY_5M1E>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        public ActionResult BanGroupJson()
        {
            string sql = " select GroupUserId,FullName from Base_GroupUser where DepartmentId='{0}' ";
            sql = string.Format(sql, ManageProvider.Provider.Current().DepartmentId);
            DataTable dt = Bll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult PostJson()
        {
            string sql = "select PostName from FY_Post where DepartMentID='{0}'";
            sql = string.Format(sql, ManageProvider.Provider.Current().DepartmentId);
            DataTable dt = Bll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public string ConfirmIt(string ID)
        {
            StringBuilder strsql = new StringBuilder();
            strsql.AppendFormat(@" update fy_5m1e set IsCofirm=1,EndBy='{1}'  where ID='{0}' ", ID,ManageProvider.Provider.Current().UserId);
            Bll.ExecuteSql(strsql);
            return "1";
        }

        public string GCKConfirmIt(string ID)
        {
            StringBuilder strsql = new StringBuilder();
            strsql.AppendFormat(@" update fy_5m1e set GCKEndBy='{1}'  where ID='{0}' ", ID, ManageProvider.Provider.Current().UserName);
            Bll.ExecuteSql(strsql);
            return "1";
        }

        public void ExcelExport()
        {
            ExcelHelper ex = new ExcelHelper();
            string sql = @" select  a.processname as 工位,a.createdate as 创建日期,b.FullName as 班次,a.changepoint as 变化点,
a.changecontent as 变化内容,a.changereason as 变化原因,a.changelevel as 变化等级,a.changeaction as 变化措施,a.enddate as 有效期,d.RealName as 关闭人,
(select top 1 ProblemDescripe from FY_ProblemAction  where PlanDID in (select plandid from FY_Plan aa left join FY_PlanDetail bb on aa.PlanID=bb.PlanID where bb.ProcessID=a.id) and createbydept='{0}') as 审核发现问题
from FY_5M1E a 
left join Base_GroupUser b  on a.bangroup=b.GroupUserId
left join Base_User c on a.createby=c.UserId
left join Base_User d on a.endby=d.UserId

where 1=1 and a.departmentid='{0}' ";
            sql = string.Format(sql, ManageProvider.Provider.Current().DepartmentId);
            //sql = sql + condition;
            DataTable ListData = Bll.GetDataTable(sql);
            //ex.EcportExcel(ListData, "一般问题导出");

            MemoryStream ms = NpoiHelper.RenderDataTableToExcel(ListData) as MemoryStream;

            /*情况1：在Asp.NET中，输出文件流，浏览器自动提示下载*/
            Response.AddHeader("Content-Disposition", string.Format("attachment; filename=download.xls"));
            Response.BinaryWrite(ms.ToArray());
            ms.Close();
            ms.Dispose();

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

                
                return Content(new JsonMessage { Success = true, Code = IsOk.ToString(), Message = Message }.ToString());
            }
            catch (Exception ex)
            {
               
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

    }
}
