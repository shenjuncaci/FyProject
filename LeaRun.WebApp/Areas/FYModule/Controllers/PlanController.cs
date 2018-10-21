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

namespace LeaRun.WebApp.Areas.FYModule.Controllers
{
    public class PlanController : Controller
    {
        //
        // GET: /FYModule/Plan/
        FY_PlanBll PlanBll = new FY_PlanBll();

        public ActionResult Index()
        {
            return View();
        }

        public int InsertEventData(string PlanContent, string PlanDate, string ResponseBy, string Line, string banzu)
        {
            StringBuilder strSql = new StringBuilder();
            string UserID = ManageProvider.Provider.Current().UserId;
            string DepartmentID = ManageProvider.Provider.Current().DepartmentId;
            if (DepartmentID == "")
            {
                return -1;
            }
            strSql.AppendFormat(" insert into fy_plan (PlanID,UserID,Plandate,PlanContent,DepartmentID,ResponseByID,BackColor,Line,GroupID) " +
                "values(newid(),'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                UserID, PlanDate, PlanContent, DepartmentID, ResponseBy, "grey", Line, banzu);
            int result = PlanBll.ExecuteSql(strSql);
            return result;
        }

        public ActionResult InsertPlan()
        {
            return View();
        }

        /// <summary>
        /// 提供calendar数据
        /// </summary>
        /// Date为js格式，自己分割取年和月
        /// <returns>id1,id2|date1,date2|conten1,conten2</returns>
        public string GetEventData(string Date)
        {

            string[] dateArr = Date.Split(' ');
            string monthEN = dateArr[1];    //js的时间获取的月份是英文简写，需再转一次
            string month = "";
            switch (monthEN)
            {
                case "Jan":
                    month = "1";
                    break;
                case "Feb":
                    month = "2";
                    break;
                case "Mar":
                    month = "3";
                    break;
                case "Apr":
                    month = "4";
                    break;
                case "May":
                    month = "5";
                    break;
                case "Jun":
                    month = "6";
                    break;
                case "Jul":
                    month = "7";
                    break;
                case "Aug":
                    month = "8";
                    break;
                case "Sep":
                    month = "9";
                    break;
                case "Oct":
                    month = "10";
                    break;
                case "Nov":
                    month = "11";
                    break;
                case "Dec":
                    month = "12";
                    break;
                default:
                    month = monthEN;
                    break;

            }
            string year = dateArr[3];

            string id = ManageProvider.Provider.Current().ObjectId;
            string sql = @"select PlanID,Plandate,case  when exists (select * from FY_PlanDetail where PlanID=a.PlanID) and not exists (select * from FY_PlanDetail where PlanID=a.PlanID and ExamResult='') then '已完成' when IsLeave=0 then PlanContent else '休息' end as PlanContent,a.BackColor from fy_plan a 
left join Base_User b on a.ResponseByID=b.UserId left join Base_GroupUser c on a.groupid=c.GroupUserId
where a.BackColor!='' and a.DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "' " +
//"and plandate>='"+year+"-"+month+"-01"+
"and year(plandate)=" + year + " and month(plandate)=" + month + " " +
" order by Plandate,BackColor ";
            DataTable dt = PlanBll.GetDataTable(sql);
            string result = "";
            string temp1 = "";
            string temp2 = "";
            string temp3 = "";
            string temp4 = "";
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    temp1 = temp1 + dt.Rows[i][0] + ",";
                    temp2 = temp2 + dt.Rows[i][1] + ",";
                    temp3 = temp3 + dt.Rows[i][2] + ",";
                    temp4 = temp4 + dt.Rows[i][3] + ",";
                }
                temp1 = temp1.Substring(0, temp1.Length - 1);
                temp2 = temp2.Substring(0, temp2.Length - 1);
                temp3 = temp3.Substring(0, temp3.Length - 1);
                temp4 = temp4.Substring(0, temp4.Length - 1);
            }
            result = temp1 + "|" + temp2 + "|" + temp3 + "|" + temp4;
            return result;
        }

        public ActionResult AuditList()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PlanBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult AuditForm(string KeyValue)
        {
            string deptID = "";
            string sqlGetDept = " select * from fy_plan where PlanID='" + KeyValue + "' ";
            DataTable dtGetDept = PlanBll.GetDataTable(sqlGetDept);
            if (dtGetDept.Rows.Count > 0)
            {
                deptID = dtGetDept.Rows[0]["DepartmentID"].ToString();
            }

            string IsLeader = "";
            string SqlIsLeader = @"select * from Base_ObjectUserRelation where UserId='" + ManageProvider.Provider.Current().UserId + "' and ObjectId in ('8431ea77-69c9-484c-a67f-4f3419c0d393', '91c17ca4-0cbf-43fa-829e-3021b055b6c4','54804f22-89a1-4eee-b257-255deaf4face')";
            DataTable dtIsLeader = PlanBll.GetDataTable(SqlIsLeader);
            if (dtIsLeader.Rows.Count > 0)
            {
                //如果有车间主任、厂长、副总的权限，除了通用还要加上系统监督的审批内容
                IsLeader = "系统监督";
            }

            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"insert into fy_plandetail  select NEWID(),'{0}',processid,'','{1}',ProcessName,AbleProcess,AuditContent,FailureEffect,ReactionPlan from FY_Process where processid not in 
                (select processid from fy_plandetail where planid='{0}') and (ProcessName in (select PlanContent from FY_Plan where PlanID='{0}' ) 
                or (processname in ('通用','{3}') and DepartmentID='cb016a86-d835-4eb9-ad80-6a86d2140c88') )  and (DepartmentID='{2}' or DepartmentID='cb016a86-d835-4eb9-ad80-6a86d2140c88') 
                and cast(EndDate as date)>=cast(GETDATE() as date) ",
                KeyValue, ManageProvider.Provider.Current().UserId, deptID, IsLeader);
            PlanBll.ExecuteSql(strSql);

            //获取数据展示到页面
            string sql = @" select a.plandid,a.ProcessName,a.AbleProcess,a.AuditContent,a.FailureEffect,a.ReactionPlan,a.examresult 
from fy_plandetail a left join FY_Process b on a.processid=b.ProcessID where a.planid='" + KeyValue + "' order by a.ProcessName desc ";
            DataTable dt = PlanBll.GetDataTable(sql);
            ViewData["dt"] = dt;

            return View();
        }

        public ActionResult Form()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, FY_Plan entity, string BuildFormJson, HttpPostedFileBase Filedata)
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

                    //entity.DepartmentID = ManageProvider.Provider.Current().DepartmentId;
                    entity.ResponseByID = ManageProvider.Provider.Current().UserId;
                    entity.IsLeave = 0;
                    entity.Plandate = DateTime.Now;
                    entity.UserID = ManageProvider.Provider.Current().UserId;
                    entity.BackColor = "";
                    entity.Modify(KeyValue);


                    database.Update(entity, isOpenTrans);

                }
                else
                {
                    //这些在新建的时候给他固定
                    //entity.DepartmentID = ManageProvider.Provider.Current().DepartmentId;
                    entity.ResponseByID = ManageProvider.Provider.Current().UserId;
                    entity.IsLeave = 0;
                    entity.Plandate = DateTime.Now;
                    entity.UserID = ManageProvider.Provider.Current().UserId;
                    entity.BackColor = "";


                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.PlanID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.PlanID, ModuleId, isOpenTrans);
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
            FY_Plan entity = DataFactory.Database().FindEntity<FY_Plan>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        public int LeaveEventData(string id)
        {
            string sql = " select * from FY_PlanDetail where PlanID='" + id + "'  ";
            DataTable dt = PlanBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                //返回-2表示已经有审核记录，不能请假
                return -2;
            }
            StringBuilder strSql = new StringBuilder();

            strSql.AppendFormat(" update fy_plan set IsLeave=1 where planid='{0}'", id);
            int result = PlanBll.ExecuteSql(strSql);
            return result;
        }

        public int DeleteEventData(string id)
        {
            string sql = " select * from FY_PlanDetail where PlanID='" + id + "'  ";
            DataTable dt = PlanBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                //返回-2表示已经有明细数据，不能删除
                return -2;
            }
            StringBuilder strSql = new StringBuilder();

            strSql.AppendFormat(" update fy_plan set PlanContent='' where planid='{0}'", id);
            int result = PlanBll.ExecuteSql(strSql);
            return result;
        }

        public ActionResult EventJson()
        {
            DataTable ListData = PlanBll.GetEventList();
            return Content(ListData.ToJson());
        }

        public ActionResult EventJson2(string DepartmentId)
        {
            string sql = "select distinct ProcessName from FY_Process where DepartmentID='" + DepartmentId + "'";
            DataTable dt = PlanBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult LineJson()
        {
            string sql = " select distinct LineName from FY_ProduceLine where DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "'  ";
            DataTable dt = PlanBll.GetDataTable(sql);
            return Content(dt.ToJson());

        }

        public ActionResult LineJson2(string DepartmentId)
        {
            string sql = " select distinct LineName from FY_ProduceLine where DepartmentID='" + DepartmentId + "'  ";
            DataTable dt = PlanBll.GetDataTable(sql);
            return Content(dt.ToJson());

        }


        public ActionResult banzuJson()
        {
            string sql = " select distinct GroupUserId,FullName from  Base_GroupUser where DepartmentId='" + ManageProvider.Provider.Current().DepartmentId + "' ";
            DataTable dt = PlanBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult ResponseJson()
        {
            DataTable ListData = PlanBll.GetResponseByList();
            return Content(ListData.ToJson());
        }

        public ActionResult ResponseAllJson()
        {
            DataTable ListData = PlanBll.GetResponseAllByList();
            return Content(ListData.ToJson());
        }

        public int UpdateExamResult(string id, string result)
        {
            StringBuilder strSql = new StringBuilder();

            strSql.AppendFormat(" update fy_plandetail set ExamResult='{0}' where plandid='{1}'", result, id);
            int resultCount = PlanBll.ExecuteSql(strSql);
            return resultCount;
        }

        public ActionResult Report()
        {
            return View();
        }


        public ActionResult GetReportJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson
            , string startDate, string endDate, string UserMan)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PlanBll.GetReportList(keywords, ref jqgridparam, ParameterJson, startDate, endDate, UserMan);
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

        public void ExcelExport(string startDate, string endDate)
        {
            ExcelHelper ex = new ExcelHelper();
            string sql = "select e.fullname as 部门,d.realname as 用户名,a.Plandate as 计划日期,a.PlanContent as 计划内容,c.AbleProcess as 适用过程,c.AuditContent as 审核内容,c.FailureEffect as 失效后果,c.ProcessName as 工序名称,b.ExamResult as 检查结果 ";
            sql += " from FY_Plan a left join FY_PlanDetail b on a.PlanID=b.PlanID left join FY_Process c on b.ProcessID=c.ProcessID left join Base_User d on a.ResponseByID=d.UserId left join base_department e on e.departmentid=d.departmentid where 1=1 order by plandate,fullname desc ";
            if (startDate != null && startDate != "undefined" && startDate != "")
            {
                sql += " and a.Plandate>='" + startDate + "' ";
            }

            if (endDate != null && endDate != "undefined" && endDate != "")
            {
                sql += " and a.Plandate<='" + endDate + "' ";
            }
            DataTable ListData = PlanBll.GetDataTable(sql);
            ex.EcportExcel(ListData, "分层审核报表导出");
        }

        public string GetInfo()
        {
            string Result = "";
            string sql = @"select a.FullName,a.StartTime,a.EndTime,a.BackColor,b.RealName 
from Base_GroupUser a left join Base_User b on a.GroupLeader=b.UserId
where a.DepartmentId='" + ManageProvider.Provider.Current().DepartmentId + "' order by a.StartTime ";
            DataTable dt = PlanBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Result = Result + dt.Rows[i]["FullName"] + " " + " 时间:" + dt.Rows[i]["StartTime"] + "--" + dt.Rows[i]["EndTime"] +
                        "<div style='background:" + dt.Rows[i]["BackColor"] + ";color:" + dt.Rows[i]["BackColor"] + "'>11111</div>" + "</br>";
                }
                Result = Result + "车间主任 " + "<div style='background:grey;color:grey'>11111</div>" + "</br>";
            }

            bool IsLeader = false;  //判断是否车间班长，是班长的情况下，不显示批量排产键
            IManageUser user = ManageProvider.Provider.Current();
            string[] objectID = user.ObjectId.Split(',');
            for (int i = 0; i < objectID.Length; i++)
            {
                if (objectID[i] == "91c17ca4-0cbf-43fa-829e-3021b055b6c4")
                {
                    IsLeader = true;
                }
            }
            if (IsLeader == true)
            {
                Result = Result + "<button id=\"SetPlan\" class=\"button button-caution button - square button - small\" onclick=\"BatchPlan()\">随机生成审核计划</button> </br>";
            }
            //下面添加厂长车间主任的信息
            //Result = Result + "<div></div>";
            return Result;
        }
        /// <summary>
        /// 根据基础数据一键随机排产
        /// date格式 七月 2017
        /// </summary>
        /// <returns></returns>
        public int BatchPlan(string date)
        {
            string[] dateArr = date.Split(' ');
            string monthEN = dateArr[0];    //传过来的月份是中文，修改成阿拉伯数字
            string month = "";
            switch (monthEN)
            {
                case "一月":
                    month = "1";
                    break;
                case "二月":
                    month = "2";
                    break;
                case "三月":
                    month = "3";
                    break;
                case "四月":
                    month = "4";
                    break;
                case "五月":
                    month = "5";
                    break;
                case "六月":
                    month = "6";
                    break;
                case "七月":
                    month = "7";
                    break;
                case "八月":
                    month = "8";
                    break;
                case "九月":
                    month = "9";
                    break;
                case "十月":
                    month = "10";
                    break;
                case "十一月":
                    month = "11";
                    break;
                case "十二月":
                    month = "12";
                    break;
                default:
                    month = monthEN;
                    break;

            }
            string year = dateArr[1];
            //如果有数据，先把当前月份的数据删除，删除的数据必须是没有明细表的数据
            StringBuilder DeleteSql = new StringBuilder();
            DeleteSql.AppendFormat("delete from fy_plan where DepartmentID='{0}' and month(plandate)=" + month + " and year(plandate)=" + year + " and (BackColor!='' or backcolor='grey') " +
                " and not exists (select * from FY_PlanDetail where planid=fy_plan.PlanID ) and cast(plandate as date)>cast(getdate() as date) ",
                ManageProvider.Provider.Current().DepartmentId);
            PlanBll.ExecuteSql(DeleteSql);

            StringBuilder InsertSql = new StringBuilder();
            //第一步，获取基础数据
            string sql = @"select b.UserId,a.FullName,a.StartTime,a.EndTime,a.BackColor,b.RealName,a.GroupUserId 
from Base_GroupUser a left join Base_User b on a.GroupLeader=b.UserId
where a.DepartmentId='" + ManageProvider.Provider.Current().DepartmentId + "'";
            DataTable dt = PlanBll.GetDataTable(sql);
            //此处获取岗位信息，排产时随机指定审核表内容
            string sqlPost = @" select ProcessName from FY_Process where DepartMentID='" + ManageProvider.Provider.Current().DepartmentId + "' and ProcessName!='通用'  ";
            DataTable dtPost = PlanBll.GetDataTable(sqlPost);

            string sqlLine = @"select LineName,LineID from FY_ProduceLine where DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "'";
            DataTable dtLine = PlanBll.GetDataTable(sqlLine);
            Random ra = new Random();
            int aa = ra.Next(dtPost.Rows.Count);
            int bb = ra.Next(dtLine.Rows.Count);
            //第二部，根据获取的基础数据循环插入计划条数
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DateTime nowdt = DateTime.Now;

                    //循环日期的方法
                    //这个月最小值
                    DateTime mindt = DateTime.Parse(year + "-" + month + "-01");
                    DateTime maxdt = mindt.AddMonths(1).AddDays(-1);
                    if (nowdt > mindt)
                    {
                        mindt = Convert.ToDateTime(nowdt.AddDays(1).ToString("yyyy-MM-dd"));
                    }
                    //这个月最大值
                    //DateTime maxdt = DateTime.Parse(DateTime.Parse(year + "-" + (Convert.ToInt32(month)+1).ToString() + "-01").AddDays(-1).ToString("yyyy-MM-dd"));

                    while (mindt <= maxdt)
                    {
                        aa = ra.Next(dtPost.Rows.Count);
                        bb = ra.Next(dtLine.Rows.Count);
                        InsertSql.AppendFormat(@"insert into fy_plan (PlanID,UserID,Plandate,PlanContent,DepartmentID,ResponseByID,BackColor,GroupID,line) 
values(newid(),'{0}','{1}',{2},'{3}','{4}','{5}','{6}','{7}')",
                            ManageProvider.Provider.Current().UserId, 
                            mindt, 
                            "(select top 1 PostName from FY_LinePost where LineID='"+ dtLine.Rows[bb]["LineID"].ToString() + "' order by NEWID())",
                            ManageProvider.Provider.Current().DepartmentId, 
                            "",
                            dt.Rows[i]["BackColor"].ToString(), 
                            dt.Rows[i]["GroupUserId"].ToString(),
                            dtLine.Rows[bb]["LineName"].ToString());

                        //如果不是周末，再添加车间主任的审核计划
                        //string temp = mindt.DayOfWeek.ToString();
                        mindt = mindt.AddDays(1);

                    }
                }
            }

            #region 第三步，加上车间主任的信息，厂长因为没有和车间绑定，暂时没有办法加入到随机排产中，后期可以在部门处加一个厂长的字段来实现厂长的随机计划
            //InsertSql.AppendFormat("insert into fy_plan (PlanID,UserID,Plandate,PlanContent,DepartmentID,ResponseByID,BackColor,GroupID,line) values(newid(),'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
            //                ManageProvider.Provider.Current().UserId, year + "-" + month + "-05", dtPost.Rows[aa]["ProcessName"].ToString(),
            //                ManageProvider.Provider.Current().DepartmentId, ManageProvider.Provider.Current().UserId,
            //                "", "",
            //                dtLine.Rows[bb]["LineName"].ToString());
            //InsertSql.AppendFormat("insert into fy_plan (PlanID,UserID,Plandate,PlanContent,DepartmentID,ResponseByID,BackColor,GroupID,line) values(newid(),'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
            //                ManageProvider.Provider.Current().UserId, year + "-" + month + "-10", dtPost.Rows[aa]["ProcessName"].ToString(),
            //                ManageProvider.Provider.Current().DepartmentId, ManageProvider.Provider.Current().UserId,
            //                "", "",
            //                dtLine.Rows[bb]["LineName"].ToString());
            //InsertSql.AppendFormat("insert into fy_plan (PlanID,UserID,Plandate,PlanContent,DepartmentID,ResponseByID,BackColor,GroupID,line) values(newid(),'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
            //                ManageProvider.Provider.Current().UserId, year + "-" + month + "-15", dtPost.Rows[aa]["ProcessName"].ToString(),
            //                ManageProvider.Provider.Current().DepartmentId, ManageProvider.Provider.Current().UserId,
            //                "", "",
            //                dtLine.Rows[bb]["LineName"].ToString());
            //InsertSql.AppendFormat("insert into fy_plan (PlanID,UserID,Plandate,PlanContent,DepartmentID,ResponseByID,BackColor,GroupID,line) values(newid(),'{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
            //                ManageProvider.Provider.Current().UserId, year + "-" + month + "-25", dtPost.Rows[aa]["ProcessName"].ToString(),
            //                ManageProvider.Provider.Current().DepartmentId, ManageProvider.Provider.Current().UserId,
            //                "", "",
            //                dtLine.Rows[bb]["LineName"].ToString());
            DateTime nowdt2 = DateTime.Now;

            DateTime mindt2 = DateTime.Parse(year + "-" + month + "-01");
            DateTime maxdt2 = mindt2.AddMonths(1).AddDays(-1);
            if (nowdt2 > mindt2)
            {
                mindt2 = Convert.ToDateTime(nowdt2.AddDays(1).ToString("yyyy-MM-dd"));
            }

            while (mindt2 <= maxdt2)
            {
                aa = ra.Next(dtPost.Rows.Count);
                bb = ra.Next(dtLine.Rows.Count);
                if (mindt2.DayOfWeek.ToString() != "Sunday" && mindt2.DayOfWeek.ToString() != "Saturday")
                {
                    InsertSql.AppendFormat(@"insert into fy_plan (PlanID,UserID,Plandate,PlanContent,DepartmentID,ResponseByID,BackColor,GroupID,line) 
values(newid(),'{0}','{1}',{2},'{3}','{4}','{5}','{6}','{7}')",
                    ManageProvider.Provider.Current().UserId, 
                    mindt2, 
                    "(select top 1 PostName from FY_LinePost where LineID='" + dtLine.Rows[bb]["LineID"].ToString() + "' order by NEWID())",
                    ManageProvider.Provider.Current().DepartmentId, ManageProvider.Provider.Current().UserId,
                    "grey", 
                    "",
                    dtLine.Rows[bb]["LineName"].ToString());
                }
                mindt2 = mindt2.AddDays(1);
            }

            #endregion
            //最后，执行sql，我去；这么长的sql希望执行效率没有问题
            return PlanBll.ExecuteSql(InsertSql);
        }

        public ActionResult UpdatePlan(string id)
        {
            string sql = "select * from fy_plan where planid='" + id + "'";
            DataTable dt = PlanBll.GetDataTable(sql);
            //ViewData["dt"] = dt;
            string line = "";
            string post = "";
            if (dt.Rows.Count > 0)
            {
                line = dt.Rows[0]["Line"].ToString();
                post = dt.Rows[0]["PlanContent"].ToString();
            }
            ViewData["line"] = line;
            ViewData["post"] = post;
            return View();
        }



        public int UpdateEventData(string PlanContent, string PlanID, string Line)
        {
            string sql = " select * from FY_PlanDetail where PlanID='" + PlanID + "'  ";
            DataTable dt = PlanBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                //返回-2表示已经有明细数据，不能删除
                return -2;
            }
            StringBuilder strSql = new StringBuilder();
            string UserID = ManageProvider.Provider.Current().UserId;
            string DepartmentID = ManageProvider.Provider.Current().DepartmentId;
            if (DepartmentID == "")
            {
                return -1;
            }
            strSql.AppendFormat("update fy_plan set PlanContent='{0}',Line='{2}' where planid='{1}'", PlanContent, PlanID, Line);
            int result = PlanBll.ExecuteSql(strSql);
            return result;
        }

        public ActionResult ProblemResponse(string id)
        {
            return View();
        }

        public int InsertAction(string problem, string action1, string response, string plandate,string IsRapid)
        {
            if(IsRapid=="否")
            {
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat("insert into FY_ProblemAction (actionid,problemdescripe,actioncontent,responseby,plandate,createby,createbydept,createdt,problemstate) values (NEWID(),'" + problem + "','" + action1 + "','" + response + "','" + plandate + "','" + ManageProvider.Provider.Current().UserId + "','" + ManageProvider.Provider.Current().DepartmentId + "',getdate(),'进行中')");
                int result = PlanBll.ExecuteSql(strSql);

                string GetReciverSql = " select Email from base_user where userid='" + response + "' ";
                DataTable dt = PlanBll.GetDataTable(GetReciverSql);
                if (dt.Rows.Count > 0)
                {
                    MailHelper.SendEmail(dt.Rows[0][0].ToString(), "您好，您的分层审核有一项不合格，请注意登录系统查看");
                }
                return result;
            }
            else
            {
                string code = "";
                string GetReciverSql = " select code from base_user where userid='" + response + "' ";
                DataTable dt = PlanBll.GetDataTable(GetReciverSql);
                if (dt.Rows.Count > 0)
                {
                    code = dt.Rows[0][0].ToString();
                }

                IDatabase database = DataFactory.Database();
                DbTransaction isOpenTrans = database.BeginTrans();
                //FY_Rapid rapid = new FY_Rapid();
                //rapid.Create();
                //rapid.res_cdate = DateTime.Now;

                ////测试下微信公众号消息通知
                //WeChatHelper.SendWxMessage(rapid.res_cpeo, "您好，您有一条新的快速反应需要处理，具体如下：" + rapid.res_ms + "\n 请登录系统处理：172.19.0.5:8086  ");
                ////int IsEmail = SendEmail(rapid.res_cpeo, "您好，您有一条新的快速反应需要处理，具体如下：" + rapid.res_ms + "\n 请登录系统处理：172.19.0.5:8086  ");
                //rapid.IsEmail = 1;
                //rapid.PlanTime = Convert.ToDateTime(plandate);
                //rapid.res_ms = problem;
                //rapid.res_cpeo = code;
                FY_GeneralProblem GPentity = new FY_GeneralProblem();
                GPentity.Create();
                GPentity.ProblemDescripe = problem;
                GPentity.ResponseBy = code;
                GPentity.FollowBy = code;
                GPentity.PlanFinishDt = Convert.ToDateTime(plandate);
                

                WeChatHelper.SendWxMessage(GPentity.ResponseBy, "您好，您有一条新的一般问题需要处理，具体如下：" + GPentity.ProblemDescripe + "\n 请登录系统处理：172.19.0.5:8086  ");

                database.Insert(GPentity, isOpenTrans);
                database.Commit();
                return 1;
            }
            

            
        }
        /// <summary>
        /// result格式：  ID1:结果1;ID2:结果2;
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public int UpdateExamResultAll(string result)
        {
            StringBuilder strSql = new StringBuilder();
            string[] resultArr = result.Split(';');
            for (int i = 0; i < resultArr.Length; i++)
            {
                string[] IdResult = resultArr[i].Split(':');
                strSql.AppendFormat(" update fy_plandetail set ExamResult='{0}',RealCreateby='{2}' where plandid='{1}' ", IdResult[1], IdResult[0], ManageProvider.Provider.Current().UserId);
            }
            int last = PlanBll.ExecuteSql(strSql);
            return last;
        }

        public ActionResult ChangZhangJson()
        {
            string sql = " select UserId,RealName from Base_User a where exists (select * from Base_ObjectUserRelation where ObjectId='54804f22-89a1-4eee-b257-255deaf4face' and UserId=a.UserId) and Enabled=1 ";
            DataTable dt = PlanBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                return Content(dt.ToJson());
            }
            else
            {
                return Content("");
            }
        }

        public int SubmitOtherData(string result)
        {

            //前端有#号后面的值会传不过来，在前端先替换，再在后台操作的时候再替换回来 /(ㄒoㄒ)/~~
            result = result.Replace('|', '#');
            StringBuilder strSql = new StringBuilder();
            string[] resultArr = result.Split(',');
            //for(int i=0;i<resultArr.Length;i++)
            //{
            //    string[] DataArr = resultArr[i].Split(',');
            //    if(DataArr[0]=="")
            //    {
            //        strSql.AppendFormat(" insert into fy_plan (PlanID,UserID,Plandate,PlanContent,DepartmentID,ResponseByID,BackColor,Line) values(newid(),'{0}','{1}','{2}','{3}','{4}','{5}','{6}') ", 
            //            ManageProvider.Provider.Current().UserId, DataArr[4], DataArr[2], ManageProvider.Provider.Current().DepartmentId,
            //            DataArr[1], "",DataArr[3]);
            //    }
            //    else
            //    {
            //        strSql.AppendFormat(" update fy_plan set UserID='{0}',Plandate='{1}',PlanContent='{2}',ResponseByID='{3}',Line='{4}' where PlanID='{5}' ",
            //            ManageProvider.Provider.Current().UserId, DataArr[4], DataArr[2], 
            //            DataArr[1], DataArr[3],DataArr[0]);
            //    }
            //}

            if (resultArr[0] == "")
            {
                strSql.AppendFormat(" insert into fy_plan (PlanID,UserID,Plandate,PlanContent,DepartmentID,ResponseByID,BackColor,Line) values(newid(),'{0}','{1}','{2}','{3}','{4}','{5}','{6}') ",
                    ManageProvider.Provider.Current().UserId, resultArr[4], resultArr[2], ManageProvider.Provider.Current().DepartmentId,
                    resultArr[1], "", resultArr[3]);
            }
            else
            {
                strSql.AppendFormat(" update fy_plan set UserID='{0}',Plandate='{1}',PlanContent='{2}',ResponseByID='{3}',Line='{4}' where PlanID='{5}' ",
                    ManageProvider.Provider.Current().UserId, resultArr[4], resultArr[2],
                    resultArr[1], resultArr[3], resultArr[0]);
            }
            return PlanBll.ExecuteSql(strSql);

        }

        public ActionResult ChejianzhurenJson()
        {
            string sql = " select UserId,RealName from Base_User a where userid='" + ManageProvider.Provider.Current().UserId + "' and Enabled=1 ";
            DataTable dt = PlanBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                return Content(dt.ToJson());
            }
            else
            {
                return Content("");
            }
        }

        /// <summary>
        /// 加载数据，厂长一条，车间主任4条记录
        /// </summary>
        /// <returns></returns>
        public string SetOtherData(string date)
        {
            #region
            string[] dateArr = date.Split(' ');
            string monthEN = dateArr[0];    //传过来的月份是中文，修改成阿拉伯数字
            string month = "";
            switch (monthEN)
            {
                case "一月":
                    month = "1";
                    break;
                case "二月":
                    month = "2";
                    break;
                case "三月":
                    month = "3";
                    break;
                case "四月":
                    month = "4";
                    break;
                case "五月":
                    month = "5";
                    break;
                case "六月":
                    month = "6";
                    break;
                case "七月":
                    month = "7";
                    break;
                case "八月":
                    month = "8";
                    break;
                case "九月":
                    month = "9";
                    break;
                case "十月":
                    month = "10";
                    break;
                case "十一月":
                    month = "11";
                    break;
                case "十二月":
                    month = "12";
                    break;
                default:
                    month = monthEN;
                    break;

            }
            string year = dateArr[1];
            #endregion

            string result1 = "";   //厂长数据字符串
            string result2 = "";  //车间主任数据字符串
            string sql = @" select PlanID,Plandate,PlanContent,ResponseByID,Line from FY_Plan 
where MONTH(Plandate)='" + month + "' and YEAR(Plandate)='" + year + "' and BackColor='' and DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "' " +
                "and ResponseByID!='" + ManageProvider.Provider.Current().UserId + "' ";
            string sqlchejianzhuren = @" select PlanID,Plandate,PlanContent,ResponseByID,Line from FY_Plan 
where MONTH(Plandate)='" + month + "' and YEAR(Plandate)='" + year + "' and BackColor='' and DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "' " +
                "and ResponseByID='" + ManageProvider.Provider.Current().UserId + "'";
            DataTable dt1 = PlanBll.GetDataTable(sql);  //厂长数据
                                                        //DataTable dt2 = PlanBll.GetDataTable(sqlchejianzhuren);  //车间主任数据

            if (dt1.Rows.Count > 0)
            {
                result1 += dt1.Rows[0][0].ToString() + "," + dt1.Rows[0]["ResponseByID"].ToString() + "," + dt1.Rows[0]["PlanContent"].ToString() + "," +
                    dt1.Rows[0]["Line"].ToString() + "," + dt1.Rows[0]["Plandate"].ToString();
            }
            else
            {
                result1 += ",,,,";
            }
            //if(dt2.Rows.Count>0)
            //{

            //    for(int i=0;i<dt2.Rows.Count;i++)
            //    {
            //        result2+= dt2.Rows[i][0].ToString() + "," + dt2.Rows[i]["ResponseByID"].ToString() + "," + dt2.Rows[i]["PlanContent"].ToString() + "," +
            //        dt2.Rows[i]["Line"].ToString() + "," + dt2.Rows[i]["Plandate"].ToString()+"|";
            //    }
            //    result2 = result2.Substring(0, result2.Length - 1);
            //}
            //else
            //{ result2 += ",,,,|,,,,|,,,,|,,,,"; }
            return result1;
        }

        #region 分层审核报表部分
        /// <summary>
        /// 符合率报表，时间段内Y的数量除以时间段内所有检查的数量
        /// </summary>
        /// <returns></returns>
        public ActionResult CompleteRate()
        {
            return View();
        }

        public ActionResult GetCompleteRate(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam
            , string year, string department)
        {
            if (year == null || year == "undefined" || year == "")
            {
                year = DateTime.Now.Year.ToString();
            }
            if (department == null || department == "undefined")
            {
                department = "";
            }
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PlanBll.GetMonthlyPostRate(keywords, ref jqgridparam, year, department);
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

        //查询条件，部门/车间下拉框绑定
        public ActionResult DepartmentJson()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select distinct a.DepartmentID,b.FullName,sortcode from FY_Plan a left join Base_Department b on a.DepartmentID=b.DepartmentId order by sortcode ");
            DataTable dt = PlanBll.GetDataTable(strSql.ToString());
            return Content(dt.ToJson());
        }

        //添加了厂长管理的部门
        public ActionResult CZDepartmentJson()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"select distinct a.DepartmentID,b.FullName from FY_Plan 
a left join Base_Department b on a.DepartmentID=b.DepartmentId where 1=1
", ManageProvider.Provider.Current().UserId);
            DataTable dt = PlanBll.GetDataTable(strSql.ToString());
            return Content(dt.ToJson());
        }
        /// <summary>
        /// 增加总经办的部门下拉框
        /// </summary>
        /// <returns></returns>
        public ActionResult DepartmentJson2()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"select * from ( select distinct b.DepartmentID,b.FullName,b.SortCode from FY_Plan a left join Base_Department b on a.DepartmentID=b.DepartmentId 
--union select DepartmentId,FullName,'1000' from Base_Department where DepartmentId='159df668-e428-4dcb-9a71-c7cdeabdeb03' 
union select '','监督人员','0' ) as a order by sortcode ");
            DataTable dt = PlanBll.GetDataTable(strSql.ToString());
            return Content(dt.ToJson());
        }

        public ActionResult GroupJson(string DepartmentId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select distinct GroupUserId,FullName from Base_GroupUser where DepartmentId='"+DepartmentId+"'");
            DataTable dt = PlanBll.GetDataTable(strSql.ToString());
            return Content(dt.ToJson());
        }
        //public ActionResult AllDepartMentJson()
        //{

        //}
        /// <summary>
        /// 按适用过程（系统中的工序表中的适用过程）分类的适用率报表
        /// </summary>
        /// <returns></returns>
        public ActionResult ProcessCompleteRateReport()
        {
            return View();
        }

        public ActionResult GetProcessCompleteRate(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam
            , string year, string department)
        {
            if (year == null || year == "undefined" || year == "")
            {
                year = DateTime.Now.Year.ToString();
            }
            if (department == null || department == "undefined")
            {
                department = "";
            }
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PlanBll.GetMonthlyProcessRate(keywords, ref jqgridparam, year, department);
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

        public ActionResult SperateReport()
        {
            return View();
        }

        public string GetSperateReport(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam
            , string startDate, string endDate)
        {
            string result = "";
            string tempX = "";
            string tempY1 = "";
            string tempY2 = "";
            string tempY3 = "";
            string tempY4 = "";
            DateTime now = DateTime.Now;
            DateTime d1 = new DateTime(now.Year, now.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);
            if (startDate == "undefined" || startDate == "" || startDate == null)
            {
                startDate = d1.ToString();
            }
            if (endDate == "undefined" || endDate == "" || endDate == null)
            {
                endDate = d2.ToString();
            }

            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PlanBll.GetSperateReport(keywords, ref jqgridparam, startDate, endDate);
                //var JsonData = new
                //{
                //    total = jqgridparam.total,
                //    page = jqgridparam.page,
                //    records = jqgridparam.records,
                //    costtime = CommonHelper.TimerEnd(watch),
                //    rows = ListData,
                //};
                if(ListData.Rows.Count>0)
                {
                    for(int i=0;i<ListData.Rows.Count;i++)
                    {
                        tempX += ListData.Rows[i]["fullname"].ToString() + ",";
                        tempY1 += ListData.Rows[i]["BZrate"].ToString() + ",";
                        tempY2 += ListData.Rows[i]["CJZRrate"].ToString() + ",";
                        tempY3 += ListData.Rows[i]["CZRrate"].ToString() + ",";
                        tempY4 += ListData.Rows[i]["CJrate"].ToString() + ",";
                    }
                    tempX = tempX.Substring(0, tempX.Length - 1);
                    tempY1 = tempY1.Substring(0, tempY1.Length - 1);
                    tempY2 = tempY2.Substring(0, tempY2.Length - 1);
                    tempY3 = tempY3.Substring(0, tempY3.Length - 1);
                    tempY4 = tempY4.Substring(0, tempY4.Length - 1);
                }
                result = tempX + "|" + tempY1 + "|" + tempY2 + "|" + tempY3 + "|" + tempY4;
                return result;
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return "";
            }
        }

        public ActionResult PersonalCompleteRate()
        {
            return View();
        }

        public String GetPersonalCompleteRate(string DepartmentID,string StartDate,string EndDate)
        {
            string result = "";
            string tempX = "";
            string tempY = "";

            DateTime now = DateTime.Now;
            DateTime d1 = new DateTime(now.Year, now.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);
            if (StartDate == "undefined" || StartDate == "" || StartDate == null)
            {
                StartDate = d1.ToString();
            }
            if (EndDate == "undefined" || EndDate == "" || EndDate == null)
            {
                EndDate = d2.ToString();
            }
            DataTable ListData = PlanBll.GetPersonalCompleteRate(DepartmentID,StartDate,EndDate);
            if(ListData.Rows.Count>0)
            {
                for(int i=0;i<ListData.Rows.Count;i++)
                {
                    tempX += ListData.Rows[i]["RealName"].ToString()+",";
                    tempY += ListData.Rows[i]["completerate"].ToString()+",";
                }
                tempX = tempX.Substring(0, tempX.Length - 1);
                tempY = tempY.Substring(0, tempY.Length - 1);
                result = tempX + "|" + tempY;
            }
            return result;

        }

        public ActionResult DetialForm(string Name,string startDate,string endDate)
        {
            DateTime now = DateTime.Now;
            DateTime d1 = new DateTime(now.Year, now.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);
            if (startDate == "undefined" || startDate == "" || startDate == null)
            {
                startDate = d1.ToString();
            }
            if (endDate == "undefined" || endDate == "" || endDate == null)
            {
                endDate = d2.ToString();
            }
            string sql = @"select CONVERT(varchar(100), Plandate, 120),PlanContent,RealName,ApproveStatus,detailcount from PersonalSperateCompleteDetail where cast(plandate as datetime)>=cast('" + startDate + "' as datetime) and cast(plandate as datetime)<=cast('" + endDate + "' as datetime) and realname='"+Name+ "' order by plandate ";
            DataTable dt = PlanBll.GetDataTable(sql);
            ViewData["dt"] = dt;
            return View();
        }

        public string GetUnqualifiedCount(string DepartmentID,string startDate,string endDate)
        {
            string result = "";
            string tempX = "";
            string tempY = "";

            DateTime now = DateTime.Now;
            DateTime d1 = new DateTime(now.Year, now.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);
            if (startDate == "undefined" || startDate == "" || startDate == null)
            {
                startDate = d1.ToString();
            }
            if (endDate == "undefined" || endDate == "" || endDate == null)
            {
                endDate = d2.ToString();
            }
            string sql = @"select count(*),c.RealName from fy_plan a left join FY_PlanDetail b on a.planid=b.PlanID
left join Base_User c on b.RealCreateby=c.UserId
where b.ExamResult='N' and RealName is not null and cast(plandate as datetime)>=cast('" + startDate + "' as datetime) and cast(plandate as datetime)<=cast('" + endDate + "' as datetime) and c.departmentid='"+DepartmentID+"' group by c.RealName";

            DataTable dt = PlanBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                for(int i=0;i<dt.Rows.Count;i++)
                {
                    tempX += dt.Rows[i][1].ToString() + ",";
                    tempY += dt.Rows[i][0].ToString() + ",";
                }
                tempX = tempX.Substring(0, tempX.Length - 1);
                tempY = tempY.Substring(0, tempY.Length - 1);
                result = tempX + "|" + tempY;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 为了不刷新页面直接显示数据，提供的另一种获取数据方式
        /// </summary>
        /// <param name="Date"></param>
        /// <returns></returns>
        public string GetAjaxEventData(string Date)
        {

            string[] dateArr = Date.Split(' ');
            string monthEN = dateArr[0];    //js的时间获取的月份是英文简写，需再转一次
            string month = "";
            switch (monthEN)
            {
                case "一月":
                    month = "1";
                    break;
                case "二月":
                    month = "2";
                    break;
                case "三月":
                    month = "3";
                    break;
                case "四月":
                    month = "4";
                    break;
                case "五月":
                    month = "5";
                    break;
                case "六月":
                    month = "6";
                    break;
                case "七月":
                    month = "7";
                    break;
                case "八月":
                    month = "8";
                    break;
                case "九月":
                    month = "9";
                    break;
                case "十月":
                    month = "10";
                    break;
                case "十一月":
                    month = "11";
                    break;
                case "十二月":
                    month = "12";
                    break;
                default:
                    month = monthEN;
                    break;

            }
            string year = dateArr[1];

            string id = ManageProvider.Provider.Current().ObjectId;
            string sql = @"select PlanID,Plandate,case  when exists (select * from FY_PlanDetail where PlanID=a.PlanID) and not exists (select * from FY_PlanDetail where PlanID=a.PlanID and ExamResult='') then '已完成' when IsLeave=0 then PlanContent else '休息' end as PlanContent,a.BackColor from fy_plan a 
left join Base_User b on a.ResponseByID=b.UserId left join Base_GroupUser c on a.groupid=c.GroupUserId
where a.BackColor!='' and a.DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "' " +
//"and plandate>='"+year+"-"+month+"-01"+
"and year(plandate)=" + year + " and month(plandate)=" + month + " " +
" order by Plandate,BackColor ";
            DataTable dt = PlanBll.GetDataTable(sql);
            string result = "";
            string temp1 = "";
            string temp2 = "";
            string temp3 = "";
            string temp4 = "";
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    temp1 = temp1 + dt.Rows[i][0] + ",";
                    temp2 = temp2 + dt.Rows[i][1] + ",";
                    temp3 = temp3 + dt.Rows[i][2] + ",";
                    temp4 = temp4 + dt.Rows[i][3] + ",";
                }
                temp1 = temp1.Substring(0, temp1.Length - 1);
                temp2 = temp2.Substring(0, temp2.Length - 1);
                temp3 = temp3.Substring(0, temp3.Length - 1);
                temp4 = temp4.Substring(0, temp4.Length - 1);
            }
            result = temp1 + "|" + temp2 + "|" + temp3 + "|" + temp4;
            return result;
        }

        public ActionResult Delete(string KeyValue)
        {
            var Message = "删除失败。";
            int IsOk = 0;
            string sql = "select * from fy_plandetail where planid='"+KeyValue+ "' and ExamResult!='' ";
            DataTable dt = PlanBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                Message = "已有审核记录，不能删除！";
            }
            else
            {
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@"delete from fy_plan where planid='{0}' delete from fy_plandetail where planid='{0}' ", KeyValue);

                IsOk = PlanBll.ExecuteSql(strSql);
                if (IsOk > 0)
                {
                    Message = "删除成功。";
                }
            }

            
            return Content(new JsonMessage { Success = true, Code = IsOk.ToString(), Message = Message }.ToString());
        }

        public ActionResult ProblemAction()
        {
            return View();
        }
        public string GetProblemActionData(string year)
        {
            string result = "";
            string temp1 = "";
            string temp2 = "";
            string temp3 = "";
            string temp4 = "";
            //string sql = " select count(*) as rapidcount,MONTH(res_cdate) as month from FY_Rapid where YEAR(res_cdate)='"+year+"' group by MONTH(res_cdate),YEAR(res_cdate)  ";
            string sql = @"select isnull(problemcount,0),ISNULL(okcount,0),basicmonth,case when ISNULL(okcount,0)=0 then 0 else cast(100.0*ISNULL(okcount,0)/isnull(problemcount,0) as decimal(18,2)) end as rate
from
Base_Month a 
left join (select count(*) as problemcount,MONTH(createdt) as month 
from FY_ProblemAction where YEAR(createdt)='{0}' group by MONTH(createdt),YEAR(createdt)) as b
on a.BasicMonth=b.month
left join  (select count(*) as okcount,MONTH(createdt) as month 
from FY_ProblemAction where YEAR(createdt)='{0}' 
and ProblemState='已完成'
group by MONTH(createdt),YEAR(createdt)) as c
on a.BasicMonth=c.month ";
            sql = string.Format(sql, year);
            DataTable dt = PlanBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    temp1 = temp1 + dt.Rows[i][0] + ",";
                    temp2 = temp2 + dt.Rows[i][1] + ",";
                    temp3 = temp3 + dt.Rows[i][2] + ",";
                    temp4 = temp4 + dt.Rows[i][3] + ",";
                }
                temp1 = temp1.Substring(0, temp1.Length - 1);
                temp2 = temp2.Substring(0, temp2.Length - 1);
                temp3 = temp3.Substring(0, temp3.Length - 1);
                temp4 = temp4.Substring(0, temp4.Length - 1);
            }
            result = temp1 + "|" + temp2 + "|" + temp3+"|"+temp4;


            return result;
        }


        public ActionResult YearJson()
        {
            string sql= " select distinct(year(createdt)) as year from FY_ProblemAction ";
            DataTable dt = PlanBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult ProblemActionByResponseUnit()
        {
            return View();
        }


        public string GetProblemActionDataByResponse(string StartDate,string EndDate)
        {
            string result = "";
            string temp1 = "";
            string temp2 = "";
            string temp3 = "";
            string temp4 = "";
            //string sql = " select count(*) as rapidcount,MONTH(res_cdate) as month from FY_Rapid where YEAR(res_cdate)='"+year+"' group by MONTH(res_cdate),YEAR(res_cdate)  ";
            string sql = @" select count(*),
(select count(*) from FY_ProblemAction aa 
left join Base_User bb on aa.ResponseBy=bb.UserId
left join Base_Department cc on bb.DepartmentId=cc.DepartmentId
where cast(aa.CreateDt as date)>= cast('{0}' as date) 
and cast(aa.CreateDt as date)<=cast('{1}' as date) 
and ProblemState='已完成'
and cc.DepartmentId=c.DepartmentId),c.FullName
 from FY_ProblemAction a 
left join Base_User b on a.ResponseBy=b.UserId
left join Base_Department c on b.DepartmentId=c.DepartmentId
where cast(a.CreateDt as date)>= cast('{0}' as date) 
and cast(a.CreateDt as date)<=cast('{1}' as date) 
group by c.FullName,c.DepartmentId ";

            sql = string.Format(sql, StartDate, EndDate);
            DataTable dt = PlanBll.GetDataTable(sql);
           
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    temp1 = temp1 + dt.Rows[i][0] + ",";
                    temp2 = temp2 + dt.Rows[i][1] + ",";
                    temp3 = temp3 + dt.Rows[i][2] + ",";
                   
                }
                temp1 = temp1.Substring(0, temp1.Length - 1);
                temp2 = temp2.Substring(0, temp2.Length - 1);
                temp3 = temp3.Substring(0, temp3.Length - 1);
                
            }
            result = temp1 + "|" + temp2 + "|" + temp3;


            return result;
        }

        public string GetProblemActionDataByCreate(string StartDate, string EndDate)
        {
            string result = "";
            string temp1 = "";
            string temp2 = "";
            string temp3 = "";
            string temp4 = "";
            //string sql = " select count(*) as rapidcount,MONTH(res_cdate) as month from FY_Rapid where YEAR(res_cdate)='"+year+"' group by MONTH(res_cdate),YEAR(res_cdate)  ";
            string sql = @"select count(*),
(select count(*) from FY_ProblemAction aa 
left join Base_User bb on aa.CreateBy=bb.UserId
left join Base_Department cc on bb.DepartmentId=cc.DepartmentId
where cast(aa.CreateDt as date)>= cast('{0}' as date) 
and cast(aa.CreateDt as date)<=cast('{1}' as date) 
and ProblemState='已完成'
and cc.DepartmentId=c.DepartmentId),c.FullName
 from FY_ProblemAction a 
left join Base_User b on a.CreateBy=b.UserId
left join Base_Department c on b.DepartmentId=c.DepartmentId
where cast(a.CreateDt as date)>= cast('{0}' as date) 
and cast(a.CreateDt as date)<=cast('{1}' as date) 
group by c.FullName,c.DepartmentId  ";

            sql = string.Format(sql, StartDate, EndDate);
            DataTable dt = PlanBll.GetDataTable(sql);

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    temp1 = temp1 + dt.Rows[i][0] + ",";
                    temp2 = temp2 + dt.Rows[i][1] + ",";
                    temp3 = temp3 + dt.Rows[i][2] + ",";

                }
                temp1 = temp1.Substring(0, temp1.Length - 1);
                temp2 = temp2.Substring(0, temp2.Length - 1);
                temp3 = temp3.Substring(0, temp3.Length - 1);

            }
            result = temp1 + "|" + temp2 + "|" + temp3;


            return result;
        }


        public int CheckIsManager(string ResponseBy)
        {
            string sql = @" select * from Base_User  a

where exists(select* from Base_ObjectUserRelation where UserId= a.UserId and ObjectId in ('91c17ca4-0cbf-43fa-829e-3021b055b6c4','f6afd4e4-6fb2-446f-88dd-815ddb91b09d')) 
and a.userid='{0}'
 ";
            sql = string.Format(sql, ResponseBy);
            DataTable dt = PlanBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

    }
}
