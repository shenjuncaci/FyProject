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
    public class PostController : Controller
    {
        RepositoryFactory<FY_Post> repositoryfactory = new RepositoryFactory<FY_Post>();
        FY_PostBll PostBll = new FY_PostBll();
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
            ViewData["UserName"] = ManageProvider.Provider.Current().UserName;
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, FY_Post entity, string BuildFormJson, HttpPostedFileBase Filedata)
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
                    entity.DepartMentID = ManageProvider.Provider.Current().DepartmentId;
                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.PostID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.PostID, ModuleId, isOpenTrans);
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
            FY_Post entity = DataFactory.Database().FindEntity<FY_Post>(KeyValue);
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

        [HttpPost]
        public ActionResult DeleteRole(string KeyValue)
        {
            try
            {
                var Message = "删除失败。";
                int IsOk = 0;
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" delete from Base_ObjectUserRelation where ObjectId='54804f22-89a1-4eee-b257-255deaf4face' and UserId='{0}' ",KeyValue);
                IsOk = PostBll.ExecuteSql(strSql);
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
            Base_SysLogBll.Instance.WriteLog<FY_Post>(array, IsOk.ToString(), Message);
        }


        //20170901新增厂长维护模块
        public ActionResult CzList()
        {
            return View();
        }

        public ActionResult GridCzPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PostBll.CZGetPageList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult ProduceDepartList()
        {
            return View();
        }

        public ActionResult GetProduceDepartList()
        {
            StringBuilder sb = new StringBuilder();
            string sql = "select * from Base_Department where Nature='生产' and Enabled=1 ";
            DataTable dt = PostBll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                //if (!string.IsNullOrEmpty(dr["objectid"].ToString()))//判断是否选中
                //{
                //    strchecked = "selected";
                //}
                sb.Append("<li title=\"" + dr["FullName"] + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["DepartmentId"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["FullName"] + "</a><i></i>");
                sb.Append("</li>");
            }
            return Content(sb.ToString());
        }

        public ActionResult UserList()
        {
            return View();
        }
        public ActionResult GetUserList()
        {
            StringBuilder sb = new StringBuilder();
            string sql = "select * from Base_user where Enabled=1 ";
            DataTable dt = PostBll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                //if (!string.IsNullOrEmpty(dr["objectid"].ToString()))//判断是否选中
                //{
                //    strchecked = "selected";
                //}
                sb.Append("<li title=\"" + dr["realname"] + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["userid"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["realname"] + "</a><i></i>");
                sb.Append("</li>");
            }
            return Content(sb.ToString());
        }

        public ActionResult UserListSubmit(string UserID, string ObjectId)
        {
            try
            {
                IDatabase database = DataFactory.Database();
                StringBuilder strSql = new StringBuilder();
                
                string[] array = ObjectId.Split(',');
                for (int i = 0; i < array.Length - 1; i++)
                {
                    //避免有重复添加的，先把执行下删除的语句
                    strSql.AppendFormat(@" delete from Base_ObjectUserRelation where userid='{0}' 
and ObjectId='54804f22-89a1-4eee-b257-255deaf4face'  ", array[i]);
                    strSql.AppendFormat(@" insert into Base_ObjectUserRelation values(newid(),2,
'54804f22-89a1-4eee-b257-255deaf4face','{1}','1',getdate(),'{0}','{2}') ",ManageProvider.Provider.Current().UserId
,array[i],ManageProvider.Provider.Current().UserName);
                }

                //if (array.Length > 2)
                //{
                //    return Content(new JsonMessage { Success = true, Code = "-1", Message = "操作失败,一次只能选择一个用户。" }.ToString());
                //}
                //else
                //{

                //    database.ExecuteBySql(strSql);
                //    return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
                //}
                database.ExecuteBySql(strSql);
                return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }
        }

        public ActionResult DepartListSubmit(string UserID, string ObjectId)
        {
            try
            {
                IDatabase database = DataFactory.Database();
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" delete from fy_departRelation where userid='{0}'  ",UserID);
                string[] array = ObjectId.Split(',');
                for(int i=0;i<array.Length-1;i++)
                {
                    strSql.AppendFormat(@" insert into fy_departRelation values(newid(),'{0}','{1}') ",UserID,array[i]);
                }

                //if (array.Length > 2)
                //{
                //    return Content(new JsonMessage { Success = true, Code = "-1", Message = "操作失败,一次只能选择一个用户。" }.ToString());
                //}
                //else
                //{

                //    database.ExecuteBySql(strSql);
                //    return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
                //}
                database.ExecuteBySql(strSql);
                return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }
        }

    }
}
