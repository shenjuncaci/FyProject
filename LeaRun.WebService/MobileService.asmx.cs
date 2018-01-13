using LeaRun.Business;
using LeaRun.DataAccess;
using LeaRun.Entity;
using LeaRun.Repository;
using LeaRun.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using System.Data;

namespace LeaRun.WebService
{
    /// <summary>
    /// MobileService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    [System.Web.Script.Services.ScriptService]
    public class MobileService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }
        [WebMethod(EnableSession = true, Description = "移动端登录时调用")]
        public string CheckLogin(string Account, string Password, string Token)
        {
            //首先进行一次md5的加密
            //string cl = Password;
            ////string pwd = "";
            //MD5 md5 = MD5.Create(); //实例化一个md5对像
            //                        // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            //byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            Password = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(Password, "MD5").ToLower();

            Base_UserBll base_userbll = new Base_UserBll();
            Base_ObjectUserRelationBll base_objectuserrelationbll = new Base_ObjectUserRelationBll();
            string Msg = "";
            try
            {
                //IPScanerHelper objScan = new IPScanerHelper();
                //string IPAddress = NetHelper.GetIPAddress();
                //objScan.IP = IPAddress;
                ////objScan.DataPath = Server.MapPath("~/Resource/IPScaner/QQWry.Dat");
                //string IPAddressName = objScan.IPLocation();
                string outmsg = "";
                //VerifyIPAddress(Account, IPAddress, IPAddressName, Token);
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
                        //imanageuser.IPAddress = IPAddress;
                        //imanageuser.IPAddressName = IPAddressName;
                        imanageuser.IsSystem = true;
                        ManageProvider.Provider.AddCurrent(imanageuser);
                        //对在线人数全局变量进行加1处理
                        HttpContext rq = System.Web.HttpContext.Current;
                        rq.Application["OnLineCount"] = (int)rq.Application["OnLineCount"] + 1;
                        Msg = "3";//验证成功
                        Base_SysLogBll.Instance.WriteLog(Account, OperationType.Login, "1", "登陆成功、IP所在城市：");
                    }
                    else
                    {
                        return "4";
                    }
                }
                else
                {
                    Base_User base_user = base_userbll.UserLogin(Account, Password, out outmsg);
                    switch (outmsg)
                    {
                        case "-1":      //账户不存在
                            Msg = "-1";
                            //Base_SysLogBll.Instance.WriteLog(Account, OperationType.Login, "-1", "账户不存在、IP所在城市：" );
                            break;
                        case "lock":    //账户锁定
                            Msg = "2";
                            //Base_SysLogBll.Instance.WriteLog(Account, OperationType.Login, "-1", "账户锁定、IP所在城市：" );
                            break;
                        case "error":   //密码错误
                            Msg = "4";
                            //Base_SysLogBll.Instance.WriteLog(Account, OperationType.Login, "-1", "密码错误、IP所在城市：" );
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
                            //imanageuser.IPAddress = IPAddress;
                            //imanageuser.IPAddressName = IPAddressName;
                            imanageuser.IsSystem = false;
                            //ManageProvider.Provider.AddCurrent(imanageuser);
                            ////对在线人数全局变量进行加1处理
                            //HttpContext rq = System.Web.HttpContext.Current;
                            //rq.Application["OnLineCount"] = (int)rq.Application["OnLineCount"] + 1;
                            Msg = "3";//验证成功
                            //Base_SysLogBll.Instance.WriteLog(Account, OperationType.Login, "1", "登陆成功、IP所在城市：" );
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
            return Msg;
        }


        public void VerifyIPAddress(string Account, string IPAddress, string IPAddressName, string OpenId)
        {
            if (ConfigHelper.AppSettings("VerifyIPAddress") == "true")
            {
                List<DbParameter> parameter = new List<DbParameter>();
                parameter.Add(DbFactory.CreateDbParameter("@IPAddress", IPAddress));
                parameter.Add(DbFactory.CreateDbParameter("@IPAddressName", IPAddressName));
                parameter.Add(DbFactory.CreateDbParameter("@OpenId", DESEncrypt.Decrypt(OpenId)));
                int IsOk = DataFactory.Database().ExecuteByProc("[Login].dbo.[PROC_verify_IPAddress]", parameter.ToArray());
            }
        }
        [WebMethod(EnableSession = true, Description = "测试json数据格式")]
        public void JsonTest(string username, string password)
        {
            User u = new User();
            u.name = "demo";
            u.username = username;
            u.password = password;
            u.money = 1.00;
            string json = JsonConvert.SerializeObject(u);
            Context.Response.Write(json);
            Context.Response.End();
        }

        public class User
        {
            public string name { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public double money { get; set; }
        }

        [WebMethod(EnableSession = true, Description = "分层审核待审列表数据")]
        public void SAuditList(string username)
        {
            FY_PlanBll planbll = new FY_PlanBll();
            string sqlGetDepart = " select DepartmentId from base_user where Account='" + username+"' ";
            DataTable dt = planbll.GetDataTable(sqlGetDepart);
            string departmentid = dt.Rows[0][0].ToString();

            string sql = " select * from FY_Plan where cast(Plandate as date)=cast(GETDATE() as date) and DepartmentID='"+ departmentid + "' ";
            DataTable dtPlan = planbll.GetDataTable(sql);

            string json = dtPlan.ToJson();
            Context.Response.Write(json);
            Context.Response.End();
        }

        [WebMethod(EnableSession = true, Description = "获取考试列表")]
        public void ExamList(string username)
        {
            FY_PlanBll planbll = new FY_PlanBll();
            string sqlGetDepart = " select userid from base_user where Account='" + username + "' ";
            DataTable dt = planbll.GetDataTable(sqlGetDepart);
            string userid = dt.Rows[0][0].ToString();


            string sql = "select a.CustomExamID,a.CustomeExamName from tr_Customexam a left join Base_User b on a.CreateBy=b.UserId " +
"left join Base_Department c on a.CreateDept=c.DepartmentId where 1=1 and exists (select * from TR_CustomExamUser where userid='"+ userid + "' and  " +
" CustomExamid=a.CustomExamID) and  GETDATE()>=a.StartTime and GETDATE()<=a.EndTime ";
            //限制考试一次，正式使用的时候取消注释
            //sql = sql + " and not exists (select * from TR_Paper where KnowledgeBaseID=a.CustomExamID and FromSource=1) ";
            DataTable ResultDt = planbll.GetDataTable(sql);

            string json = ResultDt.ToJson();
            Context.Response.Write(json);
            Context.Response.End();
        }

        [WebMethod(EnableSession = true, Description = "获取选择题明细")]
        public void ExamInfo(string ExamID)
        {
            TR_CustomExamBll CustomExamBll = new TR_CustomExamBll();
            string sqlInfo = " select *,DATEDIFF(MINUTE,StartTime,EndTime) as ExamTime from TR_CustomExam  where CustomExamID='" + ExamID + "' ";
            DataTable dt3 = CustomExamBll.GetDataTable(sqlInfo);

            string ChoiceCount = dt3.Rows[0]["ChoiceQuestion"].ToString();
            string JudgmentCount = dt3.Rows[0]["JudgmentQuestion"].ToString();

            string sql = " select top " + ChoiceCount + " * from TR_ChoiceQuestion where SkillID in(select SkillID from TR_CustomExamSkill where CustomExamID='" + ExamID + "')  order by newid()  ";
            string sql2 = " select top " + JudgmentCount + " * from TR_JudgmentQuestion where SkillID in(select SkillID from TR_CustomExamSkill where CustomExamID='" + ExamID + "')  order by newid() ";
            DataTable dt = CustomExamBll.GetDataTable(sql);
            //DataTable dt2 = CustomExamBll.GetDataTable(sql2);


            string json = dt.ToJson();
            Context.Response.Write(json);
            Context.Response.End();
        }

        [WebMethod(EnableSession = true, Description = "获取判断题明细")]
        public void ExamInfo2(string ExamID)
        {
            TR_CustomExamBll CustomExamBll = new TR_CustomExamBll();
            string sqlInfo = " select *,DATEDIFF(MINUTE,StartTime,EndTime) as ExamTime from TR_CustomExam  where CustomExamID='" + ExamID + "' ";
            DataTable dt3 = CustomExamBll.GetDataTable(sqlInfo);

            string ChoiceCount = dt3.Rows[0]["ChoiceQuestion"].ToString();
            string JudgmentCount = dt3.Rows[0]["JudgmentQuestion"].ToString();

            string sql = " select top " + ChoiceCount + " * from TR_ChoiceQuestion where SkillID in(select SkillID from TR_CustomExamSkill where CustomExamID='" + ExamID + "')  order by newid()  ";
            string sql2 = " select top " + JudgmentCount + " * from TR_JudgmentQuestion where SkillID in(select SkillID from TR_CustomExamSkill where CustomExamID='" + ExamID + "')  order by newid() ";
            //DataTable dt = CustomExamBll.GetDataTable(sql);
            DataTable dt2 = CustomExamBll.GetDataTable(sql2);


            string json = dt2.ToJson();
            Context.Response.Write(json);
            Context.Response.End();
        }

        [WebMethod(EnableSession = true, Description = "提交考试的结果，写入数据库，返回分数")]
        public string ExamSubmit(string ExamID,string Detail,string UserName)
        {
            try
            {

                //获取用户ID
                string UserID = "";

                string sqlGetUserID = " select UserId from base_user where account='" + UserName+"' ";
                
                TR_CustomExamBll CustomExamBll = new TR_CustomExamBll();
                DataTable dtGetUserID = CustomExamBll.GetDataTable(sqlGetUserID);
                if(dtGetUserID.Rows.Count>0)
                {
                    UserID = dtGetUserID.Rows[0][0].ToString();
                }
                IDatabase database = DataFactory.Database();
                DbTransaction isOpenTrans = database.BeginTrans();

                //先建立一个主表
                TR_Paper paper = new TR_Paper();
                paper.KnowledgeBaseID = ExamID;
                paper.FromSource = 1; //表示是自定义的学习考试
                paper.UserID = UserID;
                paper.MobileCreate();


                database.Insert(paper, isOpenTrans);
                List<TR_PaperDetail> POOrderEntryList = Detail.JonsToList<TR_PaperDetail>();
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

                CustomExamBll.ExecuteSql(strSql);

                string sql = " select score from tr_paper where paperid='" + paper.PaperID + "' ";
                string Score = CustomExamBll.GetDataTable(sql).Rows[0][0].ToString();
                Score = "您好！您的本次考试成绩为" + Score;
                return Score;
            }
            catch
            {
                return "error";
            }
        }
    }
}
