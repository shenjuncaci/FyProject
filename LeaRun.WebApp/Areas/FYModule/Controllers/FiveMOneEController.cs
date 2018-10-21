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
            strsql.AppendFormat(@" update fy_5m1e set IsConfirm=1 where ID='{0}',EndBy='{1}' ",ID,ManageProvider.Provider.Current().UserId);
            Bll.ExecuteSql(strsql);
            return "1";
        }

    }
}
