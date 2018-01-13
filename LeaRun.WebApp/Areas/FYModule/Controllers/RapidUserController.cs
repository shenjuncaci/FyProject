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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LeaRun.WebApp.Areas.FYModule.Controllers
{
    public class RapidUserController : Controller
    {

        Base_UserBll base_userbll = new Base_UserBll();
        Base_CompanyBll base_companybll = new Base_CompanyBll();
        Base_ObjectUserRelationBll base_objectuserrelationbll = new Base_ObjectUserRelationBll();
        FY_PlanBll planbll = new FY_PlanBll();
        //
        // GET: /FYModule/RapidUser/

        public ActionResult Index()
        {
            return View();
        }

        //车间主任列表
        public ActionResult Index2()
        {
            return View();
        }

        //班长列表
        public ActionResult Index3()
        {
            return View();
        }

        //客服人员列表
        public ActionResult Index4()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam,string type)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = base_userbll.GetPageList(keywords, CompanyId, DepartmentId, ref jqgridparam,type);
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

        public ActionResult FormNew()
        {
            return View();
        }

        public ActionResult FormBZ()
        {
            return View();
        }

        public ActionResult ListJson()
        {
            string sql = " select GroupUserId,FullName from Base_GroupUser where 1=1 and DepartmentId='"+ManageProvider.Provider.Current().DepartmentId+"'  ";
            DataTable dt = planbll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult UserList()
        {
            return View();
        }

        public ActionResult GetUserList()
        {
            StringBuilder sb = new StringBuilder();
            string sql = "select * from Base_User where Enabled=1";
            DataTable dt = planbll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                //if (!string.IsNullOrEmpty(dr["objectid"].ToString()))//判断是否选中
                //{
                //    strchecked = "selected";
                //}
                sb.Append("<li title=\"" + dr["RealName"] + "(" + dr["Code"] + ")" + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["UserId"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["RealName"] + "</a><i></i>");
                sb.Append("</li>");
            }
            return Content(sb.ToString());
        }


        public ActionResult UserListSubmit(string DepartmentID, string ObjectId)
        {
            try
            {
                IDatabase database = DataFactory.Database();
                StringBuilder strSql = new StringBuilder();
                string[] array = ObjectId.Split(',');

                if (array.Length > 2)
                {
                    return Content(new JsonMessage { Success = true, Code = "-1", Message = "操作失败,一次只能选择一个用户。" }.ToString());
                }
                else
                {
                    //修改选择的用户到这个部门
                    strSql.AppendFormat(@" update base_user set DepartmentId='{1}' where UserId='{0}'  ", array[0], ManageProvider.Provider.Current().DepartmentId);
                    
                    //给新选择的用户添加车间主任的权限
                    strSql.AppendFormat(@"delete from Base_ObjectUserRelation where  ObjectId='f6afd4e4-6fb2-446f-88dd-815ddb91b09d' and UserId='{0}' ", array[0]);
                    strSql.AppendFormat(@"insert into Base_ObjectUserRelation values(NEWID(),2,'f6afd4e4-6fb2-446f-88dd-815ddb91b09d','{0}',1,GETDATE(),'{1}','{2}') ", array[0], ManageProvider.Provider.Current().UserId, ManageProvider.Provider.Current().UserName);
                    database.ExecuteBySql(strSql);
                    return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
                }

            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }
        }

        public ActionResult CustomerAssUserListSubmit(string DepartmentID, string ObjectId)
        {
            try
            {
                IDatabase database = DataFactory.Database();
                StringBuilder strSql = new StringBuilder();
                string[] array = ObjectId.Split(',');

                if (array.Length > 2)
                {
                    return Content(new JsonMessage { Success = true, Code = "-1", Message = "操作失败,一次只能选择一个用户。" }.ToString());
                }
                else
                {
                    

                    //给新选择的用户添加车间主任的权限
                    strSql.AppendFormat(@"delete from Base_ObjectUserRelation where  ObjectId='9cc63534-b2cb-4cbb-99d9-50d43942ee47' and UserId='{0}' ", array[0]);
                    strSql.AppendFormat(@"insert into Base_ObjectUserRelation values(NEWID(),2,'9cc63534-b2cb-4cbb-99d9-50d43942ee47','{0}',1,GETDATE(),'{1}','{2}') ", array[0], ManageProvider.Provider.Current().UserId, ManageProvider.Provider.Current().UserName);
                    database.ExecuteBySql(strSql);
                    return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
                }

            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }
        }

        public ActionResult DeleteBZ(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"delete from Base_ObjectUserRelation where UserID='{0}' and ObjectId='f6afd4e4-6fb2-446f-88dd-815ddb91b09d' ",KeyValue);
            planbll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult CustomerAssUserList()
        {
            return View();
        }

        public ActionResult DeleteCustomerAss(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"delete from Base_ObjectUserRelation where UserID='{0}' and ObjectId='9cc63534-b2cb-4cbb-99d9-50d43942ee47' ", KeyValue);
            planbll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }



    }
}
