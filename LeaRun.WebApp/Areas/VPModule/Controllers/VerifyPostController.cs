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
                
            }
            else
            {

                strSql.AppendFormat(@" update VP_VerifyPostDetail set DefectNum{3}='{1}',CheckNum{3}='{2}' 
where VerifyPostDID='{0}' ",verifypostdid,defectNum,checkNum,temp);
                //当失效数量不是0的时候，重新更新明细表中的数据，在当前日期的后面再增加周期数量的数据
                if(defectNum!="0")
                {
                    //删除今天以后的数据，再按照周期数重新添加
                    StringBuilder sqlDelete = new StringBuilder();
                    sqlDelete.AppendFormat(@" delete from VP_VerifyPostDetail where VerifyDate>cast(GETDATE() as date) 
and VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}') ",verifypostdid);
                    VerifyPostBll.ExecuteSql(sqlDelete);
                    //根据周期重新添加数据
                    IDatabase database = DataFactory.Database();
                    DbTransaction isOpenTrans = database.BeginTrans();
                    string sqlGetinfo = @" select * from VP_VerifyPost where VerifyPostID in 
(select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}') ";
                    sqlGetinfo = string.Format(sqlGetinfo, verifypostdid);
                    DataTable dtGetInfo = VerifyPostBll.GetDataTable(sqlGetinfo);
                    int C1 = Convert.ToInt32(dtGetInfo.Rows[0]["VerifyCycle"].ToString());
                    string VerifyPostID = dtGetInfo.Rows[0]["VerifyPostID"].ToString();
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

                    //此处预计加入，根据报警设置数量来发邮件预警和加入问题到快速反应以及一般问题跟踪
                }

                if(temp=="6"&&Convert.ToInt32(defectNum)==0)
                {
                    //当天最后一次，并且失效数量是0的话，检验下是否连续的无失效天数大于等于周期
                    string sql = @"select max(rn)
from
(
select VerifyPostID, VerifyDate, ROW_NUMBER() OVER(partition by VerifyPostID order by VerifyDate asc) as rn
from VP_VerifyPostDetail
where DefectNum1=0 and DefectNum2=0 and DefectNum3=0 
and DefectNum4=0 and DefectNum5=0 and DefectNum6=0 and VerifyDate<=cast(GETDATE() as date)
and VerifyDate<=GETDATE() and VerifyPostID in (select VerifyPostID from VP_VerifyPostDetail where VerifyPostDID='{0}')
) as aa";
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
            VerifyPostBll.ExecuteSql(strSql);
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
            string sql = " select code,realname+'('+code+')' as name from base_user where 1=1 and Enabled=1  ";
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

        public ActionResult ReasonJson()
        {
            string sql = @" select SetReason from VP_VerifyPost where SetDepart='{0}'  ";
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
                                     Msg = "5";
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











    }
}
