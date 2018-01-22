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
    public class FY_ProblemTrackBll : RepositoryFactory<FY_ProblemTrack>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select a.*,b.RealName as ResponseName ,c.FullName as departmentname,
b1.RealName as CreateName,c1.FullName as CreateDept,
(select top 1 PlanDt from FY_ProblemTrackDetail where ProblemID=a.ProblemID order by PlanDt desc) as Plandt,
(select top 1 Progress from FY_ProblemTrackDetail where ProblemID=a.ProblemID order by PlanDt desc) as Progress,
case when a.createby='{0}' then 1 else 0 end as IsSame
from FY_ProblemTrack a 
left join Base_User b on a.ResponseBy=b.UserId
left join Base_Department c on b.DepartmentId=c.DepartmentId 
left join Base_User b1 on a.CreateBy=b1.UserId
left join Base_Department c1 on b1.DepartmentId=c1.DepartmentId 
where 1=1
  ",ManageProvider.Provider.Current().UserId);
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
            }

            if (ManageProvider.Provider.Current().ObjectId.IndexOf("f4633511-fa82-4ded-8fd5-ec68f60f47e3") < 0)
            {
                strSql.AppendFormat(@" and (a.CreateBy='{0}' or  a.ResponseBy='{0}')  ", ManageProvider.Provider.Current().UserId);
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

        public List<FY_ProblemTrackDetail> GetDetailList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from FY_ProblemTrackDetail WHERE 1=1");
            strSql.Append(" AND ProblemID = @ProblemID order by CreateDt ");
            parameter.Add(DbFactory.CreateDbParameter("@ProblemID", KeyValue));
            return DataFactory.Database().FindListBySql<FY_ProblemTrackDetail>(strSql.ToString(), parameter.ToArray());
        }
        public DataTable GetFileList(string keyword, ref JqGridParam jqgridparam, string ParameterJson, string SkillID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select * from FY_ProblemTrackFile where ProblemID='{0}'  ", SkillID);
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
    }

}
