using LeaRun.DataAccess;
using LeaRun.Entity;
using LeaRun.Repository;
using LeaRun.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LeaRun.Business
{
    public class FY_RapidBll : RepositoryFactory<FY_Rapid>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson, string MyTask)
        {
            
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from RapidList_New where 1=1 ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
                //parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            if (MyTask == "yes")
            {
                strSql.AppendFormat(" and RapidState!='已完成' and RealName like '{0}' ", ManageProvider.Provider.Current().UserName);
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }


        public DataTable GetDataTable(string sql)
        {
            return Repository().FindDataSetBySql(sql).Tables[0];
        }


        public int Approve(string KeyValue, string field, string state, string isok,string dt,string node)
        {
            StringBuilder strSql = new StringBuilder();
            if (isok == "y")
            {
                if (state == "未提交"||state=="回退"||state== "撤回")
                {
                    if (field == "res_yzb")
                    {
                        strSql.AppendFormat(" update fy_rapid set {0}='待审',RapidState='进行中',{0}dt='{2}',{0}node='{3}' where res_id='{1}'", field, KeyValue, DateTime.Now.ToString(), node);
                    }
                    //else if(field== "res_verifypost")
                    //{
                    //    strSql.AppendFormat(" update fy_rapid set res_postverify='待审',RapidState='进行中',res_postverifydt='{1}',res_postverifynode='{2}' where res_id='{0}'", KeyValue, DateTime.Now.ToString(), node);
                    //}
                    else
                    {
                        string PreField = "";
                        if(field== "res_postverify")
                        {
                            PreField = "res_yzb";
                        }
                        if (field == "res_fx")
                        {
                            PreField = "res_postverify";
                        }
                        if (field == "res_cs")
                        {
                            PreField = "res_fx";
                        }
                        if (field == "res_fcf")
                        {
                            PreField = "res_cs";
                        }
                        if (field == "res_fcsh")
                        {
                            PreField = "res_fcf";
                        }
                        if (field == "res_csgz")
                        {
                            PreField = "res_fcsh";
                        }
                        if (field == "res_fmea")
                        {
                            PreField = "res_csgz";
                        }
                        if (field == "res_bzgx")
                        {
                            PreField = "res_fmea";
                        }
                        if (field == "res_jyjx")
                        {
                            PreField = "res_bzgx";
                        }
                        if(field=="res_8d")
                        {
                            PreField = "res_jyjx";
                        }
                        string TempSql = "select "+PreField+ " from fy_rapid where res_id='"+KeyValue+"'";
                        DataTable TempDt = GetDataTable(TempSql);
                        if (TempDt.Rows.Count > 0)
                        {
                            if (TempDt.Rows[0][0].ToString() == "未提交")
                            {
                                //表示上一个节点没有提交，不能提交当前节点
                                return -3;
                            }
                            else
                            {
                                strSql.AppendFormat(" update fy_rapid set {0}='待审',RapidState='进行中',{0}dt='{2}',{0}node='{3}' where res_id='{1}'", field, KeyValue, DateTime.Now.ToString(), node);
                            }
                        }
                        else
                        {
                            strSql.AppendFormat(" update fy_rapid set {0}='待审',RapidState='进行中',{0}dt='{2}',{0}node='{3}' where res_id='{1}'", field, KeyValue, DateTime.Now.ToString(), node);
                        }
                    }
                }
                if (state == "待审")
                {
                    strSql.AppendFormat(" update fy_rapid set {0}='通过',{0}dt='{2}',{0}node='{3}' where res_id='{1}'", field, KeyValue, dt, node);
                }


            }
            if (isok == "n")
            {
                if (state == "待审")
                {
                    strSql.AppendFormat(" update fy_rapid set {0}='回退',RapidState='回退',{0}dt='{2}',{0}node='{3}' where res_id='{1}'", field, KeyValue, dt, node);
                    //增加计分，每次退回扣一分，再fy_rapidscore表中增加一条-1的记录
                    strSql.AppendFormat(" insert into FY_RapidScore select NEWID(),b.UserId,-1,'方案退回扣分',GETDATE() from FY_Rapid a left join Base_User b on a.res_cpeo=b.Code where res_id='{0}' ", KeyValue);
                    SendEmail(KeyValue, "你提交的QSB方案被退回");
                }
                if (state == "通过")
                {
                    strSql.AppendFormat(" update fy_rapid set {0}='回退',RapidState='回退',{0}dt='{2}',{0}node='{3}' where res_id='{1}'", field, KeyValue, dt, node);
                    SendEmail(KeyValue, "你提交的QSB方案被退回");
                }
            }
            //表示发起人自己撤回
            if(isok== "sn")
            {
                strSql.AppendFormat(" update fy_rapid set {0}='撤回',{0}dt='{2}',{0}node='{3}' where res_id='{1}'", field, KeyValue, dt, node);
            }
            int result = Repository().ExecuteBySql(strSql);
            return result;
        }

        public DataTable GetList()
        {
            string sql = " select code,RealName from Base_User where Enabled=1 ";
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public DataTable GetDepartUserList()
        {
            string sql = " select code,RealName from Base_User where DepartmentId='"+ManageProvider.Provider.Current().DepartmentId+ "' and Enabled=1  ";
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public DataTable GetCustomerList()
        {
            string sql = " select fy_cus_name from FY_CUS ";
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public DataTable GetBigType()
        {
            string sql = " select code,fullname from Base_DataDictionaryDetail where DataDictionaryId='7d093120-e471-4972-b53b-1639d78f3ede'  and ParentId='0' ";
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public DataTable GetDetailType(string BigType)
        {
            string sql = " select code,fullname from Base_DataDictionaryDetail where DataDictionaryId='7d093120-e471-4972-b53b-1639d78f3ede'  ";
            string Condition = "";
            if(BigType!="Undefined"&&BigType!=null&&BigType!="")
            {
                Condition += " and ParentID in (select DataDictionaryDetailId from Base_DataDictionaryDetail where Code='"+BigType+"') ";
                sql = sql + Condition;
            }
            
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public DataTable GetReportJson(string keyword, ref JqGridParam jqgridparam)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from ##list ");
            StringBuilder proc = new StringBuilder();
            proc.AppendFormat(@"RapidMonthlyReport 2017 ");
            
            //Repository().ExecuteBySql(proc);
            //if (!string.IsNullOrEmpty(keyword))
            //{
                
                //parameter.Add(DbFactory.CreateDbParameter("@keyword", 2017));
            //}
            parameter.Add(DbFactory.CreateDbParameter("@Year", 2017));

            return Repository().FindDataSetByProc("RapidMonthlyReport", parameter.ToArray()).Tables[0];
        }




        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="code">用户编码</param>
        /// <returns>1成功，0失败</returns>
        public int SendEmail(string code, string Content)
        {
            string sql = " select Email from base_user where Enabled=1 and code in (select res_cpeo from FY_Rapid where res_id='" + code+"' ) ";
            DataTable dt = GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                try
                {
                    var emailAcount = "shfy_it@fuyaogroup.com";
                    var emailPassword = "Sj1234";
                    var reciver = dt.Rows[0][0].ToString();
                    var content = Content;
                    MailMessage message = new MailMessage();
                    //设置发件人,发件人需要与设置的邮件发送服务器的邮箱一致
                    MailAddress fromAddr = new MailAddress("shfy_it@fuyaogroup.com");
                    message.From = fromAddr;
                    //设置收件人,可添加多个,添加方法与下面的一样
                    message.To.Add(reciver);
                    //设置抄送人
                    message.CC.Add("jun.shen@fuyaogroup.com");
                    //设置邮件标题
                    message.Subject = "QSB快速反应";
                    //设置邮件内容
                    message.Body = content;
                    //设置邮件发送服务器,服务器根据你使用的邮箱而不同,可以到相应的 邮箱管理后台查看,下面是QQ的
                    SmtpClient client = new SmtpClient("mail.fuyaogroup.com", 25);
                    //设置发送人的邮箱账号和密码
                    client.Credentials = new NetworkCredential(emailAcount, emailPassword);
                    //启用ssl,也就是安全发送
                    client.EnableSsl = true;

                    ServicePointManager.ServerCertificateValidationCallback =
   delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
                    //发送邮件
                    client.Send(message);
                    return 1;
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }


        public DataTable GetAttendanceList(string keyword, ref JqGridParam jqgridparam, string ParameterJson, string MyTask)
        {

            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select b.userid,b.RealName,c.AttendanceState,
case when exists (select * from temp_usercode where usercode=b.code) then 1 else 0 end as type from (

select distinct res_cpeo from [dbo].[SendMailList]
union
select distinct usercode from temp_usercode
union
select distinct code from FY_Attendance aa left join Base_User bb on aa.UserID=bb.UserId
where cast(aa.AttendanceDate as date)=cast(GETDATE() as date) 
) as a
left join Base_User b on a.res_cpeo=b.Code 
left join FY_Attendance c on b.UserId=c.UserId and cast(c.AttendanceDate as date)=cast(GETDATE() as date)
where Enabled=1
");
            
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public int ExecuteSql(StringBuilder sql)
        {
            int result = Repository().ExecuteBySql(sql);
            return result;
        }

        public DataTable GetAttendanceReportJson(string keyword, ref JqGridParam jqgridparam,string startdate,string enddate)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            //strSql.Append(@"select * from ##list ");
            //StringBuilder proc = new StringBuilder();
            //proc.AppendFormat(@"RapidMonthlyReport 2017 ");

            //Repository().ExecuteBySql(proc);
            //if (!string.IsNullOrEmpty(keyword))
            //{

            //parameter.Add(DbFactory.CreateDbParameter("@keyword", 2017));
            //}
            parameter.Add(DbFactory.CreateDbParameter("@StartDate", startdate));
            parameter.Add(DbFactory.CreateDbParameter("@EndDate", enddate));

            return Repository().FindDataSetByProc("AttendanceReport", parameter.ToArray()).Tables[0];
        }


        public DataTable GetAttendanceListNew(string keyword, ref JqGridParam jqgridparam, string StartDate,string EndDate)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select b.Code,b.RealName,a.KqCode,a.KqDate from FY_KqoQin a left join Base_User b on a.KqCode=b.Code  ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" and a.kqdate>='"+StartDate+"' and a.kqdate<='"+EndDate+"' ");
                //parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            //if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            //{
            //    strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            //}
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

    }
}
