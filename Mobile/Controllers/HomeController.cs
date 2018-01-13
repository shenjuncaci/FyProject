using LeaRun.Business;
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
    }
}