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
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace LeaRun.WebApp.Areas.FYModule.Controllers
{
    public class ChangeController : Controller
    {
        RepositoryFactory<FY_Change> repositoryfactory = new RepositoryFactory<FY_Change>();
        FY_ChangeBll ChangeBll = new FY_ChangeBll();
        Base_CodeRuleBll base_coderulebll = new Base_CodeRuleBll();
        //
        // GET: /FYModule/Process/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson,
            string IsMy)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ChangeBll.GetPageList(keywords, ref jqgridparam, ParameterJson,IsMy);
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

        private string BillCode()
        {
            string UserId = ManageProvider.Provider.Current().UserId;
            //string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            string ModuleId = "872893dd-8124-4a0d-aec9-fbecd0b90c52";
            return base_coderulebll.GetBillCode(UserId, ModuleId);
        }

        public ActionResult Form()
        {
            string IsManager = "0"; //表示不是部门负责人,当是部门负责人的时候，这个变量改为部门的ID
            string IsTopManager = "0";  //总经理角色，0表示不是总经理，1表示是总经理  
            string IsFi = "0"; //表示是否有财务的权限
            string IsProjectLeader = "0";  //表示是否是项目主管
            ViewData["UserID"] = ManageProvider.Provider.Current().UserId;

            string sql = " select * from Base_ObjectUserRelation where ObjectId='e65b5d68-3d6e-4865-9de4-c79a631fda48' and UserId='"+ManageProvider.Provider.Current().UserId+"'  ";
            DataTable dt = ChangeBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                IsManager = ManageProvider.Provider.Current().DepartmentId;
            }
            ViewData["IsManager"] = IsManager;

            sql = " select * from Base_ObjectUserRelation where ObjectId='15f14d9c-e74c-46ac-8641-b3c1bac26940' and UserId='" + ManageProvider.Provider.Current().UserId + "'  ";
            dt = ChangeBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                IsTopManager = "1";
            }
            ViewData["IsTopManager"] = IsTopManager;

            sql = " select * from Base_ObjectUserRelation where ObjectId='259b1ace-50ca-42b3-b85e-44e72ab7dd64' and UserId='" + ManageProvider.Provider.Current().UserId + "'  ";
            dt = ChangeBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                IsFi = "1";
            }
            ViewData["IsFi"] = IsFi;

            sql = " select * from Base_ObjectUserRelation where ObjectId='6ed93a98-4031-4c2f-919a-18dc3abe71ed' and UserId='" + ManageProvider.Provider.Current().UserId + "'  ";
            dt = ChangeBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                IsProjectLeader = "1";
            }
            ViewData["IsProjectLeader"] = IsProjectLeader;
            //string KeyValue = Request["KeyValue"];
            //if (string.IsNullOrEmpty(KeyValue))
            //{
            //    ViewBag.BillNo = this.BillCode();
            //    ViewBag.CreateUserName = ManageProvider.Provider.Current().UserName;
            //}
            return View();
        }

        public ActionResult FormNew()
        {
            Base_NoteNOBll notenobll = new Base_NoteNOBll();
            string KeyValue = Request["KeyValue"];
            if (string.IsNullOrEmpty(KeyValue))
            {
                ViewBag.BillNo = notenobll.Code("ChangeNote");
                ViewBag.CreateUserName = ManageProvider.Provider.Current().UserName;
            }
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, FY_Change entity, string BuildFormJson, HttpPostedFileBase Filedata)
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
                    //entity.DepartmentID = ManageProvider.Provider.Current().DepartmentId;
                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.ChangeID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ChangeID, ModuleId, isOpenTrans);
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
            FY_Change entity = DataFactory.Database().FindEntity<FY_Change>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        public ActionResult DepartmentJson()
        {
            string sql = " select distinct DepartmentId,FullName from Base_Department where 1=1 ";
            DataTable dt = ChangeBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult AddChangeUser(string ChangeID)
        {
            ViewData["ChangeID"] = ChangeID;
            return View();
        }

        public ActionResult GetUserList(string ChangeID)
        {
            StringBuilder sb = new StringBuilder();
            string sql = "  select * from Base_User a left join FY_ChangeUser b on a.UserId=b.UserID and b.changeID='"+ChangeID+"' where a.Enabled=1 ";
            DataTable dt =  ChangeBll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                if (!string.IsNullOrEmpty(dr["ChangeID"].ToString()))//判断是否选中
                {
                    strchecked = "selected";
                }
                sb.Append("<li title=\"" + dr["RealName"] + "(" + dr["Code"] + ")" + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["UserId"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["RealName"] + "</a><i></i>");
                sb.Append("</li>");
            }
            return Content(sb.ToString());
        }

        /// <summary>
        /// 添加评审人，模态窗口提交以后的方法
        /// </summary>
        /// <param name="ChangeID"></param>
        /// <param name="ObjectId"></param>
        /// <returns></returns>
        public ActionResult UserListSubmit(string ChangeID, string ObjectId)
        {
            try
            {
                
                StringBuilder strSql = new StringBuilder();
                string[] array = ObjectId.Split(',');

                strSql.AppendFormat(@"delete from FY_ChangeUser where ChangeID='{0}' ",ChangeID);

                for(int i=0;i<array.Length-1;i++)
                {
                    strSql.AppendFormat(@"insert into FY_ChangeUser values(NEWID(),'{0}','{1}')", ChangeID, array[i]);
                }
                ChangeBll.ExecuteSql(strSql);
                return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());

            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }
        }

        public ActionResult SetChangeUser(string KeyValue)
        {
            string result = "";
            string sql = "select b.RealName from FY_ChangeUser a left join Base_User b on a.UserID=b.UserId where a.ChangeID='" + KeyValue+"'";
            DataTable dt = ChangeBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                for(int i=0;i<dt.Rows.Count;i++)
                {
                    result += dt.Rows[i][0].ToString() + ",";
                }
                result = result.Substring(0, result.Length - 1);
            }
            //return result;
            return Content(new JsonMessage { Success = true, Code = 1.ToString(),Content=result, Message = "操作成功。" }.ToString());
        }

        public ActionResult GetChangeReviewList(string ChangeID)
        {
            try
            {
                var JsonData = new
                {
                    rows = ChangeBll.GetChangeReviewList(ChangeID),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult GetChangeCostList(string ChangeID)
        {
            try
            {
                var JsonData = new
                {
                    rows = ChangeBll.GetChangeCostList(ChangeID),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult GetChangeBreakList(string ChangeID)
        {
            try
            {
                var JsonData = new
                {
                    rows = ChangeBll.GetChangeBreakList(ChangeID),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult SendAudit(string ChangeID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" insert into FY_ChangeReview
select NEWID(),DepartmentID,ResponseID,ChangeData,'',GETDATE(),'{0}',ChangeDataID,'未提交','',0,'' 
from FY_ChangeData a
where not exists (select * from FY_ChangeReview where ChangeDataID=a.ChangeDataID and ChangeID='{0}')  
and a.CreateBy='{1}' ", ChangeID,ManageProvider.Provider.Current().UserId);
            strSql.AppendFormat(@"update FY_Change set ChangeState='等待评审' where ChangeID='{0}'  ", ChangeID);
            strSql.AppendFormat(@"insert into fy_changelog values (newid(),'发送评审',getdate(),'{0}','{1}') ",ManageProvider.Provider.Current().UserId,ChangeID);
            ChangeBll.ExecuteSql(strSql);

            //发送邮件提醒
            string reciver = "";
            string content = "您好，你有一个变更单需要评审！";
            string sql = @"select b.Email   
from FY_ChangeData a left join Base_User b on a.ResponseID=b.UserId where 
a.CreateBy='{0}' ";
            sql = string.Format(sql, ManageProvider.Provider.Current().UserId);
            DataTable dt = ChangeBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                for(int i=0;i<dt.Rows.Count;i++)
                {
                    reciver = reciver + dt.Rows[i][0].ToString() + ",";
                    
                }
                reciver = reciver.Substring(0, reciver.Length - 1);
            }
            SendEmailByAccount(reciver, content);

            return Content(new JsonMessage { Success = true, Code = 1.ToString(),Message = "操作成功。" }.ToString());
        }

        public ActionResult SubmitReview(string ChangeReviewID,string Measures,string PlanDate,string Result,string DetailForm)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            List<FY_ChangeReview> DetailList = DetailForm.JonsToList<FY_ChangeReview>();
            foreach (FY_ChangeReview entityD in DetailList)
            {
                if (!string.IsNullOrEmpty(entityD.ChangeReviewID))
                {


                    entityD.Modify(entityD.ChangeReviewID);
                    
                    database.Update(entityD, isOpenTrans);
                    //index++;
                }
            }
            database.Commit();

            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"update FY_ChangeReview set Measures='{0}',PlanDate='{1}',ChangeReviewState='进行中',Result='{3}' where ChangeReviewID='{2}' ",
                Measures,PlanDate,ChangeReviewID,Result);
            ChangeBll.ExecuteSql(strSql);

            //发送邮件提醒部门负责人审批
            string reciver = "";
            string content = "您好，有一个变更单需要您审批评审内容！";
            string sql = @"select Email from Base_User where 
exists (select * from Base_ObjectUserRelation where UserId='{1}' and ObjectId='e65b5d68-3d6e-4865-9de4-c79a631fda48')
and DepartmentId in (select ReviewDepart from FY_ChangeReview  where ChangeReviewID='{0}') ";
            sql = string.Format(sql,ChangeReviewID,ManageProvider.Provider.Current().UserId);
            DataTable dt = ChangeBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reciver = reciver + dt.Rows[i][0].ToString() + ",";

                }
                reciver = reciver.Substring(0, reciver.Length - 1);
            }
            SendEmailByAccount(reciver, content);

            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult SubmitBreak(string ChangeBreakID,string StockMessage,string DetailForm)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            List<FY_ChangeBreak> DetailList = DetailForm.JonsToList<FY_ChangeBreak>();
            foreach (FY_ChangeBreak entityD in DetailList)
            {
                if (!string.IsNullOrEmpty(entityD.ChangeBreakID))
                {


                    entityD.Modify(entityD.ChangeBreakID);

                    database.Update(entityD, isOpenTrans);
                    //index++;
                }
            }
            database.Commit();


            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"update FY_ChangeBreak set StockMessage='{0}',BreakState='进行中' where ChangeBreakID='{1}' ",
                StockMessage, ChangeBreakID);
            ChangeBll.ExecuteSql(strSql);

            //发送邮件提醒部门负责人审批
            string reciver = "";
            string content = "您好，有一个变更单需要您审批评审内容！";
            string sql = @"select Email from Base_User where 
exists (select * from Base_ObjectUserRelation where UserId='{1}' and ObjectId='e65b5d68-3d6e-4865-9de4-c79a631fda48')
and DepartmentId in (select ChangeBreakDeptID from FY_ChangeBreak  where ChangeBreakID='{0}') ";
            sql = string.Format(sql, ChangeBreakID, ManageProvider.Provider.Current().UserId);
            DataTable dt = ChangeBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reciver = reciver + dt.Rows[i][0].ToString() + ",";

                }
                reciver = reciver.Substring(0, reciver.Length - 1);
            }
            SendEmailByAccount(reciver, content);

            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult ApproveReview(string ChangeReviewID,string tag)
        {
            string State = "";
            if(tag=="Yes")
            {
                State = "已完成";
            }
            if(tag=="No")
            {
                State = "退回";
            }
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"update FY_ChangeReview set ChangeReviewState='{0}' where ChangeReviewID='{1}' ",
                State,ChangeReviewID);

            ChangeBll.ExecuteSql(strSql);

            string sql = " select * from FY_ChangeReview where ChangeReviewState!='已完成' and ChangeID in (select ChangeID from FY_ChangeReview where ChangeReviewID='"+ChangeReviewID+"')  ";
            DataTable dt = ChangeBll.GetDataTable(sql);
            if(dt.Rows.Count<=0)
            {
                StringBuilder strSql2 = new StringBuilder();
                strSql2.AppendFormat(@"update FY_Change set ChangeState='评审完成' where ChangeID  in (select ChangeID from FY_ChangeReview where ChangeReviewID='{0}') ",ChangeReviewID);
                ChangeBll.ExecuteSql(strSql2);

                //发送邮件提醒
                string reciver = "";
                string content1 = "您好，您的变更单已通过评审！";
                string sql11 = @"select Email from FY_Change a left join Base_User b on a.CreateByID=b.UserId 
where ChangeID  in (select ChangeID from FY_ChangeReview where ChangeReviewID='{0}') ";
                sql11 = string.Format(sql11, ChangeReviewID);
                DataTable dt11 = ChangeBll.GetDataTable(sql);
                if (dt11.Rows.Count > 0)
                {
                    for (int i = 0; i < dt11.Rows.Count; i++)
                    {
                        reciver = reciver + dt11.Rows[i][0].ToString() + ",";

                    }
                    reciver = reciver.Substring(0, reciver.Length - 1);
                }
                SendEmailByAccount(reciver, content1);
            }
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult EndReview(string ChangeReviewID,string FllowStatus)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" update FY_ChangeReview set FllowStatus='{0}',IsEnd=1 where ChangeReviewID='{1}' ",
                FllowStatus,ChangeReviewID);
            ChangeBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult ApproveBreak(string ChangeBreakID,string tag)
        {
            string State = "";
            if (tag == "Yes")
            {
                State = "已完成";
            }
            if (tag == "No")
            {
                State = "退回";
            }
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"update FY_ChangeBreak set BreakState='{0}' where ChangeBreakID='{1}' ",
                State, ChangeBreakID);

            ChangeBll.ExecuteSql(strSql);

            string sql = " select * from FY_ChangeBreak where BreakState!='已完成' and ChangeID in (select ChangeID from FY_ChangeBreak where ChangeBreakID='" + ChangeBreakID + "')  ";
            DataTable dt = ChangeBll.GetDataTable(sql);
            if (dt.Rows.Count <= 0)
            {
                StringBuilder strSql2 = new StringBuilder();
                strSql2.AppendFormat(@"update FY_Change set ChangeState='等待财务确认' where ChangeID  in (select ChangeID from FY_ChangeBreak where ChangeBreakID='{0}') ", ChangeBreakID);
                ChangeBll.ExecuteSql(strSql2);

                //发送邮件提醒
                string reciver = " yinan.wang@fuyaogroup.com ";
                string content1 = "您好，您有一个变更单号需要审批！请登录系统查看 http://172.19.0.5：8086";
                //            string sql = @"select Email from FY_Change a left join Base_User b on a.CreateByID=b.UserId 
                //where changeid='{0}' ";
                //            sql = string.Format(sql, ChangeID);
                //            DataTable dt = ChangeBll.GetDataTable(sql);
                //            if (dt.Rows.Count > 0)
                //            {
                //                for (int i = 0; i < dt.Rows.Count; i++)
                //                {
                //                    reciver = reciver + dt.Rows[i][0].ToString() + ",";

                //                }
                //                reciver = reciver.Substring(0, reciver.Length - 1);
                //            }
                SendEmailByAccount(reciver, content1);
            }
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult SendTopManagerAudit(string ChangeID,string content)
        {
            StringBuilder strSql = new StringBuilder();
            if (content != "")
            {
                strSql.AppendFormat(@"update FY_Change set CreateOpinion='{1}',ChangeState='等待总经理批准' where ChangeID='{0}' ", ChangeID, content);
            }
            else
            {
                strSql.AppendFormat(@"update FY_Change set ChangeState='等待总经理批准' where ChangeID='{0}' ", ChangeID);
            }
            strSql.AppendFormat(@"insert into fy_changelog values (newid(),'提交总经理',getdate(),'{0}','{1}') ", ManageProvider.Provider.Current().UserId,ChangeID);
            ChangeBll.ExecuteSql(strSql);

            //发送邮件提醒
            string reciver = " qingfa.chen@fuyaogroup.com ";
            string content1 = "您好，您有一个变更单号需要审批！请登录系统查看 http://172.19.0.5：8086";
//            string sql = @"select Email from FY_Change a left join Base_User b on a.CreateByID=b.UserId 
//where changeid='{0}' ";
//            sql = string.Format(sql, ChangeID);
//            DataTable dt = ChangeBll.GetDataTable(sql);
//            if (dt.Rows.Count > 0)
//            {
//                for (int i = 0; i < dt.Rows.Count; i++)
//                {
//                    reciver = reciver + dt.Rows[i][0].ToString() + ",";

//                }
//                reciver = reciver.Substring(0, reciver.Length - 1);
//            }
            SendEmailByAccount(reciver, content1);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult TopManagerAudit1(string ChangeID, string tag, string content)
        {
            string State = "";
            if (tag == "Yes")
            {
                State = "总经理审批通过";

                //发送邮件提醒
                string reciver = "";
                string content1 = "您好，总经理已审批通过你的变更单，请执行下一步操作！";
                string sql = @"select Email from FY_Change a left join Base_User b on a.CreateByID=b.UserId 
where changeid='{0}' ";
                sql = string.Format(sql, ChangeID);
                DataTable dt = ChangeBll.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        reciver = reciver + dt.Rows[i][0].ToString() + ",";

                    }
                    reciver = reciver.Substring(0, reciver.Length - 1);
                }
                SendEmailByAccount(reciver, content1);
            }
            else if (tag == "No")
            {
                State = "总经理回退";

                //发送邮件提醒
                string reciver = "";
                string content1 = "您好，您的变更单被总经理退回！";
                string sql = @"select Email from FY_Change a left join Base_User b on a.CreateByID=b.UserId 
where changeid='{0}' ";
                sql = string.Format(sql, ChangeID);
                DataTable dt = ChangeBll.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        reciver = reciver + dt.Rows[i][0].ToString() + ",";

                    }
                    reciver = reciver.Substring(0, reciver.Length - 1);
                }
                SendEmailByAccount(reciver, content1);
            }
            else
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败。" }.ToString());
            }
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" update FY_Change set ChangeState='{1}',Approve1Dt=getdate(),Approve1By='{2}' ,TopManager1Remark='{3}'
where ChangeID='{0}' ", ChangeID, State, ManageProvider.Provider.Current().UserName, content);
            if (State == "总经理审批通过")
            {
                strSql.AppendFormat(@" update FY_Change set IsChangeOver='评审通过' where changeid='{0}' ", ChangeID);
            }
            strSql.AppendFormat(@"insert into fy_changelog values (newid(),'{1}',getdate(),'{0}','{2}') ", ManageProvider.Provider.Current().UserId, State, ChangeID);
            ChangeBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult TopManagerAudit(string ChangeID,string tag,string content)
        {
            string State = "";
            if(tag=="Yes")
            {
                State = "总经理审批通过";
            }
            else if(tag=="No")
            {
                State = "总经理回退";
            }
            else
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败。" }.ToString());
            }
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" update FY_Change set ChangeState='{1}',ApproveDt=getdate(),ApproveBy='{2}' ,TopManagerRemark='{3}'
where ChangeID='{0}' ", ChangeID,State,ManageProvider.Provider.Current().UserName,content);
            //if(State=="总经理审批通过")
            //{
            //    strSql.AppendFormat(@" update FY_Change set IsChangeOver='评审通过' where changeid='{0}' ",ChangeID);
            //}
            strSql.AppendFormat(@"insert into fy_changelog values (newid(),'{1}',getdate(),'{0}','{2}') ", ManageProvider.Provider.Current().UserId,State,ChangeID);
            ChangeBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult TopManagerForm()
        {
            return View();
        }

        public ActionResult SaveCostDetail(string ChangeID,string DetailForm)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            database.Delete<FY_ChangeCost>("ChangeID", ChangeID, isOpenTrans);
            List<FY_ChangeCost> POOrderEntryList = DetailForm.JonsToList<FY_ChangeCost>();
            int index = 1;
            foreach (FY_ChangeCost costentry in POOrderEntryList)
            {
                if (!string.IsNullOrEmpty(costentry.CostType))
                {
                    
                    costentry.Create();
                    costentry.ChangeID = ChangeID;
                    database.Insert(costentry, isOpenTrans);
                    index++;
                }
            }
            database.Commit();

            return Content(new JsonMessage { Success = true, Code = "1", Message = "保存成功" }.ToString());
        }

        public ActionResult SaveBreakDetail(string ChangeID,string DetailForm)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            database.Delete<FY_ChangeBreak>("ChangeID", ChangeID, isOpenTrans);
            List<FY_ChangeBreak> POOrderEntryList = DetailForm.JonsToList<FY_ChangeBreak>();
            int index = 1;
            foreach (FY_ChangeBreak breakentry in POOrderEntryList)
            {
                if (!string.IsNullOrEmpty(breakentry.ChangeBreakName))
                {

                    breakentry.Create();
                    breakentry.ChangeID = ChangeID;
                    database.Insert(breakentry, isOpenTrans);
                    index++;
                }
            }
            database.Commit();

            return Content(new JsonMessage { Success = true, Code = "1", Message = "保存成功" }.ToString());
        }

        public ActionResult InsertCost(string ChangeID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"insert into FY_ChangeCost (CostID,ChangeID,CostType)
select NEWID(),'{0}',Code from Base_DataDictionaryDetail a where DataDictionaryId='b31c1ff6-db7b-4842-a1d4-722fa6712c62'
and not exists (select * from FY_ChangeCost where CostType=a.Code and ChangeID='{0}') ",ChangeID);
            strSql.AppendFormat(@"insert into fy_changelog values (newid(),'填写费用',getdate(),'{0}','{1}') ", ManageProvider.Provider.Current().UserId, ChangeID);
            ChangeBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult InsertBreak(string ChangeID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"insert into fy_changebreak  
select NEWID(),breakname,case when  b.RealName='待定' then '{1}' else b.RealName end,
case when a.responseby='d7366d2c-d10b-4826-8659-0115edea086e' then '{2}' else a.ResponseBy end,'','','{0}','未提交',b.DepartmentId
from fy_breakpoint a left join Base_User b on a.responseby=b.UserId where not exists
(select * from fy_changebreak where changebreakname=a.BreakName and changeid='{0}') ", ChangeID,ManageProvider.Provider.Current().UserName,
ManageProvider.Provider.Current().UserId);
            strSql.AppendFormat(@"update FY_Change set ChangeState='等待库存确认' where ChangeID='{0}' ", ChangeID);
            strSql.AppendFormat(@"insert into fy_changelog values (newid(),'提交断点确认',getdate(),'{0}','{1}') ", ManageProvider.Provider.Current().UserId, ChangeID);
            ChangeBll.ExecuteSql(strSql);

            //发送邮件提醒填写断点信息
            string reciver = "";
            string content = "您好，你有一个变更单需要填写断点信息！";
            string sql = @"select b.Email   
from fy_breakpoint a left join Base_User b on a.ResponseBy=b.UserId where 
1=1 ";
            sql = string.Format(sql, ManageProvider.Provider.Current().UserId);
            DataTable dt = ChangeBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reciver = reciver + dt.Rows[i][0].ToString() + ",";

                }
                reciver = reciver.Substring(0, reciver.Length - 1);
            }
            SendEmailByAccount(reciver,content);

            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult EndNote(string ChangeID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"update fy_change set changeState='已完成' where changeid='{0}' ",ChangeID);
            strSql.AppendFormat(@"insert into fy_changelog values (newid(),'结单',getdate(),'{0}','{1}') ", ManageProvider.Provider.Current().UserId,ChangeID);
            ChangeBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult LogForm(string KeyValue)
        {
            string sql = @" select b.RealName,ChangeLogContent,c.ChangeNO,c.ProjectName,CONVERT(varchar, a.ChangeLogDt, 20 )   
from FY_ChangeLog a 
left join Base_User b on a.CreateBy=b.UserId
left join FY_Change c on a.ChangeID=c.ChangeID where a.changeid='"+KeyValue+ "' order by a.ChangeLogDt ";
            DataTable dt = ChangeBll.GetDataTable(sql);
            ViewData["dt"] = dt;
            return View();
        }

        public ActionResult FiConfirm(string ChangeID,string ScrappeGlass,string ScrappeMaterial,string ScrappeAll,
            string FiRemark)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"update fy_change set CreateState='等待发起人填写意见',changeState='财务确认完成',ScrappeGlass='{0}',ScrappeMaterial='{1}',
ScrappeAll='{2}',FiRemark='{3}',FiBy='{4}',FyDt=getdate() where changeid='{5}' ",
ScrappeGlass, ScrappeMaterial, ScrappeAll, FiRemark, ManageProvider.Provider.Current().UserName, ChangeID);
            strSql.AppendFormat(@"insert into fy_changelog values (newid(),'等待发起人填写意见',getdate(),'{0}','{1}') ", ManageProvider.Provider.Current().UserId, ChangeID);

            //

           


            ChangeBll.ExecuteSql(strSql);

            //发送邮件提醒填写断点信息
            string reciver = "";
            string content = "您好，你的变更单财务已审批，请登录系统填写意见！";
            string sql = @" select Email from FY_Change a left join Base_User b on a.CreateByID=b.UserId where ChangeID='{0}' ";
            sql = string.Format(sql, ChangeID);
            DataTable dt = ChangeBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    reciver = reciver + dt.Rows[i][0].ToString() + ",";

                }
                reciver = reciver.Substring(0, reciver.Length - 1);
            }
            SendEmailByAccount(reciver, content);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult CreateApprove(string ChangeID,string tag,string content)
        {
            string result = "";
            StringBuilder strSql = new StringBuilder();
            if (tag=="Yes")
            {
                result = "部门负责人审核通过";
                strSql.AppendFormat(@"update fy_change set ChangeState='部门负责人审核通过',
Manager='{1}',ManagerDt=getdate(),ManagerRemark='同意' where changeid='{0}' ", ChangeID,ManageProvider.Provider.Current().UserName);
            }
            else if(tag=="No")
            {
                result = "部门负责人退回";
            }
            else
            {
                result = "等待部门负责人审核";

                //发送邮件提醒填写断点信息
                string reciver = "";
                string content1 = "您好，您有一个变更单需要审批！";
                string sql = @" select Email from Base_User where 
exists (select * from Base_ObjectUserRelation where UserId='' and ObjectId='e65b5d68-3d6e-4865-9de4-c79a631fda48')
and DepartmentId in (select DepartmentId from Base_User a where a.UserId in (select CreateByID from FY_Change where ChangeID='{0}')) ";
                sql = string.Format(sql, ChangeID);
                DataTable dt = ChangeBll.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        reciver = reciver + dt.Rows[i][0].ToString() + ",";

                    }
                    reciver = reciver.Substring(0, reciver.Length - 1);
                }
                SendEmailByAccount(reciver, content1);

            }
            strSql.AppendFormat(@"update fy_change set CreateState='{0}',CreateRemark='{1}' where changeid='{2}' ",
                result,content,ChangeID);
            if(result == "等待部门负责人审核")
            {
                result = "提交给部门负责人审批";
            }
            strSql.AppendFormat(@"insert into fy_changelog values (newid(),'{2}',getdate(),'{0}','{1}') ", ManageProvider.Provider.Current().UserId, ChangeID,result);
            ChangeBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }


        public ActionResult ProjectApprove(string ChangeID,string tag)
        {
            string result = "";
            StringBuilder strSql = new StringBuilder();
            if(tag=="Yes")
            {
                result = "项目主管评审通过";

                string reciver = "";
                string content1 = "您好，您发起的变更单项目主管已审批通过！";
                string sql = @"select Email from FY_Change a left join Base_User b on a.CreateByID=b.UserId 
                where changeid='{0}' ";
                sql = string.Format(sql, ChangeID);
                DataTable dt = ChangeBll.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        reciver = reciver + dt.Rows[i][0].ToString() + ",";

                    }
                    reciver = reciver.Substring(0, reciver.Length - 1);
                }
                SendEmailByAccount(reciver, content1);
            }
            else if(tag=="No")
            {
                result = "项目主管退回";

                string reciver = "";
                string content1 = "您好，您发起的变更单被项目主管退回！";
                string sql = @"select Email from FY_Change a left join Base_User b on a.CreateByID=b.UserId 
                where changeid='{0}' ";
                sql = string.Format(sql, ChangeID);
                DataTable dt = ChangeBll.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        reciver = reciver + dt.Rows[i][0].ToString() + ",";

                    }
                    reciver = reciver.Substring(0, reciver.Length - 1);
                }
                SendEmailByAccount(reciver, content1);
            }
            else
            {
                result = "等待项目主管评审";

                string reciver = "yongqiang.li@fuyaogroup.com";
                string content1 = "您好，您有一个变更单需要审批！";
//                string sql = @"select Email from FY_Change a left join Base_User b on a.CreateByID=b.UserId 
//where changeid='{0}' ";
//                sql = string.Format(sql, ChangeID);
//                DataTable dt = ChangeBll.GetDataTable(sql);
//                if (dt.Rows.Count > 0)
//                {
//                    for (int i = 0; i < dt.Rows.Count; i++)
//                    {
//                        reciver = reciver + dt.Rows[i][0].ToString() + ",";

//                    }
//                    reciver = reciver.Substring(0, reciver.Length - 1);
//                }
                SendEmailByAccount(reciver, content1);
            }
            strSql.AppendFormat(@"update fy_change set changestate='{0}' where changeid='{1}' ",result,ChangeID);
            if (result == "等待项目主管评审")
            {
                result = "提交给项目主管审批";
            }
            strSql.AppendFormat(@"insert into fy_changelog values (newid(),'{2}',getdate(),'{0}','{1}') ", ManageProvider.Provider.Current().UserId, ChangeID, result);
            ChangeBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult FlowMapForm(string KeyValue)
        {
            string sql = " select ChangeState,CreateState from fy_change where changeid='" + KeyValue+"' ";
            DataTable dt = ChangeBll.GetDataTable(sql);
            string ChangeState = "";
            string CreateState = "";

            if(dt.Rows.Count>0)
            {
                ChangeState = dt.Rows[0]["ChangeState"].ToString();
                CreateState = dt.Rows[0]["CreateState"].ToString();
            }

            ViewData["ChangeState"] = ChangeState;

            ViewData["CreateState"] = CreateState;

            return View();
        }


        public string SendEmailByAccount(string reciver,string Content)
        {
            var emailAcount = "shfy_it@fuyaogroup.com";
            var emailPassword = "Sj1234";

            string[] reviverArr = reciver.Split(',');


            var content = Content;
            MailMessage message = new MailMessage();
            //设置发件人,发件人需要与设置的邮件发送服务器的邮箱一致
            MailAddress fromAddr = new MailAddress("shfy_it@fuyaogroup.com");
            message.From = fromAddr;
            //设置收件人,可添加多个,添加方法与下面的一样
            for(int i=0;i<reviverArr.Length;i++)
            {
                message.To.Add(reviverArr[i]);
            }
            //message.To.Add("yao.sun@fuyaogroup.com");

            //message.To.Add("zhonghua.yan@fuyaogroup.com");

            //message.To.Add("li.wang@fuyaogroup.com");

            

            //设置抄送人
            message.CC.Add("jun.shen@fuyaogroup.com");
            //设置邮件标题
            message.Subject = "QSB快速反应系统邮件";
            //设置邮件内容
            message.Body = content;
            //设置邮件发送服务器,服务器根据你使用的邮箱而不同,可以到相应的 邮箱管理后台查看,下面是QQ的
            SmtpClient client = new SmtpClient("mail.fuyaogroup.com", 25);
            //设置发送人的邮箱账号和密码
            client.Credentials = new NetworkCredential(emailAcount, emailPassword);
            //启用ssl,也就是安全发送
            client.EnableSsl = true;
            //发送邮件
            //加这段之前用公司邮箱发送报错：根据验证过程，远程证书无效
            //加上后解决问题
            ServicePointManager.ServerCertificateValidationCallback =
delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
            client.Send(message);
            return "0";
        }
    }
}
