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
    public class PM_ProjectBll : RepositoryFactory<PM_Project>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select case when a.isend=1 then '已结案' else dbo.GetFlowState(a.flowid) end as flowstate,a.*,b.fullname 
from pm_project a left join base_department b on a.departmentid=b.departmentid where 1=1   ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetDataTable(string sql)
        {
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public int ExecuteSql(StringBuilder sql)
        {
            int result = Repository().ExecuteBySql(sql);
            return result;
        }

        public List<PM_ProjectMember> GetDetailList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select a.*,b.realname from PM_ProjectMember a left join Base_user b on a.userid=b.userid where a.ProjectID=@ProjectID");
            //strSql.Append(" AND CustomExamID = @CustomExamID and IsEnabe=1 order by SortNO ");
            parameter.Add(DbFactory.CreateDbParameter("@ProjectID", KeyValue));
            return DataFactory.Database().FindListBySql<PM_ProjectMember>(strSql.ToString(), parameter.ToArray());
        }

        public List<PM_ProjectActivity> GetActivityList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from PM_ProjectActivity where ProjectID=@ProjectID  order by ActivityDate ");
            //strSql.Append(" AND CustomExamID = @CustomExamID and IsEnabe=1 order by SortNO ");
            parameter.Add(DbFactory.CreateDbParameter("@ProjectID", KeyValue));
            return DataFactory.Database().FindListBySql<PM_ProjectActivity>(strSql.ToString(), parameter.ToArray());
        }

        public List<PM_ProjectProblem> GetProblemList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from PM_ProjectProblem where ProjectID=@ProjectID  order by SortNO ");
            //strSql.Append(" AND CustomExamID = @CustomExamID and IsEnabe=1 order by SortNO ");
            parameter.Add(DbFactory.CreateDbParameter("@ProjectID", KeyValue));
            return DataFactory.Database().FindListBySql<PM_ProjectProblem>(strSql.ToString(), parameter.ToArray());
        }

        public DataTable GetUserList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.*,b.fullname from base_user a left join base_department b on a.departmentid=b.departmentid where 1=1   ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }


        public DataTable GetProfitList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select a.*,b.fullname from pm_project a left join base_department b on a.departmentid=b.departmentid 
where 1=1 and  DataProvider='{0}'  ",ManageProvider.Provider.Current().UserId);
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public List<PM_ProjectProfit> GetProfitDetailList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from pm_projectProfit where ProjectID=@ProjectID order by ProfitDate ");
            //strSql.Append(" AND CustomExamID = @CustomExamID and IsEnabe=1 order by SortNO ");
            parameter.Add(DbFactory.CreateDbParameter("@ProjectID", KeyValue));
            return DataFactory.Database().FindListBySql<PM_ProjectProfit>(strSql.ToString(), parameter.ToArray());
        }

        public List<PM_ProjectPlan> GetProjectPlanList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> paramter = new List<DbParameter>();
            strSql.Append(@"select * from PM_ProjectPlan where ProjectID=@ProjectID order by PlanStartDate");
            paramter.Add(DbFactory.CreateDbParameter("@ProjectID", KeyValue));
            return DataFactory.Database().FindListBySql<PM_ProjectPlan>(strSql.ToString(), paramter.ToArray());
        }

        public List<PM_ProjectTarget> GetProjectTargetList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> paramter = new List<DbParameter>();
            strSql.Append(@"select * from PM_ProjectTarget where ProjectID=@ProjectID order by TargetContent");
            paramter.Add(DbFactory.CreateDbParameter("@ProjectID", KeyValue));
            return DataFactory.Database().FindListBySql<PM_ProjectTarget>(strSql.ToString(), paramter.ToArray());
        }

    }
}
