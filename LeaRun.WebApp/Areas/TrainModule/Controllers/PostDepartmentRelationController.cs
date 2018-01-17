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

namespace LeaRun.WebApp.Areas.TrainModule.Controllers
{
    public class PostDepartmentRelationController : Controller
    {
        RepositoryFactory<TR_PostDepartmentRelation> repositoryfactory = new RepositoryFactory<TR_PostDepartmentRelation>();
        TR_PostDepartmentRelationBll PostDepartRelationBll = new TR_PostDepartmentRelationBll();
        //
        // GET: /FYModule/Process/

        public ActionResult Index()
        {
            string sql = @" select FullName,case when ParentId='0' then '' else (select FullName Base_Department where DepartmentId=a.ParentId) end 
from Base_Department a where DepartmentId='{0}' ";
            sql = string.Format(sql, ManageProvider.Provider.Current().DepartmentId);
            DataTable dt = PostDepartRelationBll.GetDataTable(sql);
            ViewData["Department"] = dt.Rows[0][0].ToString();
            if (dt.Rows[0][1].ToString() == "")
            {
                ViewData["Department"] = dt.Rows[0][1].ToString() + "-" + dt.Rows[0][0].ToString();
            }

            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, 
            JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PostDepartRelationBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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

        //[HttpPost]
        //public ActionResult SubmitForm(string KeyValue, TR_PostDepartmentRelation entity, string BuildFormJson, HttpPostedFileBase Filedata)
        //{
        //    string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
        //    IDatabase database = DataFactory.Database();
        //    DbTransaction isOpenTrans = database.BeginTrans();
        //    try
        //    {
        //        string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
        //        if (!string.IsNullOrEmpty(KeyValue))
        //        {
        //            if (KeyValue == ManageProvider.Provider.Current().UserId)
        //            {
        //                throw new Exception("无权限编辑信息");
        //            }


        //            entity.Modify(KeyValue);


        //            database.Update(entity, isOpenTrans);

        //        }
        //        else
        //        {

        //            entity.Create();


        //            database.Insert(entity, isOpenTrans);

        //            Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.RelateionID, isOpenTrans);
        //        }
        //        Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.RelateionID, ModuleId, isOpenTrans);
        //        database.Commit();
        //        return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        database.Rollback();
        //        database.Close();
        //        return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
        //    }
        //}

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetForm(string KeyValue)
        {
            TR_PostDepartmentRelation entity = DataFactory.Database().FindEntity<TR_PostDepartmentRelation>(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<TR_PostDepartmentRelation>(array, IsOk.ToString(), Message);
        }


        public ActionResult PostList()
        {
            return View();
        }

        public ActionResult UserList()
        {
            return View();
        }

        public ActionResult GetPostList()
        {
            StringBuilder sb = new StringBuilder();
            string sql = " select a.PostID,a.PostName from TR_Post a ";
            DataTable dt = PostDepartRelationBll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                //都修改为不要选中的状态
                //if (!string.IsNullOrEmpty(dr["relationid"].ToString()))//判断是否选中
                //{
                //    strchecked = "selected";
                //}
                sb.Append("<li title=\"" + dr["PostName"] + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["PostID"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["PostName"] + "</a><i></i>");
                sb.Append("</li>");
            }
            return Content(sb.ToString());
        }

        public ActionResult GetUserList()
        {
            StringBuilder sb = new StringBuilder();
            string sql = "select a.UserID,a.RealName+'('+Code+')' as Name from Base_user a where Enabled=1 ";
            DataTable dt = PostDepartRelationBll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                //都修改为不要选中的状态
                //if (!string.IsNullOrEmpty(dr["relationid"].ToString()))//判断是否选中
                //{
                //    strchecked = "selected";
                //}
                sb.Append("<li title=\"" + dr["Name"] + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["UserID"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["Name"] + "</a><i></i>");
                sb.Append("</li>");
            }
            return Content(sb.ToString());
        }

        public ActionResult PostListSubmit(string DepartmentID, string ObjectId)
        {
            try
            {
                string SqlIsExists = " select * from TR_PostDepartmentRelation where DepartmentID='"+ManageProvider.Provider.Current().DepartmentId+"'";
                DataTable dtIsExists = PostDepartRelationBll.GetDataTable(SqlIsExists);
                string strIsExists = "";
                if(dtIsExists.Rows.Count>0)
                {
                    for(int i=0;i<dtIsExists.Rows.Count;i++)
                    {
                        strIsExists += dtIsExists.Rows[i]["PostID"].ToString()+",";
                    }
                }
                //IDatabase database = DataFactory.Database();
                StringBuilder strSql = new StringBuilder();
                string[] array = ObjectId.Split(',');

               
                for(int i=0;i<array.Length-1;i++)
                {
                    if (strIsExists.IndexOf(array[i]) >= 0)
                    { }
                    else
                    {
                        SqlIsExists = "select * from TR_PostDepartmentRelation where PostID='" + array[i] + "' and DepartmentID=''";
                        //strSql.AppendFormat(@"delete from TR_PostDepartmentRelation where departmentid='{0}' ", ManageProvider.Provider.Current().UserId);
                        strSql.AppendFormat(@" insert into TR_PostDepartmentRelation (relationid,postid,departmentid) 
values(NEWID(),'{0}','{1}') ", array[i], ManageProvider.Provider.Current().DepartmentId);
                    }
                }
                PostDepartRelationBll.ExecuteSql(strSql);
                return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());


            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DepartmentID"></param>
        /// <param name="ObjectId">aec46227-27ad-4870-b0d1-acf156a2d677,69f64349-a957-4393-8488-43eea8e0036d,</param>
        /// <returns></returns>
        public ActionResult UserListSubmit(string DepartmentID, string ObjectId)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();
               
                string[] array = ObjectId.Split(',');
                for(int i=0;i<array.Length-1;i++)
                {
                    strSql.AppendFormat(@" update Base_user set DepartmentId='{0}' where UserId='{1}'  ",
                        ManageProvider.Provider.Current().DepartmentId,array[i]);


                    //删除了该员工原来的岗位以及主管评定
                    strSql.AppendFormat(@" delete from TR_EvaluateDetail 
where UserPostRelationID in (select UserPostRelationID from TR_UserPost where UserID='{0}') ",array[i]);
                     
                    strSql.AppendFormat(@" update tr_userpost set IsEnable=0 where userid='{0}' ", array[i]);
                }
                PostDepartRelationBll.ExecuteSql(strSql);
                return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());


            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }


        }


        public ActionResult GetDetailList(string RelationID)
        {
            try
            {
                var JsonData = new
                {
                    rows = PostDepartRelationBll.GetDetailList(RelationID),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult GetEvaluateDetailList(string RelationID)
        {
            try
            {
                //首先插入数据
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" insert into TR_EvaluateDetail 
select  NEWID(),'{0}',d.SkillID,d.SkillName,0,c.SkillWeight from TR_UserPost a
left join TR_PostDepartmentRelation b on a.DepartmentPostID=b.RelationID
left join TR_PostDepartmentRelationDetail c on b.RelationID=c.RelationID
left join TR_Skill d on c.SkillID=d.SkillID
where a.UserPostRelationID='{0}'
and not exists (select * from TR_EvaluateDetail where UserPostRelationID='{0}' and skillid=d.SkillID) 
 ",
RelationID);
                PostDepartRelationBll.ExecuteSql(strSql);
                var JsonData = new
                {
                    rows = PostDepartRelationBll.GetEvaluateDetailList(RelationID),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }
        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, string DetailForm)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            database.Delete<TR_PostDepartmentRelationDetail>("RelationID", KeyValue, isOpenTrans);
            List<TR_PostDepartmentRelationDetail> DetailList = DetailForm.JonsToList<TR_PostDepartmentRelationDetail>();
            int index = 1;
            foreach (TR_PostDepartmentRelationDetail entity in DetailList)
            {
                if (!string.IsNullOrEmpty(entity.SkillName))
                {
                    entity.Create();
                    entity.RelationID = KeyValue;
                    database.Insert(entity, isOpenTrans);
                    index++;
                }
            }
            database.Commit();

            return Content(new JsonMessage { Success = true, Code = "1", Message = "保存成功" }.ToString());
        }
        [HttpPost]
        public ActionResult SubmitEvaluateForm(string KeyValue, string DetailForm)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            //database.Delete<TR_EvaluateDetail>("UserPostRelationID", KeyValue, isOpenTrans);
            List<TR_EvaluateDetail> DetailList = DetailForm.JonsToList<TR_EvaluateDetail>();
            int index = 1;
            decimal srd = 0;
            decimal srdFz = 0;
            decimal srdFm = 0;
            foreach (TR_EvaluateDetail entity in DetailList)
            {
                if (!string.IsNullOrEmpty(entity.EvaluateID))
                {
                    entity.Modify(entity.EvaluateID);
                    entity.UserPostRelationID = KeyValue;
                    database.Update(entity, isOpenTrans);
                    srdFz += entity.SkillWeight*entity.EvaluateScore;
                    srdFm += entity.SkillWeight;

                    index++;
                }
            }
            srd = srdFz / srdFm;
            srd = Math.Round(srd, 0);
            database.Commit();

            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" update  TR_UserPost set Evaluate='{0}' where UserPostRelationID='{1}' ",
                srd,KeyValue);
            PostDepartRelationBll.ExecuteSql(strSql);

            return Content(new JsonMessage { Success = true, Code = "1", Message = "评审成功" }.ToString());
        }

        public ActionResult SkillList()
        {
            return View();
        }

        public ActionResult GetSkillListJson(string keywords, string CompanyId, string DepartmentId,
            JqGridParam jqgridparam, string ParameterJson,string SkillName,string SkillType)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PostDepartRelationBll.GetSkillList(keywords, ref jqgridparam, ParameterJson,SkillName,SkillType);
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

        public ActionResult UserIndex()
        {
            string sql = @" select FullName,case when ParentId='0' then '' else (select FullName from Base_Department where DepartmentId=a.ParentId) end 
from Base_Department a where DepartmentId='{0}' ";
            sql = string.Format(sql, ManageProvider.Provider.Current().DepartmentId);
            DataTable dt = PostDepartRelationBll.GetDataTable(sql);
            ViewData["Department"] = dt.Rows[0][0].ToString();

            //判断是否部门负责人，如果部门负责人在添加的时候需要做提示操作
            string Isdept = "0";

            if (dt.Rows[0][1].ToString() == "")
            {
                
                Isdept = "1";
            }
            else
            {
                ViewData["Department"] = dt.Rows[0][1].ToString() + "-" + dt.Rows[0][0].ToString();
            }
            ViewData["Isdept"] = Isdept;

            return View();
        }


        public ActionResult GetUserListJson(string keywords, string CompanyId, string DepartmentId,
            JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PostDepartRelationBll.GetUserList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult DepartPostList()
        {
            return View();
        }


        public ActionResult GetDepartPostList(string UserID,string IsMain)
        {
            StringBuilder sb = new StringBuilder();
            string sql = @" select a.RelationID,b.PostName,c.UserPostRelationID from TR_PostDepartmentRelation a left join TR_Post b on a.PostID=b.PostID left join TR_UserPost c on
a.RelationID=c.DepartmentPostID and c.userid='"+UserID+ "' and c.IsMain='" + IsMain + "' where a.DepartmentID='" + ManageProvider.Provider.Current().DepartmentId+"'  ";
            DataTable dt = PostDepartRelationBll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                if (!string.IsNullOrEmpty(dr["UserPostRelationID"].ToString()))//判断是否选中
                {
                    strchecked = "selected";
                }
                sb.Append("<li title=\"" + dr["PostName"] + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["RelationID"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["PostName"] + "</a><i></i>");
                sb.Append("</li>");
            }
            return Content(sb.ToString());
        }

        public ActionResult DepartPostListSubmit(string UserID, string ObjectId,string IsMain)
        {
            try
            {
                //IDatabase database = DataFactory.Database();
                StringBuilder strSql = new StringBuilder();
                string[] array = ObjectId.Split(',');
                if(IsMain=="1"&&array.Length>2)
                {
                    return Content(new JsonMessage { Success = false, Code = "-1", Message = "主岗只能有一个" }.ToString());
                }
                strSql.AppendFormat(@"delete from TR_UserPost where UserID='{0}' and IsMain={1} ", UserID,IsMain);
                for (int i = 0; i < array.Length - 1; i++)
                {
                    
                    strSql.AppendFormat(@" insert into TR_UserPost (UserPostRelationID,UserID,DepartmentPostID,IsMain) 
values(NEWID(),'{0}','{1}','{2}') ", UserID, array[i],IsMain);
                }
                PostDepartRelationBll.ExecuteSql(strSql);
                return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());


            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }
        }

        public ActionResult SkillTypeJson()
        {
            string sql = " select distinct SkillType from TR_Skill where 1=1 ";
            DataTable dt = PostDepartRelationBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult SkillListBatch()
        {
            return View();
        }

        public ActionResult EvaluateList()
        {
            return View();
        }

        public ActionResult GridEvaluateListJson(string keywords, string CompanyId, string DepartmentId,
           JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PostDepartRelationBll.GetEvaluateList(keywords, ref jqgridparam, ParameterJson);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Type">1.完全掌握，2.熟练掌握，3.基本掌握，4。尚未掌握</param>
        public int UpdateEvaluate(string ID,string Type)
        {
            string Evaluate = "";
            switch (Type)
            {
                case "1":
                    Evaluate = "完全掌握";
                    break;
                case "2":
                    Evaluate = "熟练掌握";
                    break;
                case "3":
                    Evaluate = "基本掌握";
                    break;
                case "4":
                    Evaluate = "尚未掌握";
                    break;
                default:
                    Evaluate = "未知错误";
                    return -1;
                    break;
            }
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"update tr_userpost set Evaluate='{0}' where UserPostRelationID='{1}'",Evaluate,ID);
            PostDepartRelationBll.ExecuteSql(strSql);
            return 0;  
                
        }

        public ActionResult EvaluateForm()
        {
            return View();
        }

        public int QuitOne(string UserID)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" update Base_User set  Enabled=0 where UserID='{0}' ", UserID);
                return PostDepartRelationBll.ExecuteSql(strSql);
            }
            catch
            {
                return -1;
            }


        }


    }
}
