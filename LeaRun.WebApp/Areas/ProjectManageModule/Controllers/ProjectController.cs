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
        Base_FlowBll FlowBll = new Base_FlowBll();
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
        public ActionResult SubmitForm(string KeyValue, PM_Project entity, string BuildFormJson, HttpPostedFileBase Filedata, string DetailForm,string ActivityForm)
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

                    //新建单据的时候，注册流程】
                    entity.FlowID = FlowBll.RegistFlow("Sj_ProjectManage", entity.ProjectID, entity.DataProvider);

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

        //SubmitActivityForm
        [HttpPost]
        public ActionResult SubmitActivityForm(string KeyValue,string BuildFormJson, HttpPostedFileBase Filedata, string DetailForm, string ActivityForm,string ProblemForm)
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

                    database.Delete<PM_ProjectActivity>("ProjectID", KeyValue, isOpenTrans);
                    List<PM_ProjectActivity> ActivityList = ActivityForm.JonsToList<PM_ProjectActivity>();
                    int index = 1;
                    foreach (PM_ProjectActivity entityD in ActivityList)
                    {
                        if (!string.IsNullOrEmpty(entityD.ActivityContent))
                        {
                            entityD.Create();
                            entityD.ActivityDate = DateTime.Now;
                            entityD.ProjectID = KeyValue;
                            
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }

                    database.Delete<PM_ProjectProblem>("ProjectID", KeyValue, isOpenTrans);
                    List<PM_ProjectProblem> ProblemList = ProblemForm.JonsToList<PM_ProjectProblem>();
                    index = 1;
                    foreach (PM_ProjectProblem entityD in ProblemList)
                    {
                        if (!string.IsNullOrEmpty(entityD.ProblemDescripe))
                        {
                            entityD.Create();
                            entityD.PutDate = DateTime.Now;
                            entityD.ProjectID = KeyValue;
                            entityD.SortNO = index;

                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }



                }
                
                //Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ProjectID, ModuleId, isOpenTrans);
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

        public ActionResult GetActivityList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectBll.GetActivityList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult GetProblemList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectBll.GetProblemList(KeyValue),
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

        public ActionResult ProfitList()
        {
            return View();
        }

        public ActionResult GridProfitListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectBll.GetProfitList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult ProfitListForm()
        {
            return View();
        }

        public ActionResult GetProfitDetailList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectBll.GetProfitDetailList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult SubmitProfitForm(string KeyValue, PM_Project entity, string BuildFormJson, HttpPostedFileBase Filedata, string ProfitForm)
        {

            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();


            string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";

            database.Delete<PM_ProjectProfit>("ProjectID", KeyValue, isOpenTrans);
            List<PM_ProjectProfit> DetailList = ProfitForm.JonsToList<PM_ProjectProfit>();
            int index = 1;
            foreach (PM_ProjectProfit entityD in DetailList)
            {
                if (!string.IsNullOrEmpty(entityD.ProfitDate.ToString()))
                {
                    entityD.Create();
                    entityD.ProjectID = KeyValue;
                    database.Insert(entityD, isOpenTrans);
                    index++;
                }
            }
            database.Update(entity, isOpenTrans);
            database.Commit();
            return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
        }

        public ActionResult FlowForm(string ProjectID)
        {
            PM_Project entity = DataFactory.Database().FindEntity<PM_Project>(ProjectID);
            FlowDisplay flow = FlowBll.FlowDisplay(entity.FlowID);
            ViewData["flow"] = flow;
            ViewData["UserID"] = ManageProvider.Provider.Current().UserId;
            return View();
        }

        public int submit(string KeyValue,string FlowID,string type)
        {

            int a = 0;
            StringBuilder strSql = new StringBuilder();
            //
            string sql = " select CurrentPost from Base_FlowLog where FlowID='" + FlowID + "'  ";
            DataTable dt = ProjectBll.GetDataTable(sql);
            

            //FY_ObjectTracking entity = DataFactory.Database().FindEntity<FY_ObjectTracking>(KeyValue);
            if (type == "-1" || type == "-2")
            {
                a = FlowBll.RejectFlow(FlowID);
            }
            else
            {
                a = FlowBll.SubmitFlow(FlowID);
            }
            strSql.AppendFormat(@" update PM_Project set Approvestatus='{0}' where ProjectID='{1}'  ",a,KeyValue);


            ProjectBll.ExecuteSql(strSql);
            return a;
        }

        public ActionResult PrintForm(string KeyValue)
        {
            string sql = @"  select ProjectNO,ProjectName,a.CreateDate,PlanFinishDate,
 a.ProjectNature,a.ProjectStatus,a.ProjectIndicators,a.BenchMark,a.Target,a.CalculationFormula ,c.RealName,b.FullName
 from PM_Project a
 left join Base_Department b on a.DepartMentID=b.DepartmentId
 left join Base_User c on a.DataProvider=c.UserId
 where a.ProjectID='" + KeyValue+"'";
            DataTable dt = ProjectBll.GetDataTable(sql);

            string sqlMember = @"  select UserName,PostName,Duty from PM_ProjectMember where ProjectID='"+KeyValue+"' ";
            DataTable dtMember = ProjectBll.GetDataTable(sqlMember);

            ViewData["dt"] = dt;
            ViewData["dtMember"] = dtMember;
            return View();
        }

        public ActionResult ActivityForm()
        {
            return View();
        }


    }
}
