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
using System.Text;

namespace LeaRun.Business
{
    public class FY_PlanBll : RepositoryFactory<FY_PlanBll>
    {
        public DataTable GetDataTable(string sql)
        {
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public int ExecuteSql(StringBuilder sql)
        {
            int result = Repository().ExecuteBySql(sql);
            return result;
        }

        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            bool IsLeader = false;  //判断是否车间主任，有车间主任权限可以显示当前日期之前所有未审核完的记录
            bool IsBz = false; //加一个角色的判断，用来判断是否同时显示车间主任和车间班长的审批内容
            IManageUser user = ManageProvider.Provider.Current();
            string[] objectID = user.ObjectId.Split(',');
            for (int i = 0; i < objectID.Length; i++)
            {
                if (objectID[i] == "91c17ca4-0cbf-43fa-829e-3021b055b6c4"||objectID[i]== "54804f22-89a1-4eee-b257-255deaf4face"|| objectID[i]== "8431ea77-69c9-484c-a67f-4f3419c0d393")
                {
                    IsLeader = true;
                }
                if(objectID[i]== "f6afd4e4-6fb2-446f-88dd-815ddb91b09d")
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
               "GETDATE()<=cast(cast( case when c.duty='白班' then a.Plandate else DATEADD(day,1,a.Plandate) end as nvarchar(30))+' '+c.EndTime as datetime) "+
               " and not exists (select * from FY_PlanDetail where planid=a.planid and ExamResult!='')");
                

            }
            else
            {
                strSql.Append(@" select PlanID,Plandate,PlanContent,b.RealName,a.line from fy_plan a left join Base_User b on a.ResponseByID=b.UserId 
left join Base_GroupUser c on c.GroupUserId=a.groupid where PlanContent!='' and Plandate=cast(GETDATE() as date) and ResponseByID='" + ManageProvider.Provider.Current().UserId + "' and (a.BackColor='' or a.BackColor='grey' ) and not exists (select * from FY_PlanDetail where planid=a.planid and ExamResult!='') ");
                if(IsBz==true)
                {
                    strSql.Append(@" union select PlanID,Plandate,PlanContent,b.RealName,a.line from fy_plan a left join Base_User b on a.ResponseByID=b.UserId
             left join Base_GroupUser c on c.GroupUserId=a.groupid
where PlanContent!='' and  IsLeave=0  and a.BackColor!='' and a.BackColor!='grey' and
           a.DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "' and   GETDATE()>=cast(cast(a.plandate as nvarchar(30))+' '+c.StartTime as datetime) and " +
               "GETDATE()<=cast(cast( case when c.duty='白班' then a.Plandate else DATEADD(day,1,a.Plandate) end as nvarchar(30))+' '+c.EndTime as datetime) and not exists (select * from FY_PlanDetail where planid=a.planid and ExamResult!='') ");
                }
            }
           
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (RealName LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetEventList()
        {
            string sql = " select distinct ProcessName from FY_Process where DepartmentID='"+ManageProvider.Provider.Current().DepartmentId+"' and processname!='通用' ";
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public DataTable GetResponseByList()
        {
            string sql = " select distinct a.UserId,a.RealName+'('+a.Code+')' as RealName from Base_User a  where a.DepartmentId='" + ManageProvider.Provider.Current().DepartmentId+ "' and Enabled=1 ";
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public DataTable GetResponseAllByList()
        {
            string sql = " select distinct a.UserId,a.RealName+'('+a.Code+')' as RealName from Base_User a  where 1=1 and Enabled=1 ";
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public DataTable GetReportList(string keyword, ref JqGridParam jqgridparam, string ParameterJson,
            string startDate,string endDate,string UserMan)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select e.fullname,d.RealName,a.Plandate,a.PlanContent,b.AbleProcess,
b.AuditContent,b.FailureEffect,b.ProcessName,b.ExamResult
from FY_Plan a
left join FY_PlanDetail b on a.PlanID=b.PlanID
left join FY_Process c on b.ProcessID=c.ProcessID
left join Base_User d on b.RealCreateby=d.UserId 
left join base_department e on e.departmentid=a.departmentid where 1=1 ");
            if (!string.IsNullOrEmpty(keyword)&&keyword!="undefined")
            {
                strSql.Append(@" AND (ProcessName LIKE @keyword 
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(startDate))
            {
                strSql.AppendFormat(" and a.Plandate>='{0}' ",startDate);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                strSql.AppendFormat(" and a.Plandate<='{0}' ", endDate);
            }
            if (!string.IsNullOrEmpty(UserMan))
            {
                strSql.AppendFormat(" and d.RealName like '%{0}%' ", UserMan);
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetMonthlyPostRate(string keyword, ref JqGridParam jqgridparam,string year,string department)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
           
            StringBuilder proc = new StringBuilder();
            

            //Repository().ExecuteBySql(proc);
            //if (!string.IsNullOrEmpty(keyword))
            //{

            //parameter.Add(DbFactory.CreateDbParameter("@keyword", 2017));
            //}
            parameter.Add(DbFactory.CreateDbParameter("@Year", year));
            parameter.Add(DbFactory.CreateDbParameter("@Department", department));

            return Repository().FindDataSetByProc("SeparateAuditReport", parameter.ToArray()).Tables[0];
        }

        public DataTable GetMonthlyProcessRate(string keyword, ref JqGridParam jqgridparam, string year, string department)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();

            StringBuilder proc = new StringBuilder();


            //Repository().ExecuteBySql(proc);
            //if (!string.IsNullOrEmpty(keyword))
            //{

            //parameter.Add(DbFactory.CreateDbParameter("@keyword", 2017));
            //}
            parameter.Add(DbFactory.CreateDbParameter("@Year", year));
            parameter.Add(DbFactory.CreateDbParameter("@DepartmentID", department));

            return Repository().FindDataSetByProc("SAProcessReport", parameter.ToArray()).Tables[0];
        }

        public DataTable GetSperateReport(string keyword, ref JqGridParam jqgridparam,
            string startdate,string enddate)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(DbFactory.CreateDbParameter("@StartDate", startdate));
            parameter.Add(DbFactory.CreateDbParameter("@EndDate", enddate));
            return Repository().FindDataSetByProc("BZCompleteRate", parameter.ToArray()).Tables[0];
        }

        public DataTable GetPersonalCompleteRate(string DepartmentID,string StartDate,string EndDate)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(DbFactory.CreateDbParameter("@DepartmentID", DepartmentID));
            parameter.Add(DbFactory.CreateDbParameter("@StartDate", StartDate));
            parameter.Add(DbFactory.CreateDbParameter("@EndDate", EndDate));
            //完成率改为完成次数
            //return Repository().FindDataSetByProc("PersonalCompleteRate", parameter.ToArray()).Tables[0];
            
            return Repository().FindDataSetByProc("PersonalCompleteCount", parameter.ToArray()).Tables[0];
        }



    }
}
