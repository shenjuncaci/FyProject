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
    public class ProjectController : Controller
    {
        // GET: /ProjectManageModule/Notice/

        RepositoryFactory<PM_Project> repositoryfactory = new RepositoryFactory<PM_Project>();
        PM_ProjectBll ProjectBll = new PM_ProjectBll();
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
                DataTable ListData = ProjectBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
            Base_NoteNOBll notenobll = new Base_NoteNOBll();
            string KeyValue = Request["KeyValue"];
            if (string.IsNullOrEmpty(KeyValue))
            {
                ViewBag.BillNo = notenobll.Code("ProjectNO");
                ViewBag.CreateUserName = ManageProvider.Provider.Current().UserName;
            }
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, PM_Project entity, string BuildFormJson, HttpPostedFileBase Filedata, string DetailForm)
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

                    database.Delete<PM_ProjectMember>("ProjectID", KeyValue, isOpenTrans);
                    List<PM_ProjectMember> DetailList = DetailForm.JonsToList<PM_ProjectMember>();
                    int index = 1;
                    foreach (PM_ProjectMember entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.UserID))
                        {
                            entityD.Create();
                            entityD.ProjectID = KeyValue;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }

                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();

                    List<PM_ProjectMember> DetailList = DetailForm.JonsToList<PM_ProjectMember>();
                    int index = 1;
                    foreach (PM_ProjectMember entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.UserID))
                        {
                            entityD.Create();
                            entityD.ProjectID = entity.ProjectID;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }

                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.ProjectID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ProjectID, ModuleId, isOpenTrans);
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
            PM_Project entity = DataFactory.Database().FindEntity<PM_Project>(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<PM_Project>(array, IsOk.ToString(), Message);
        }

        public ActionResult DepartmentJson()
        {
            string sql = " select departmentid,fullname from base_department where 1=1 order by FullName ";
            DataTable dt = ProjectBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult UserJson()
        {
            string sql = @"select userid,realname from Base_User where Enabled=1 and 
(DepartmentId = '5bce3524-2835-46c9-af3a-90250f2cf198' or DepartmentId in (select DepartmentId from Base_Department where ParentId = '5bce3524-2835-46c9-af3a-90250f2cf198') )";
            DataTable dt = ProjectBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult GetDetailList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectBll.GetDetailList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult UserListBatch()
        {
            return View();
        }

        public ActionResult GetUserListJson(string keywords, string CompanyId, string DepartmentId,
           JqGridParam jqgridparam, string ParameterJson, string SkillName, string SkillType)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectBll.GetUserList(keywords, ref jqgridparam, ParameterJson);
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
    }
}
