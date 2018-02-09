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


//*----------Dragon be here!----------/

//* 　　　┏┓　　　┏┓

//* 　　┏┛┻━━━┛┻┓

//* 　　┃　　　　　　　┃

//* 　　┃　　　━　　　┃

//* 　　┃　┳┛　┗┳　┃

//* 　　┃　　　　　　　┃

//* 　　┃　　　┻　　　┃

//* 　　┃　　　　　　　┃

//* 　　┗━┓　　　┏━┛

//* 　　　　┃　　　┃神兽保佑

//* 　　　　┃　　　┃代码无BUG！

//* 　　　　┃　　　┗━━━┓

//* 　　　　┃　　　　　　　┣┓

//* 　　　　┃　　　　　　　┏┛

//* 　　　　┗┓┓┏━┳┓┏┛

//* 　　　　　┃┫┫　┃┫┫

//* 　　　　　┗┻┛　┗┻┛

//* ━━━━━━神兽出没━━━━━━

namespace LeaRun.WebApp.Areas.VPModule.Controllers
{
    public class VerifyPostController : Controller
    {
        RepositoryFactory<VP_VerifyPost> repositoryfactory = new RepositoryFactory<VP_VerifyPost>();
        VP_VerifyPostBll VerifyPostBll = new VP_VerifyPostBll();
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
                DataTable ListData = VerifyPostBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
        public ActionResult SubmitForm(string KeyValue, VP_VerifyPost entity, string BuildFormJson, HttpPostedFileBase Filedata)
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

                    entity.Create();

                    //新建的时候网明细表里插入数据,数据未发生日期+1天+周期数的条数
                    DateTime mindt = Convert.ToDateTime(entity.StartDate).AddDays(1);
                    DateTime maxdt = mindt.AddDays(entity.VerifyCycle);
                    while(mindt<maxdt)
                    {
                        VP_VerifyPostDetail EntityD = new VP_VerifyPostDetail();
                        EntityD.Create();
                        EntityD.VerifyDate = mindt;
                        EntityD.VerifyPostID = entity.VerifyPostID;

                        database.Insert(EntityD, isOpenTrans);

                        mindt=mindt.AddDays(1);


                    }

                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.VerifyPostID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.VerifyPostID, ModuleId, isOpenTrans);
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
            VP_VerifyPost entity = DataFactory.Database().FindEntity<VP_VerifyPost>(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<TR_Post>(array, IsOk.ToString(), Message);
        }

        public ActionResult DepartJson()
        {
            string sql = " select DepartmentId,FullName from Base_Department where Nature='生产' ";
            DataTable dt = VerifyPostBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult VerifyPostDetailList()
        {
            return View();
        }

        public ActionResult GridPageDetailListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = VerifyPostBll.GetDetailPageList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult InputDetail(string id)
        {
            return View();
        }

        public string UpdateDetail(string verifypostdid,string timespan,string tag,string defectNum,string checkNum)
        {
            string temp = "0";
            if(timespan=="00:00-04:00")
            {
                temp = "1";
            }
            else if(timespan == "04:00-08:00")
            {
                temp = "2";
            }
            else if(timespan=="08:00-12:00")
            {
                temp = "3";
            }
            else if (timespan == "12:00-16:00")
            {
                temp = "4";
            }
            else if (timespan == "16:00-20:00")
            {
                temp = "5";
            }
            else if (timespan == "20:00-24:00")
            {
                temp = "6";
            }
            else
            {
                temp = "";
            }
            StringBuilder strSql = new StringBuilder();
            if (tag == "未生产")
            {
                strSql.AppendFormat(@" update VP_VerifyPostDetail set Status{1}='未生产',DefectNum{1}=0,CheckNum{1}=0 where VerifyPostDID='{0}' "
    , verifypostdid,temp);
                if(temp=="6")
                {
                    //当天最后一次，并且失效数量是0的话，检验下是否连续的无失效天数大于等于周期
                    string sql = @"select isnull(dbo.[GetContinuousDayByVeryPostDID]('{0}'),0)";
                    sql = string.Format(sql, verifypostdid);

                    DataTable dt = VerifyPostBll.GetDataTable(sql);
                    int MaxRq = 0;
                    if (dt.Rows.Count > 0)
                    {
                        MaxRq = Convert.ToInt32(dt.Rows[0][0].ToString());
                    }

                    //获取周期
                    sql = @"select VerifyCycle from VP_VerifyPost where 
VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}')";
                    sql = string.Format(sql, verifypostdid);
                    dt = VerifyPostBll.GetDataTable(sql);
                    int Cycle = Convert.ToInt32(dt.Rows[0][0].ToString());

                    //连续天数大于周期，变成完成
                    if (MaxRq >= Cycle)
                    {
                        StringBuilder sqlFinish = new StringBuilder();
                        sqlFinish.AppendFormat(@" update VP_VerifyPost set Status='已完成',RealQuitDate=getdate()
where VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}') ", verifypostdid);
                        VerifyPostBll.ExecuteSql(sqlFinish);
                    }
                }
                
            }
            else
            {
                int OneLevelAlarm = 0;
                int TwoLevelAlarm = 0;
                int ThreeLevelAlarm = 0;
                int DefectNumAll = 0;
                strSql.AppendFormat(@" update VP_VerifyPostDetail set DefectNum{3}='{1}',CheckNum{3}='{2}',Status{3}='' 
where VerifyPostDID='{0}' ", verifypostdid,defectNum,checkNum,temp);
                VerifyPostBll.ExecuteSql(strSql);
                //当失效数量不是0的时候，重新更新明细表中的数据，在当前日期的后面再增加周期数量的数据
                if (defectNum!="0")
                {
                    string sqlIsToday = " select VerifyDate from VP_VerifyPostDetail where VerifyPostDID='{0}' ";
                    sqlIsToday = string.Format(sqlIsToday, verifypostdid);
                    DataTable dtIsToday = VerifyPostBll.GetDataTable(sqlIsToday);
                    if (Convert.ToDateTime(dtIsToday.Rows[0][0].ToString()) ==Convert.ToDateTime(DateTime.Now))
                    {
                        #region 编辑的数据是今天的数据的情况
                        //删除今天以后的数据，再按照周期数重新添加
                        StringBuilder sqlDelete = new StringBuilder();
                        sqlDelete.AppendFormat(@" delete from VP_VerifyPostDetail where VerifyDate>cast(GETDATE() as date) 
and VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}') ", verifypostdid);
                        VerifyPostBll.ExecuteSql(sqlDelete);
                        //根据周期重新添加数据
                        IDatabase database = DataFactory.Database();
                        DbTransaction isOpenTrans = database.BeginTrans();
                        string sqlGetinfo = @" select a.*,cast(dbo.[GetAllDefectNum]('{0}') as int) as DefectNumAll
from VP_VerifyPost a left join VP_VerifyPostDetail b on a.VerifyPostID=b.VerifyPostID 
where a.VerifyPostID in 
(select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}') ";
                        sqlGetinfo = string.Format(sqlGetinfo, verifypostdid);
                        DataTable dtGetInfo = VerifyPostBll.GetDataTable(sqlGetinfo);
                        int C1 = Convert.ToInt32(dtGetInfo.Rows[0]["VerifyCycle"].ToString());
                        string VerifyPostID = dtGetInfo.Rows[0]["VerifyPostID"].ToString();
                        OneLevelAlarm = Convert.ToInt32(dtGetInfo.Rows[0]["OneLevelAlarm"].ToString());
                        TwoLevelAlarm = Convert.ToInt32(dtGetInfo.Rows[0]["TwoLevelAlarm"].ToString());
                        ThreeLevelAlarm = Convert.ToInt32(dtGetInfo.Rows[0]["ThreeLevelAlarm"].ToString());
                        DefectNumAll= Convert.ToInt32(dtGetInfo.Rows[0]["DefectNumAll"].ToString());

                        try
                        {
                            DateTime mindt = Convert.ToDateTime(DateTime.Now).AddDays(1);
                            DateTime maxdt = mindt.AddDays(C1);
                            while (mindt < maxdt)
                            {
                                VP_VerifyPostDetail EntityD = new VP_VerifyPostDetail();
                                EntityD.Create();
                                EntityD.VerifyDate = mindt;
                                EntityD.VerifyPostID = VerifyPostID;

                                database.Insert(EntityD, isOpenTrans);

                                mindt = mindt.AddDays(1);


                            }

                            database.Commit();
                        }
                        catch
                        {
                            database.Rollback();
                            database.Close();
                        }
                        #endregion
                    }
                    //不是当天的数据
                    else
                    {
                        string sqlCount = @" select a.*,b.VerifyCycle,b.OneLevelAlarm,b.TwoLevelAlarm,b.ThreeLevelAlarm,cast(dbo.[GetAllDefectNum]('{0}') as int) as DefectNumAll
from VP_VerifyPostDetail a left join VP_VerifyPost b on a.VerifyPostID=b.VerifyPostID 
where a.VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}') and cast(a.VerifyDate as date)>=cast('{1}' as date) 
order by VerifyDate desc ";
                        sqlCount = string.Format(sqlCount, verifypostdid, dtIsToday.Rows[0][0].ToString());
                        DataTable dtCount = VerifyPostBll.GetDataTable(sqlCount);
                        int C1 = Convert.ToInt32(dtCount.Rows[0]["VerifyCycle"].ToString());
                        string VerifyPostID = dtCount.Rows[0]["VerifyPostID"].ToString();
                        OneLevelAlarm = Convert.ToInt32(dtCount.Rows[0]["OneLevelAlarm"].ToString());
                        TwoLevelAlarm = Convert.ToInt32(dtCount.Rows[0]["TwoLevelAlarm"].ToString());
                        ThreeLevelAlarm = Convert.ToInt32(dtCount.Rows[0]["ThreeLevelAlarm"].ToString());
                        DefectNumAll= Convert.ToInt32(dtCount.Rows[0]["DefectNumAll"].ToString());
                        if (C1 - dtCount.Rows.Count+1 > 0)
                        {
                            IDatabase database = DataFactory.Database();
                            DbTransaction isOpenTrans = database.BeginTrans();
                            try
                            {
                                DateTime mindt = Convert.ToDateTime(dtCount.Rows[0]["VerifyDate"].ToString()).AddDays(1);
                                DateTime maxdt = mindt.AddDays(C1 - dtCount.Rows.Count+1);
                                while (mindt < maxdt)
                                {
                                    VP_VerifyPostDetail EntityD = new VP_VerifyPostDetail();
                                    EntityD.Create();
                                    EntityD.VerifyDate = mindt;
                                    EntityD.VerifyPostID = VerifyPostID;

                                    database.Insert(EntityD, isOpenTrans);

                                    mindt = mindt.AddDays(1);


                                }

                                database.Commit();
                            }
                            catch
                            {
                                database.Rollback();
                                database.Close();
                            }
                        }

                    }
                    //此处预计加入，根据报警设置数量来发邮件预警和加入问题到快速反应以及一般问题跟踪
                    //获取累计的不良数
                    //获取基础数据，首先判断大于三级--依次降级
                    

                    if(DefectNumAll>=ThreeLevelAlarm)
                    {
                        //大于三级警报，将问题加入到快反，并发送邮件给总经理、生产副总、质量副总、质量经理、厂长、车间主任、班长
                        //获取邮件名单sql
                        string sqlGetEmail = @" select Email,a.code from Base_User  a
where exists( select * from Base_ObjectUserRelation where UserId=a.UserId and
ObjectId in ('15f14d9c-e74c-46ac-8641-b3c1bac26940','8431ea77-69c9-484c-a67f-4f3419c0d393') )
or ( 
exists ( select * from Base_ObjectUserRelation where UserId=a.UserId and
ObjectId in ('91c17ca4-0cbf-43fa-829e-3021b055b6c4','f6afd4e4-6fb2-446f-88dd-815ddb91b09d') )
and (DepartmentId='{0}' or DepartmentId in (select ParentId from Base_Department where DepartmentId='{0}' ) )
)
 ";
                        sqlGetEmail = string.Format(sqlGetEmail, ManageProvider.Provider.Current().DepartmentId);
                        DataTable dtEmail = VerifyPostBll.GetDataTable(sqlGetEmail);
                        string EmailList = "";
                        string WeChatList = "";
                        if(dtEmail.Rows.Count>0)
                        {
                            for(int i=0;i<dtEmail.Rows.Count;i++)
                            {
                                if (dtEmail.Rows[i][0].ToString() != "&nbsp;")
                                {
                                    EmailList += dtEmail.Rows[i][0].ToString() + ",";
                                    WeChatList += dtEmail.Rows[i][1].ToString() + "|";
                                }
                            }
                            EmailList = EmailList.Substring(0, EmailList.Length - 1);
                            WeChatList = WeChatList.Substring(0, WeChatList.Length - 1);
                            SendEmailByAccount(EmailList, " 岗位验证已达到三级预警，请登录系统查看! ");
                            WeChatHelper.SendWxMessage(WeChatList, " 岗位验证已达到三级预警，请登录系统查看! ");
                        }

                        //插入问题到快速反应

                        string sqlGetMessage = @"select a.*,b.FullName,(select top 1 Code from Base_User 
where DepartmentId=a.SetDepart and exists (select * from Base_ObjectUserRelation where ObjectId='91c17ca4-0cbf-43fa-829e-3021b055b6c4')
) as ResponseBy from VP_VerifyPost a left join Base_Department b on a.SetDepart=b.DepartmentId
where a.VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}')";
                        sqlGetMessage = string.Format(sqlGetMessage, verifypostdid);
                        FY_Rapid rapidentity = new FY_Rapid();
                        IDatabase database = DataFactory.Database();
                        DbTransaction isOpenTrans = database.BeginTrans();
                        try
                        {
                            DataTable dtGetMessage = VerifyPostBll.GetDataTable(sqlGetMessage);
                            //获取快速反应的数据
                           
                            rapidentity.Create();
                            rapidentity.res_ms = "岗位验证已达到三级预警，"+"当天的缺陷数量已达到 "+ DefectNumAll + "：" + dtGetMessage.Rows[0]["FullName"].ToString()+" "+dtGetMessage.Rows[0]["SetReason"].ToString();
                            rapidentity.res_cpeo = dtGetMessage.Rows[0]["ResponseBy"].ToString();
                            rapidentity.res_cdate = DateTime.Now;

                            database.Insert(rapidentity, isOpenTrans);
                            database.Commit();
                        }
                        catch
                        {
                            database.Rollback();
                            database.Close();
                        }
                    }
                    else
                    {
                        if(DefectNumAll>=TwoLevelAlarm)
                        {
                            //触发二级警报,厂长，质量经理，车间主任，班长
                            string sqlGetEmail = @" select Email,code from Base_User  a
where exists ( select * from Base_ObjectUserRelation where UserId=a.UserId and
ObjectId in ('91c17ca4-0cbf-43fa-829e-3021b055b6c4','f6afd4e4-6fb2-446f-88dd-815ddb91b09d') )
and (DepartmentId='{0}' or DepartmentId in (select ParentId from Base_Department where DepartmentId='{0}' ) ) ";
                            sqlGetEmail = string.Format(sqlGetEmail, ManageProvider.Provider.Current().DepartmentId);
                            DataTable dtEmail = VerifyPostBll.GetDataTable(sqlGetEmail);
                            string EmailList = "";
                            string WeChatList = "";
                            if (dtEmail.Rows.Count > 0)
                            {
                                for (int i = 0; i < dtEmail.Rows.Count; i++)
                                {
                                    if (dtEmail.Rows[i][0].ToString() != "&nbsp;")
                                    {
                                        EmailList += dtEmail.Rows[i][0].ToString() + ",";
                                        WeChatList += dtEmail.Rows[i][1].ToString() + "|";
                                    }
                                }
                                EmailList +=" li.wang@fuyaogroup.com ";
                                WeChatList += "008955";
                                SendEmailByAccount(EmailList, " 岗位验证已达到二级预警，请登录系统查看! ");
                                WeChatHelper.SendWxMessage(WeChatList, " 岗位验证已达到二级预警，请登录系统查看! ");
                            }

                            //插入问题到快速反应
                            string sqlGetMessage = @"select a.*,b.FullName,(select top 1 Code from Base_User 
where DepartmentId=a.SetDepart and exists (select * from Base_ObjectUserRelation where ObjectId='91c17ca4-0cbf-43fa-829e-3021b055b6c4')
) as ResponseBy from VP_VerifyPost a left join Base_Department b on a.SetDepart=b.DepartmentId
where a.VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}')";
                            sqlGetMessage = string.Format(sqlGetMessage, verifypostdid);

                            FY_Rapid rapidentity = new FY_Rapid();
                            IDatabase database = DataFactory.Database();
                            DbTransaction isOpenTrans = database.BeginTrans();
                            try
                            {
                                DataTable dtGetMessage = VerifyPostBll.GetDataTable(sqlGetMessage);
                                //获取快速反应的数据

                                rapidentity.Create();
                                rapidentity.res_ms = "岗位验证已达到二级预警,"+"当天的缺陷数量已达到 "+ DefectNumAll+":" + dtGetMessage.Rows[0]["FullName"].ToString() + " " + dtGetMessage.Rows[0]["SetReason"].ToString();
                                rapidentity.res_cpeo = dtGetMessage.Rows[0]["ResponseBy"].ToString();
                                rapidentity.res_cdate = DateTime.Now;

                                database.Insert(rapidentity, isOpenTrans);
                                database.Commit();
                            }
                            catch
                            {
                                database.Rollback();
                                database.Close();
                            }
                        }
                        else
                        {
                            if(DefectNumAll>=OneLevelAlarm)
                            {
                                //触发一级警报,车间主任，班长
                                string sqlGetEmail = @" select Email,code from Base_User  a
where exists ( select * from Base_ObjectUserRelation where UserId=a.UserId and
ObjectId in ('91c17ca4-0cbf-43fa-829e-3021b055b6c4','f6afd4e4-6fb2-446f-88dd-815ddb91b09d') )
and (DepartmentId='{0}' 
-- or DepartmentId in (select ParentId from Base_Department where DepartmentId='81b8d368-f4e4-4fd3-be9b-57a679c041b3' ) 
) ";
                                sqlGetEmail = string.Format(sqlGetEmail, ManageProvider.Provider.Current().DepartmentId);
                                DataTable dtEmail = VerifyPostBll.GetDataTable(sqlGetEmail);
                                string EmailList = "";
                                string WeChatList = "";
                                if (dtEmail.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dtEmail.Rows.Count; i++)
                                    {
                                        if (dtEmail.Rows[i][0].ToString() != "&nbsp;")
                                        {
                                            EmailList += dtEmail.Rows[i][0].ToString() + ",";
                                            WeChatList += dtEmail.Rows[i][1].ToString() + "|";
                                        }
                                    }
                                    EmailList = EmailList.Substring(0, EmailList.Length - 1);
                                    WeChatList = WeChatList.Substring(0, WeChatList.Length - 1);
                                    SendEmailByAccount(EmailList, " 岗位验证已达到一级预警，请登录系统查看! ");
                                    WeChatHelper.SendWxMessage(WeChatList, " 岗位验证已达到一级预警，请登录系统查看! ");
                                }

                                //插入问题到一般问题处理流程
                                string sqlGetMessage = @"select a.*,b.FullName,(select top 1 Code from Base_User 
where DepartmentId=a.SetDepart and exists (select * from Base_ObjectUserRelation where ObjectId='91c17ca4-0cbf-43fa-829e-3021b055b6c4')
) as ResponseBy from VP_VerifyPost a left join Base_Department b on a.SetDepart=b.DepartmentId
where a.VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}')";
                                sqlGetMessage = string.Format(sqlGetMessage, verifypostdid);

                                FY_GeneralProblem generalentity = new FY_GeneralProblem();
                                IDatabase database = DataFactory.Database();
                                DbTransaction isOpenTrans = database.BeginTrans();
                                try
                                {
                                    DataTable dtGetMessage = VerifyPostBll.GetDataTable(sqlGetMessage);
                                    //获取快速反应的数据

                                    generalentity.Create();
                                    generalentity.ProblemDescripe = "岗位验证已达到一级预警," + "当天的缺陷数量已达到 " + DefectNumAll +":" + dtGetMessage.Rows[0]["FullName"].ToString() + " " + dtGetMessage.Rows[0]["SetReason"].ToString();
                                    generalentity.ResponseBy = dtGetMessage.Rows[0]["ResponseBy"].ToString();
                                    generalentity.HappenDate = DateTime.Now;
                                    generalentity.CauseAnalysis = "&nbsp;";
                                    generalentity.CorrectMeasures = "&nbsp;";
                                    generalentity.FinishStatus = "进行中";


                                    database.Insert(generalentity, isOpenTrans);
                                    database.Commit();
                                }
                                catch
                                {
                                    database.Rollback();
                                    database.Close();
                                }
                            }
                        }
                    }
                }

                if(temp=="6"&&Convert.ToInt32(defectNum)==0)
                {
                    //当天最后一次，并且失效数量是0的话，检验下是否连续的无失效天数大于等于周期
                    string sql = @"select isnull(dbo.[GetContinuousDayByVeryPostDID]('{0}'),0)";
                    sql = string.Format(sql, verifypostdid);

                    DataTable dt = VerifyPostBll.GetDataTable(sql);
                    int MaxRq = 0;
                    if(dt.Rows.Count>0)
                    {
                        MaxRq = Convert.ToInt32(dt.Rows[0][0].ToString());
                    }

                    //获取周期
                    sql = @"select VerifyCycle from VP_VerifyPost where 
VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}')";
                    sql = string.Format(sql, verifypostdid);
                    dt = VerifyPostBll.GetDataTable(sql);
                    int Cycle=Convert.ToInt32(dt.Rows[0][0].ToString());
                    
                    //连续天数大于周期，变成完成
                    if(MaxRq>=Cycle)
                    {
                        StringBuilder sqlFinish =new StringBuilder();
                        sqlFinish.AppendFormat(@" update VP_VerifyPost set Status='已完成',RealQuitDate=getdate()
where VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}') ",verifypostdid);
                        VerifyPostBll.ExecuteSql(sqlFinish);
                    }

                }
            }
            //移动到前面去，不然界限判定会有问题
            //VerifyPostBll.ExecuteSql(strSql);
            return "0";
        }

        public string SendEmailByAccount(string reciver, string Content)
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
            for (int i = 0; i < reviverArr.Length; i++)
            {
                message.To.Add(reviverArr[i]);
            }
            //message.To.Add("yao.sun@fuyaogroup.com");

            //message.To.Add("zhonghua.yan@fuyaogroup.com");

            //message.To.Add("li.wang@fuyaogroup.com");



            //设置抄送人
            message.CC.Add("jun.shen@fuyaogroup.com");
            //设置邮件标题
            message.Subject = "岗位验证系统邮件";
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

        public ActionResult TrendPicture()
        {
            return View();
        }

        public string GetTrendData(string ID)
        {
            string x="";
            string y="";
            string result;
            string sql = @" select cast(month(VerifyDate) as nvarchar(10))+'-'+cast(day(VerifyDate) as nvarchar(10)),
cast(100.0*(b.DefectNum1+b.DefectNum2+b.DefectNum3+b.DefectNum4+b.DefectNum5+b.DefectNum6)/(case when (b.CheckNum1+b.CheckNum2+b.CheckNum3+b.CheckNum4+b.CheckNum5+b.CheckNum6)=0 then 1 else (b.CheckNum1+b.CheckNum2+b.CheckNum3+b.CheckNum4+b.CheckNum5+b.CheckNum6) end) as decimal(18,2)) as DefectRate
from VP_VerifyPostDetail b
where VerifyPostID='{0}'
order by VerifyDate
  ";
            sql = string.Format(sql, ID);

            DataTable dt = VerifyPostBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    x = x + dt.Rows[i][0].ToString() + ",";
                    y = y + dt.Rows[i][1].ToString() + ",";
                }
                x = x.Substring(0, x.Length - 1);
                y = y.Substring(0, y.Length - 1);
            }
            result = x + "|" + y;
            return result;
            
            
        }

        public ActionResult AuditForm(string KeyValue,string Type)
        {

            //第一步，插入数据到新表中
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" insert into VP_VerifyPostDetailReview
select NEWID(),'{0}',ReviewContent,'{1}','',''
from VP_ReviewData where ReviewContent not in 
(select ReviewContent from VP_VerifyPostDetailReview where VerifyPostDID='{0}' and reviewby='{1}') ", KeyValue,Type);
            VerifyPostBll.ExecuteSql(strSql);

            //第二部，获取数据
            string sql = " select * from VP_VerifyPostDetailReview where VerifyPostDID='{0}' and ReviewBy='{1}' ";
            sql = String.Format(sql, KeyValue,Type);
            DataTable dt = VerifyPostBll.GetDataTable(sql);
            ViewData["dt"] = dt;

            return View();
        }

        public int UpdateResultAll(string result)
        {
            StringBuilder strSql = new StringBuilder();
            string[] resultArr = result.Split(';');
            for (int i = 0; i < resultArr.Length; i++)
            {
                string[] IdResult = resultArr[i].Split(':');
                strSql.AppendFormat(" update VP_VerifyPostDetailReview set Result='{0}',RealCreateby='{2}' where ReviewID='{1}' ", IdResult[1], IdResult[0], ManageProvider.Provider.Current().UserId);
            }
            int last = VerifyPostBll.ExecuteSql(strSql);
            return last;
        }

        public ActionResult GeneralProblem()
        {
            return View();
        }

        public ActionResult ResponseJson()
        {
            string sql = " select userid,code,realname+'('+code+')' as name from base_user where 1=1 and Enabled=1  ";
            DataTable dt = VerifyPostBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult CustomerJson()
        {
            string sql = " select * from FY_CUS ";
            DataTable dt = VerifyPostBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public int InsertGeneralProblem(string _Descripe,string _ResponseBy
            ,string _PlanDate,string _ProblemType,string _ProblemType2,string _Area,
            string _IsAgain,string _Customer)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"insert into FY_GeneralProblem (GeneralProblemID,ProductArea,ProblemType,IsAgain,
ProblemType2,ResponseBy,Customer,ProblemDescripe
,HappenDate,CauseAnalysis,CorrectMeasures,FinishStatus,PlanFinishDt)
values(NEWID(),'{0}','{1}','{2}','{3}','{4}','{5}','{6}',GETDATE(),'&nbsp;','&nbsp;','进行中','{7}')",
_Area,_ProblemType,_IsAgain,_ProblemType2,_ResponseBy,_Customer,_Descripe,_PlanDate);
            return VerifyPostBll.ExecuteSql(strSql);
        }

        public ActionResult ReasonJson(string State)
        {
            string sql = @" select SetReason from VP_VerifyPost where SetDepart='{0}'  ";
            if(State!=null&&State!=""&&State!="undefined")
            {
                sql += " and status='"+State+"' ";
            }
            sql = string.Format(sql, ManageProvider.Provider.Current().DepartmentId);
            DataTable dt = VerifyPostBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="VerifyPostDID"></param>
        /// <param name="Type">标识更新哪个字段，质保、厂长、车间主任、班长</param>
        /// <returns></returns>
        public int AuditIsProduce(string VerifyPostDID,string Type)
        {
            StringBuilder strSql = new StringBuilder();
            string field = "";
            if(Type== "QA")
            {
                field = "QualityApprove";
            }
            else if(Type=="FM")
            {
                field = "FactoryManager";
            }
            else if(Type== "WM")
            {
                field = "WorkShopManager";
            }
            else if(Type== "GM")
            {
                field = "GroupManager";
            }
            else
            {
                field = "未知";
            }
            
            strSql.AppendFormat(@" update VP_VerifyPostDetail set {0}='未生产' where VerifyPostDID='{1}' ",
field,VerifyPostDID);
            try
            {
                return VerifyPostBll.ExecuteSql(strSql);
            }
            catch
            {
                return -1;
            }
            

        }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="Account">账户</param>
        /// <param name="Password">密码</param>
        /// <returns></returns>
        public ActionResult CheckLogin(string Account, string Password, string Token,string Type)
        {
            Base_UserBll base_userbll = new Base_UserBll();
            Base_ObjectUserRelationBll base_objectuserrelationbll = new Base_ObjectUserRelationBll();
            string Msg = "";
            try
            {
                IPScanerHelper objScan = new IPScanerHelper();
                string IPAddress = NetHelper.GetIPAddress();
                objScan.IP = IPAddress;
                objScan.DataPath = Server.MapPath("~/Resource/IPScaner/QQWry.Dat");
                string IPAddressName = objScan.IPLocation();
                string outmsg = "";

                
               
                    Base_User base_user = base_userbll.UserLogin(Account, Password, out outmsg);
                    switch (outmsg)
                    {
                        case "-1":      //账户不存在
                            Msg = "-1";
                            break;
                        case "lock":    //账户锁定
                            Msg = "2"; 
                            break;
                        case "error":   //密码错误
                            Msg = "4";
                            break;
                        case "succeed": //验证成功
                            //对在线人数全局变量进行加1处理
                            //HttpContext rq = System.Web.HttpContext.Current;
                            //rq.Application["OnLineCount"] = (int)rq.Application["OnLineCount"] + 1;
                            Msg = "3";//验证成功
                            string ObjectId = base_objectuserrelationbll.GetObjectId(base_user.UserId);
                            //对登录成功的账号验证他的角色是否符合系统的要求
                            string[] ObjectIDs = ObjectId.Split(',');
                            string RequireRole = "";
                            if(Type== "FM")
                            {
                                RequireRole = "54804f22-89a1-4eee-b257-255deaf4face";
                                
                             }
                            if(Type== "WM")
                            {
                                 RequireRole = "91c17ca4-0cbf-43fa-829e-3021b055b6c4";
                            }
                            if(Type== "GM")
                            {
                                 RequireRole = "f6afd4e4-6fb2-446f-88dd-815ddb91b09d";
                             }
                            for(int i=0;i<ObjectIDs.Length;i++)
                            {
                                 if(RequireRole==ObjectIDs[i])
                                 {
                                     if(Type == "WM"|| Type == "GM")
                                     {
                                         if(base_user.DepartmentId==ManageProvider.Provider.Current().DepartmentId)
                                         {
                                        Msg = "5";
                                         }
                                         else
                                          {
                                                Msg = "3";
                                          }
                                      }
                                       else if (Type == "FM")
                                        {
                                            string sql = " select ParentId from base_department where DepartmentId='" + ManageProvider.Provider.Current().DepartmentId + "' ";
                                            DataTable dt = VerifyPostBll.GetDataTable(sql);
                                            if (base_user.DepartmentId == dt.Rows[0][0].ToString())
                                            {
                                                Msg = "5";
                                            }
                                        }
                                        else
                                        {
                                            Msg = "3";
                                        }
                                     
                                  }
                            }
                            break;
                        default:
                            break;
                    }
                
            }
            catch (Exception ex)
            {
                Msg = ex.Message;
            }
            return Content(Msg);
        }

        public ActionResult loginform()
        {
            return View();
        }


        public ActionResult UserIndex()
        {
            return View();
        }

        public ActionResult GetUserListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = VerifyPostBll.GetUserList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult UserList()
        {
            return View();
        }

        public ActionResult GetUserList()
        {
            StringBuilder sb = new StringBuilder();
            string sql = "select a.UserID,a.RealName+'('+Code+')' as Name from Base_user a  where Enabled=1 ";
            DataTable dt = VerifyPostBll.GetDataTable(sql);
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

        public ActionResult UserListSubmit(string DepartmentID, string ObjectId)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();

                string[] array = ObjectId.Split(',');
                for (int i = 0; i < array.Length - 1; i++)
                {
                    


                   
                    //给新选择的用户添加车间主任的权限
                    strSql.AppendFormat(@"delete from Base_ObjectUserRelation where  ObjectId='056cfbbc-28bd-42a1-a98a-ace1adce2158' and UserId='{0}' ", array[0]);
                    strSql.AppendFormat(@"insert into Base_ObjectUserRelation values(NEWID(),2,'056cfbbc-28bd-42a1-a98a-ace1adce2158','{0}',1,GETDATE(),'{1}','{2}') ", array[0], ManageProvider.Provider.Current().UserId, ManageProvider.Provider.Current().UserName);
                   
                }
                VerifyPostBll.ExecuteSql(strSql);
                return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());


            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }


        }

        public int DeleteRole(string UserID)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();

                strSql.AppendFormat(@"delete from Base_ObjectUserRelation where  ObjectId='056cfbbc-28bd-42a1-a98a-ace1adce2158' and UserId='{0}' ", UserID);
                return VerifyPostBll.ExecuteSql(strSql);
            }
            catch
            {
                return -1;
            }
        }

        public ActionResult IsProduce()
        {
            return View();
        }











    }
}
