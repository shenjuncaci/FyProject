﻿using LeaRun.Business;
using LeaRun.Entity;
using LeaRun.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.Data.Common;
using LeaRun.DataAccess;
using LeaRun.Repository;
using System.IO;

namespace Mobile.Controllers
{
    public class HomeController : Controller
    {
        TR_SkillBll SkillBll = new TR_SkillBll();
        Base_UserBll base_userbll = new Base_UserBll();
        Base_ObjectUserRelationBll base_objectuserrelationbll = new Base_ObjectUserRelationBll();
        public ActionResult Index()
        {
            return View();
        }
         
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="Account">账户</param>
        /// <param name="Password">密码</param>
        /// <returns></returns>
        public ActionResult CheckLogin(string Account, string Password, string Token)
        {
            string Msg = "";
            try
            {
                IPScanerHelper objScan = new IPScanerHelper();
                string IPAddress = NetHelper.GetIPAddress();
                objScan.IP = IPAddress;
                objScan.DataPath = Server.MapPath("~/Resource/IPScaner/QQWry.Dat");
                string IPAddressName = objScan.IPLocation();
                string outmsg = "";
                
                //系统管理
                if (Account == ConfigHelper.AppSettings("CurrentUserName"))
                {
                    if (ConfigHelper.AppSettings("CurrentPassword") == Password)
                    {
                        IManageUser imanageuser = new IManageUser();
                        imanageuser.UserId = "System";
                        imanageuser.Account = "System";
                        imanageuser.UserName = "超级管理员";
                        imanageuser.Gender = "男";
                        imanageuser.Code = "System";
                        imanageuser.LogTime = DateTime.Now;
                        imanageuser.CompanyId = "系统";
                        imanageuser.DepartmentId = "系统";
                        imanageuser.IPAddress = IPAddress;
                        imanageuser.IPAddressName = IPAddressName;
                        imanageuser.IsSystem = true;
                        ManageProvider.Provider.AddCurrent(imanageuser);
                        //对在线人数全局变量进行加1处理
                        HttpContext rq = System.Web.HttpContext.Current;
                        rq.Application["OnLineCount"] = (int)rq.Application["OnLineCount"] + 1;
                        Msg = "3";//验证成功
                        Base_SysLogBll.Instance.WriteLog(Account, OperationType.Login, "1", "登陆成功、IP所在城市：" + IPAddressName);
                    }
                    else
                    {
                        return Content("4");
                    }
                }
                else
                {
                    Base_User base_user = base_userbll.UserLogin(Account, Password, out outmsg);
                    switch (outmsg)
                    {
                        case "-1":      //账户不存在
                            Msg = "-1";
                            Base_SysLogBll.Instance.WriteLog(Account, OperationType.Login, "-1", "账户不存在、IP所在城市：" + IPAddressName);
                            break;
                        case "lock":    //账户锁定
                            Msg = "2";
                            Base_SysLogBll.Instance.WriteLog(Account, OperationType.Login, "-1", "账户锁定、IP所在城市：" + IPAddressName);
                            break;
                        case "error":   //密码错误
                            Msg = "4";
                            Base_SysLogBll.Instance.WriteLog(Account, OperationType.Login, "-1", "密码错误、IP所在城市：" + IPAddressName);
                            break;
                        case "succeed": //验证成功
                            IManageUser imanageuser = new IManageUser();
                            imanageuser.UserId = base_user.UserId;
                            imanageuser.Account = base_user.Account;
                            imanageuser.UserName = base_user.RealName;
                            imanageuser.Gender = base_user.Gender;
                            imanageuser.Password = base_user.Password;
                            imanageuser.Code = base_user.Code;
                            imanageuser.Secretkey = base_user.Secretkey;
                            imanageuser.LogTime = DateTime.Now;
                            imanageuser.CompanyId = base_user.CompanyId;
                            imanageuser.DepartmentId = base_user.DepartmentId;
                            imanageuser.ObjectId = base_objectuserrelationbll.GetObjectId(imanageuser.UserId);
                            imanageuser.GroupID = base_objectuserrelationbll.GetGroupID(imanageuser.UserId);
                            imanageuser.IPAddress = IPAddress;
                            imanageuser.IPAddressName = IPAddressName;
                            imanageuser.IsSystem = false;
                            ManageProvider.Provider.AddCurrent(imanageuser);
                            //对在线人数全局变量进行加1处理
                            //HttpContext rq = System.Web.HttpContext.Current;
                            //rq.Application["OnLineCount"] = (int)rq.Application["OnLineCount"] + 1;
                            Msg = "3";//验证成功
                            Base_SysLogBll.Instance.WriteLog(Account, OperationType.Login, "1", "登陆成功、IP所在城市：" + IPAddressName);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Msg = ex.Message;
            }
            return Content(Msg);
        }
        [LoginAuthorize]
        public ActionResult FirstPage()
        {
            
            return View();
        }
        [LoginAuthorize]
        public ActionResult SpecialExam()
        {
            string table = "<ul data-role=\"listview\">";
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" select a.*,b.RealName,c.FullName,(select top 1 Score from TR_Paper where SkillID=a.CustomExamID and FromSource='1' and UserID='{0}' order by paperdate desc) as topscore,
(select top 1 (PaperID) from TR_Paper where SkillID=a.CustomExamID and FromSource=1 order by PaperDate desc)  as paperID
from tr_Customexam a left join Base_User b on a.CreateBy=b.UserId
left join Base_Department c on a.CreateDept=c.DepartmentId where 1=1 and exists (select * from TR_CustomExamUser where userid='{0}' and 
CustomExamid=a.CustomExamID) and not exists 
(select * from TR_Paper where FromSource=1 and UserId='{0}' 
and SkillID=a.CustomExamID and DATEDIFF(DAY,PaperDate,GETDATE())<7)  ", ManageProvider.Provider.Current().UserId);
            DataTable dt = SkillBll.GetDataTable(strSql.ToString());
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    table += "<li id=\""+dt.Rows[i]["CustomExamID"].ToString() + "\" >" + dt.Rows[i]["CustomeExamName"].ToString() + "<div style=\"text-align:right\"><button name=\"" + dt.Rows[i]["CustomExamID"].ToString() + "\" id=\"sq" + i + "\" onclick=\"examinfo(this.name,1)\">申请考试</button><button name=\"" + dt.Rows[i]["CustomExamID"].ToString() + "\" id=\"ks" + i + "\" onclick=\"examinfo(this.name,1)\">开始考试</button></div></li>";
                }

            }
            table += "</ul>";
            ViewData["Content"] = table;
            return View();
        }
        [LoginAuthorize]
        public ActionResult SkillExam()
        {
            string table = "<ul data-role=\"listview\">";
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"select *,
(select top 1(Isnull(Score, 0)) from TR_Paper where SkillID = a.SkillID and UserID = '{0}' order by PaperDate desc) as ExamScore,
(select top 1(PaperID) from TR_Paper where SkillID = a.SkillID and FromSource = 0 order by PaperDate desc) as paperID,
(select max(SkillRequire) from TR_UserPost aa
left join TR_PostDepartmentRelation bb on aa.DepartmentPostID = bb.RelationID 
left join TR_PostDepartmentRelationDetail cc on bb.RelationID = cc.RelationID 
where aa.UserID = '{0}' and cc.SkillID = a.skillid) as SkillRequire 
from TR_Skill a
where SkillID in (select SkillID from TR_PostDepartmentRelationDetail
where RelationID in (select DepartmentPostID from TR_UserPost where UserID = '{0}')) 
and not exists 
(select * from TR_Paper where FromSource=0 and UserId='{0}' 
and SkillID=a.SkillID and DATEDIFF(DAY,PaperDate,GETDATE())<7) 
", ManageProvider.Provider.Current().UserId);
            DataTable dt = SkillBll.GetDataTable(strSql.ToString());
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    table += "<li id=\"" + dt.Rows[i]["SkillID"].ToString() + "\" >" + dt.Rows[i]["SkillName"].ToString() + "<div style=\"text-align:right\"><button name=\""+ dt.Rows[i]["SkillID"].ToString() + "\" id=\"sq" + i + "\" onclick=\"examinfo(this.name,0)\">申请考试</button><button name=\"" + dt.Rows[i]["SkillID"].ToString() + "\" id=\"ks" + i + "\" onclick=\"examinfo(this.name,0)\">开始考试</button></div></li>";
                }

            }
            table += "</ul>";
            ViewData["Content"] = table;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Type">0技能考试，1专项考试</param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public string IsApply(string Type,string ID)
        {
            string sql = "";
            DataTable dt;
            if(Type=="0")
            {
                sql = " select * from TR_ExamApply where Source=0 " +
                    "and IsOK=0 and UserID='"+ManageProvider.Provider.Current().UserId+"' and ExamID='"+ID+"' ";
                dt = SkillBll.GetDataTable(sql);
                if(dt.Rows.Count>0)
                {
                    return "请耐心等待审核";
                }
                else
                {
                    sql = " select * from TR_ExamApply where Source=0 " +
                   "and IsOK=1 and UserID='" + ManageProvider.Provider.Current().UserId + "' and ExamID='" + ID + "' ";
                    dt = SkillBll.GetDataTable(sql);
                    if (dt.Rows.Count > 0)
                    {
                        return "可以参加考试";
                    }
                    else
                    {
                        StringBuilder strSql = new StringBuilder();
                        strSql.AppendFormat(@" insert into tr_examapply(applyid,userid,examid,source,applydate,isok) values(newid(),'{0}','{1}',0,getdate(),0) "
, ManageProvider.Provider.Current().UserId, ID);
                        SkillBll.ExecuteSql(strSql);
                        return "提交申请成功，请耐心等待审核";
                    }
                }
            }
            else
            {
                sql = " select * from TR_ExamApply where Source=1 " +
                    "and IsOK=0 and UserID='" + ManageProvider.Provider.Current().UserId + "' and ExamID='" + ID + "' ";
                dt = SkillBll.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    return "请耐心等待审核";
                }
                else
                {
                    sql = " select * from TR_ExamApply where Source=1 " +
                   "and IsOK=1 and UserID='" + ManageProvider.Provider.Current().UserId + "' and ExamID='" + ID + "' ";
                    dt = SkillBll.GetDataTable(sql);
                    if (dt.Rows.Count > 0)
                    {
                        return "可以参加考试";
                    }
                    else
                    {
                        StringBuilder strSql = new StringBuilder();
                        strSql.AppendFormat(@" insert into tr_examapply(applyid,userid,examid,source,applydate,isok) values(newid(),'{0}','{1}',1,getdate(),0) "
, ManageProvider.Provider.Current().UserId, ID);
                        SkillBll.ExecuteSql(strSql);
                        return "提交申请成功，请耐心等待审核";
                    }
                }
            }
            
        }

        public ActionResult ExamPage(string ID, string Type)
        {
            DataTable dt;
            if (Type == "0")
            {
                string sqlInfo = " select a.SkillName,a.QuestionNum,a.ExamMinutes as ExamTime  from TR_Skill a where SkillID='" + ID + "' ";
                dt = SkillBll.GetDataTable(sqlInfo);
            }
            else
            {
                string sqlInfo = " select *,ExamMinutes as ExamTime from TR_CustomExam  where CustomExamID='" + ID + "' ";
                dt = SkillBll.GetDataTable(sqlInfo);
            }
            ViewData["ExamTime"] = dt.Rows[0]["ExamTime"].ToString();
            return View();
        }

        public ActionResult GetPaper(string KeyValue,string Type)
        {
            if (Type == "0")
            {
                string sqlInfo = " select a.SkillName,a.QuestionNum,a.ExamMinutes from TR_Skill a where SkillID='" + KeyValue + "' ";
                DataTable dt3 = SkillBll.GetDataTable(sqlInfo);

                string QuestionNum = dt3.Rows[0][1].ToString();


                string sql = " select top " + QuestionNum + " * from TR_ChoiceQuestion where SkillID='" + KeyValue + "'  order by newid()  ";
                // string sql2 = " select top "+JudgmentCount +" * from TR_JudgmentQuestion where SkillID in(select SkillID from TR_KnowledgeBase where KnowledgeBaseID='" + KeyValue+ "')  order by newid() ";
                DataTable dt = SkillBll.GetDataTable(sql);
                return Content(dt.ToJson());
            }
            else
            {
                string sqlInfo = " select *,ExamMinutes as ExamTime from TR_CustomExam  where CustomExamID='" + KeyValue + "' ";
                DataTable dt3 = SkillBll.GetDataTable(sqlInfo);

                string ChoiceCount = dt3.Rows[0]["ChoiceQuestion"].ToString();
                string JudgmentCount = dt3.Rows[0]["JudgmentQuestion"].ToString();

                string sql = " select top " + ChoiceCount + " * from TR_CustomExamChoice where CustomExamID ='" + KeyValue + "' order by newid()  ";
                string sql2 = " select top " + JudgmentCount + " * from TR_JudgmentQuestion where SkillID in(select SkillID from TR_CustomExamSkill where CustomExamID='" + KeyValue + "')  order by newid() ";
                DataTable dt = SkillBll.GetDataTable(sql);
                //DataTable dt2 = CustomExamBll.GetDataTable(sql2);

                return Content(dt.ToJson());
            }
        }


        public string SubmitExam(string KeyValue, string DetailForm,string Type)
        {
            //首先判断下是否可以提交
            string sql = " select * from tr_examapply where userid='"+ManageProvider.Provider.Current().UserId+"' and examid='"+KeyValue+"' and isok=1 ";
            DataTable dtIsApply = SkillBll.GetDataTable(sql);
            if(dtIsApply.Rows.Count<1)
            {
                //没有经过人事审批的不能提交考试
                return "-1";
            }
            else
            {
                StringBuilder strSqlUpdateApply = new StringBuilder();
                strSqlUpdateApply.AppendFormat(" update tr_examapply set isok=2 where userid='" + ManageProvider.Provider.Current().UserId + "' and examid='" + KeyValue + "' ");
                SkillBll.ExecuteSql(strSqlUpdateApply);
            }


            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();

            //先建立一个主表
            TR_Paper paper = new TR_Paper();
            paper.SkillID = KeyValue;
            paper.FromSource = Convert.ToInt32(Type); //表示是常规学习考试
            paper.Create();


            database.Insert(paper, isOpenTrans);


            List<TR_PaperDetail> POOrderEntryList = DetailForm.JonsToList<TR_PaperDetail>();
            int index = 1;
            foreach (TR_PaperDetail entry in POOrderEntryList)
            {


                entry.Create();
                entry.PaperID = paper.PaperID;
                database.Insert(entry, isOpenTrans);
                index++;
            }
            database.Commit();

            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"update TR_PaperDetail set TrueAnswer=(select Answer from Questions where QuestionID=TR_PaperDetail.QuestionID) 
where PaperID='{0}'", paper.PaperID);

            strSql.AppendFormat(@"update TR_PaperDetail set Istrue=1 where answer=trueanswer and paperid='{0}' ", paper.PaperID);
            strSql.AppendFormat(@"update TR_Paper set score=(select dbo.CountScore('{0}')) where PaperID='{0}' ", paper.PaperID);

            SkillBll.ExecuteSql(strSql);

            string sql2 = " select score from tr_paper where paperid='" + paper.PaperID + "' ";
            string Score = SkillBll.GetDataTable(sql2).Rows[0][0].ToString();
            Score = "您好！您的本次考试成绩为" + Score;
            return Score;
        }

        public ActionResult ErrorPage()
        {
            return View();
        }


        public ActionResult MenuPage()
        {
            return View();
        }

        public ActionResult AuditPage()
        {
            bool IsLeader = false;  //判断是否车间主任，有车间主任权限可以显示当前日期之前所有未审核完的记录
            bool IsBz = false; //加一个角色的判断，用来判断是否同时显示车间主任和车间班长的审批内容
            IManageUser user = ManageProvider.Provider.Current();
            string[] objectID = user.ObjectId.Split(',');
            for (int i = 0; i < objectID.Length; i++)
            {
                if (objectID[i] == "91c17ca4-0cbf-43fa-829e-3021b055b6c4" || objectID[i] == "54804f22-89a1-4eee-b257-255deaf4face" || objectID[i] == "8431ea77-69c9-484c-a67f-4f3419c0d393")
                {
                    IsLeader = true;
                }
                if (objectID[i] == "f6afd4e4-6fb2-446f-88dd-815ddb91b09d")
                {
                    IsBz = true;
                }
            }

            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            if (IsLeader == false)
            {
                strSql.Append(@"select PlanID,Plandate,PlanContent,b.RealName,a.line from fy_plan a left join Base_User b on a.ResponseByID=b.UserId
             left join Base_GroupUser c on c.GroupUserId=a.groupid
where PlanContent!='' and  IsLeave=0  and a.BackColor!='' and a.BackColor!='grey' and
           a.DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "' and   GETDATE()>=cast(cast(a.plandate as nvarchar(30))+' '+c.StartTime as datetime) and " +
               "GETDATE()<=cast(cast( case when c.duty='白班' then a.Plandate else DATEADD(day,1,a.Plandate) end as nvarchar(30))+' '+c.EndTime as datetime) " +
               " and not exists (select * from FY_PlanDetail where planid=a.planid and ExamResult!='')");


            }
            else
            {
                strSql.Append(@" select PlanID,Plandate,PlanContent,b.RealName,a.line from fy_plan a left join Base_User b on a.ResponseByID=b.UserId 
left join Base_GroupUser c on c.GroupUserId=a.groupid where PlanContent!='' and Plandate=cast(GETDATE() as date) and ResponseByID='" + ManageProvider.Provider.Current().UserId + "' and (a.BackColor='' or a.BackColor='grey' ) and not exists (select * from FY_PlanDetail where planid=a.planid and ExamResult!='') ");
                if (IsBz == true)
                {
                    strSql.Append(@" union select PlanID,Plandate,PlanContent,b.RealName,a.line from fy_plan a left join Base_User b on a.ResponseByID=b.UserId
             left join Base_GroupUser c on c.GroupUserId=a.groupid
where PlanContent!='' and  IsLeave=0  and a.BackColor!='' and a.BackColor!='grey' and
           a.DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "' and   GETDATE()>=cast(cast(a.plandate as nvarchar(30))+' '+c.StartTime as datetime) and " +
               "GETDATE()<=cast(cast( case when c.duty='白班' then a.Plandate else DATEADD(day,1,a.Plandate) end as nvarchar(30))+' '+c.EndTime as datetime) and not exists (select * from FY_PlanDetail where planid=a.planid and ExamResult!='') ");
                }
            }

            string table = "<ul data-role=\"listview\">";

            DataTable dt = SkillBll.GetDataTable(strSql.ToString());
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    table += "<li onclick=\"auditinfo(this.id)\" id=\"" + dt.Rows[i]["PlanID"].ToString() + "\" >" + dt.Rows[i]["PlanContent"].ToString()+ "(产线："+ dt.Rows[i]["line"].ToString() + ")" + "</li>";
                }

            }
            table += "</ul>";
            ViewData["Content"] = table;

            return View();
        }

        public ActionResult AuditDetailPage(string ID)
        {
            string deptID = "";
            string sqlGetDept = " select * from fy_plan where PlanID='" + ID + "' ";
            DataTable dtGetDept = SkillBll.GetDataTable(sqlGetDept);
            if (dtGetDept.Rows.Count > 0)
            {
                deptID = dtGetDept.Rows[0]["DepartmentID"].ToString();
            }

            string IsLeader = "";
            string SqlIsLeader = @"select * from Base_ObjectUserRelation where UserId='" + ManageProvider.Provider.Current().UserId + "' and ObjectId in ('8431ea77-69c9-484c-a67f-4f3419c0d393', '91c17ca4-0cbf-43fa-829e-3021b055b6c4','54804f22-89a1-4eee-b257-255deaf4face')";
            DataTable dtIsLeader = SkillBll.GetDataTable(SqlIsLeader);
            if (dtIsLeader.Rows.Count > 0)
            {
                //如果有车间主任、厂长、副总的权限，除了通用还要加上系统监督的审批内容
                IsLeader = "系统监督";
            }

            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("insert into fy_plandetail  select NEWID(),'{0}',processid,'','{1}',ProcessName,AbleProcess,AuditContent,FailureEffect,ReactionPlan from FY_Process where processid not in " +
                "(select processid from fy_plandetail where planid='{0}') and (ProcessName in (select PlanContent from FY_Plan where PlanID='{0}' ) " +
                "or (processname in ('通用','{3}') and DepartmentID='cb016a86-d835-4eb9-ad80-6a86d2140c88') )  and (DepartmentID='{2}' or DepartmentID='cb016a86-d835-4eb9-ad80-6a86d2140c88') ",
                ID, ManageProvider.Provider.Current().UserId, deptID, IsLeader);
            SkillBll.ExecuteSql(strSql);

            //获取数据展示到页面
            string sql = @" select a.plandid,a.ProcessName,a.AbleProcess,a.AuditContent,a.FailureEffect,a.ReactionPlan,a.examresult 
from fy_plandetail a left join FY_Process b on a.processid=b.ProcessID where a.planid='" + ID + "' order by a.ProcessName desc ";
            DataTable dt = SkillBll.GetDataTable(sql);
            ViewData["dt"] = dt;
            return View();
        }

        public ActionResult ProblemResponse(string id)
        {
            return View();
        }

        public ActionResult ResponseAllJson()
        {
            FY_PlanBll planbll = new FY_PlanBll();
            DataTable ListData = planbll.GetResponseAllByList();
            return Content(ListData.ToJson());
        }

        public int InsertAction(string problem, string action1, string response, string plandate)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("insert into FY_ProblemAction values (NEWID(),'" + problem + "','" + action1 + "','" + response + "','" + plandate + "','" + ManageProvider.Provider.Current().UserId + "','" + ManageProvider.Provider.Current().DepartmentId + "')");
            int result = SkillBll.ExecuteSql(strSql);

            //string GetReciverSql = " select Email from base_user where userid='" + response + "' ";
            //DataTable dt = SkillBll.GetDataTable(GetReciverSql);
            //if (dt.Rows.Count > 0)
            //{
            //    MailHelper.SendEmail(dt.Rows[0][0].ToString(), "您好，您的分层审核有一项不合格，请注意登录系统查看");
            //}

            return result;
        }

        public ActionResult DormPage()
        {
            return View();
        }

        public ActionResult InertRepair()
        {
            return View();
        }

        public ActionResult DormList()
        {
            string sql = " select DormID,DormName from dm_dorm ";
            DataTable dt = SkillBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public string InsertRepairAjax(string Dorm,string RoomNO,string PhoneNumber,string Descripe,string Picture,string Remark)
        {

            string virtualPath = "";
            //图片上传
            if (Picture != "")
            {
                string fileGuid = CommonHelper.GetGuid;
                //long filesize = Filedata.ContentLength;
                string FileEextension = ".jpg";
                string uploadDate = DateTime.Now.ToString("yyyyMMdd");
                //string UserId = ManageProvider.Provider.Current().UserId;
                 virtualPath = string.Format("~/Resource/Document/NetworkDisk/{0}/{1}{2}", "RoomRepair", fileGuid, FileEextension);

                string realPath = string.Format(@"D:\LeaRun\Resource\Document\NetworkDisk\{0}\{1}{2}", "RoomRepair", fileGuid, FileEextension);

                //string fullFileName = this.Server.MapPath(virtualPath);
                ////创建文件夹，保存文件
                //realPath = Path.GetDirectoryName(fullFileName);
                //先处理图片文件
                string temp = Picture.Substring(23);
                byte[] arr2 = Convert.FromBase64String(Picture.Substring(23));
                using (MemoryStream ms2 = new MemoryStream(arr2))
                {
                    System.Drawing.Bitmap bmp2 = new System.Drawing.Bitmap(ms2);





                    bmp2.Save(realPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                    bmp2.Dispose();
                    ms2.Close();
                }
            }


            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();

            

            DM_RoomRepair entity = new DM_RoomRepair();
            entity.Create();
            entity.DormID = Dorm;
            entity.RoomNO = RoomNO;
            entity.MobilePhone = PhoneNumber;
            entity.RepairDescripe = Descripe;
            entity.RepairDate = DateTime.Now;
            entity.UserCode = ManageProvider.Provider.Current().Code;
            entity.UserName = ManageProvider.Provider.Current().UserName;
            entity.Remark = Remark;
            database.Insert(entity, isOpenTrans);

            if (Picture != "")
            {
                DM_RoomRepairPicture entityD = new DM_RoomRepairPicture();
                entityD.Create();
                entityD.PictureUrl = virtualPath;
                entityD.RoomRepairID = entity.RoomRepairID;

                database.Insert(entityD, isOpenTrans);
            }


            database.Commit();
            return "1";
        }

        public ActionResult ProcessRepairIndex()
        {
            string sql = @" select top 10 RepairDescripe+' '+'('+CONVERT(varchar(100),RepairDate, 23)+')',roomrepairid from DM_RoomRepair a
where a.RepairState='维修中' and a.createby='" + ManageProvider.Provider.Current().UserName+ "' order by RepairDate desc ";
            DataTable dt = SkillBll.GetDataTable(sql);

            string table = "<ol data-role=\"listview\">";
            
            if (dt.Rows.Count>0)
            {
                for(int i=0;i<dt.Rows.Count;i++)
                {
                    table += " <li onClick=\"showPicture('"+dt.Rows[i][1]+"')\">"+dt.Rows[i][0]+"</li> ";
                }
            }
            else
            {
                table = "没有数据";
            }
            table += "</ol>";
            ViewData["data"] = table;
            return View();
        }

        public ActionResult CompleteRepairIndex()
        {
            string sql = @" select top 10 RepairDescripe+' '+'('+CONVERT(varchar(100),RepairDate, 23)+')' from DM_RoomRepair a
where a.RepairState='已完成' and a.createby='" + ManageProvider.Provider.Current().UserName + "' order by RepairDate desc ";
            DataTable dt = SkillBll.GetDataTable(sql);

            string table = "<ol data-role=\"listview\">";

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    table += " <li>" + dt.Rows[i][0] + "</li> ";
                }
            }
            else
            {
                table = "没有数据";
            }
            table += "</ol>";
            ViewData["data"] = table;
            return View();
        }

        public ActionResult RepairDescripeList()
        {
            string sql = @"select a.Code,a.FullName from Base_DataDictionaryDetail a 
left join Base_DataDictionary b on a.DataDictionaryId=b.DataDictionaryId
where b.FullName='故障类型' order by a.SortCode ";
            DataTable dt = SkillBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult showPicture(string ID)
        {
            string sql = " select * from DM_RoomRepairPicture where RoomRepairID='"+ID+"'  ";
            return View();
        }

        public ActionResult test(HttpPostedFileBase Filedata)
        {
            HttpPostedFileBase file = Request.Files["file"];
            return View();
        }

        //积分查看
        public ActionResult ViewIntegral()
        {
            string Content = @"<div data-role='fieldcontain'>
                <label for='AgeOfWorklabel'>司龄：</label>
                <input readonly type='text' name='AgeOfWork' id='AgeOfWork' value='{0}'>
                <label for='IsLeaderlabel'>班组长：</label>
                <input readonly type='text' name='IsLeader' id='IsLeader' value='{1}'>
                <label for='Degreelabel'>学历：</label>
                <input readonly type='text' name='Degree' id='Degree' value='{2}'>
                <label for='SkillUplabel'>技能提升：</label>
                <input readonly type='text' name='SkillUp' id='SkillUp' value='{3}'>
                <label for='YellowBandlabel'>精益黄带人员：</label>
                <input readonly type='text' name='YellowBand' id='YellowBand' value='{4}'>
                <label for='CourseProjectlabel'>课题项目：</label>
                <input readonly type='text' name='CourseProject' id='CourseProject' value='{5}'>
                <label for='ImproveProposallabel'>改善提案：</label>
                <input readonly type='text' name='ImproveProposal' id='ImproveProposal' value='{6}'>
                <label for='RationalProposallabel'>合理化建议：</label>
                <input readonly type='text' name='RationalProposal' id='RationalProposal' value='{7}'>
                <label for='FiveStar1label'>五星班组-五大员：</label>
                <input readonly type='text' name='FiveStar1' id='FiveStar1' value='{8}'>
                <label for='FiveStar2label'>五星班组-星级班组：</label>
                <input readonly type='text' name='FiveStar2' id='FiveStar2' value='{9}'>
                <label for='ExperienceYearlabel'>经验传承-师带徒：</label>
                <input readonly type='text' name='ExperienceYear' id='ExperienceYear' value='{10}'>
                <label for='ExperienceSkilllabel'>经验传承-技能教练：</label>
                <input readonly type='text' name='ExperienceSkill' id='ExperienceSkill' value='{11}'>
                <label for='ExperienceProlabel'>经验传承-专业传承：</label>
                <input readonly type='text' name='ExperiencePro' id='ExperiencePro' value='{12}'>
                <label for='ExperineceCollegelabel'>文化贡献-社团建设：</label>
                <input readonly type='text' name='ExperineceCollege' id='ExperineceCollege' value='{13}'>
                <label for='ExperienceWriterlabel'>文化贡献-通讯稿：</label>
                <input readonly type='text' name='ExperienceWriter' id='ExperienceWriter' value='{14}'>
                <label for='Honourlabel'>荣誉：</label>
                <input readonly type='text' name='Honour' id='Honour' value='{15}'>
                <label for='Minus1label'>违纪扣分：</label>
                <input readonly type='text' name='Minus1' id='Minus1' value='{16}'>
                <label for='Minus2label'>安全扣分：</label>
                <input readonly type='text' name='Minus2' id='Minus2' value='{17}'>
                <label for='Minus2label'>总分：</label>
                <input readonly type='text' name='SumAll' id='SumAll' value='{18}'>
            </div>";


            //给内容赋值
            string sql = " select * from base_user where userid='"+ManageProvider.Provider.Current().UserId+"' ";
            DataTable dt = SkillBll.GetDataTable(sql);
            Base_User UserModel = DtConvertHelper.ConvertToModel<Base_User>(dt, 0);

            Content = string.Format(Content, UserModel.AgeOfWork, UserModel.IsLeader, UserModel.Degree, UserModel.SkillUp, UserModel.YellowBand, UserModel.CourseProject, UserModel.ImproveProposal,
                UserModel.RationalProposal, UserModel.FiveStar1, UserModel.FiveStar2, UserModel.ExperienceYear, UserModel.ExperienceSkill, UserModel.ExperiencePro, UserModel.ExperineceCollege, UserModel.ExperienceWriter,
                UserModel.Honour, UserModel.Minus1, UserModel.Minus2, UserModel.AgeOfWork + UserModel.IsLeader + UserModel.Degree + UserModel.SkillUp + UserModel.YellowBand + UserModel.CourseProject + UserModel.ImproveProposal +
                UserModel.RationalProposal + UserModel.FiveStar1 + UserModel.FiveStar2 + UserModel.ExperienceYear + UserModel.ExperienceSkill + UserModel.ExperiencePro + UserModel.ExperineceCollege + UserModel.ExperienceWriter +
                UserModel.Honour - UserModel.Minus1 - UserModel.Minus2);

            ViewData["Content"] = Content;
            return View();
        }

        public ActionResult Qiandao()
        {
            return View();
        }
    }
}