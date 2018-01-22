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
    public class FY_HrProblemBll : RepositoryFactory<FY_HrProblem>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson,string type,string IsMy)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select dbo.GetState(a.ProblemID) as DetailState,a.*,b.RealName,c.RealName as createman,(select top 1 FullName from Base_Department where DepartmentId=b.DepartmentId) as responsedepart,(select top 1 FullName from Base_Department where DepartmentId=c.DepartmentId) as createdepart
from fy_hrproblem a 
left join Base_User b on a.ResponseBy=b.UserId 
left join Base_User c on a.CreateBy=c.UserId 
where 1=1  ");
            if(type=="1")
            {
                strSql.Append(" and ProblemType='员工咨询' ");
            }
            if(type=="2")
            {
                strSql.Append(" and ProblemType='心声反馈' ");
            }
            if(type=="3")
            {
                strSql.Append(" and ProblemType='员工投诉' ");
            }
            if(type=="4")
            {
                strSql.Append(" and ProblemType='外部咨询' ");
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (ProblemDescripe LIKE @keyword or b.RealName like @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }

            if(IsMy=="1")
            {
                strSql.AppendFormat(@" and exists (select * from Base_FlowLog where NoteID=a.ProblemID and CurrentPerson='{0}') ",ManageProvider.Provider.Current().UserId);
            }

            if(ManageProvider.Provider.Current().ObjectId.IndexOf("a69ae568-3aa4-4024-8120-9cf7527902ae")<0)
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
    }
}
