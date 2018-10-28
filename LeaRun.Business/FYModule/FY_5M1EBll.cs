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
    public class FY_5M1EBLL : RepositoryFactory<FY_5M1E>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select  a.id,a.processname,a.createdate,b.FullName as bangroup,a.changepoint,
a.changecontent,a.changereason,a.changelevel,a.changeaction,a.enddate,d.RealName as endby,
(select top 1 ProblemDescripe from FY_ProblemAction  where PlanDID in (select plandid from FY_Plan aa left join FY_PlanDetail bb on aa.PlanID=bb.PlanID where bb.ProcessID=a.id) and createbydept='{0}') as problemdescripe,
a.GCKEndBy
from FY_5M1E a 
left join Base_GroupUser b  on a.bangroup=b.GroupUserId
left join Base_User c on a.createby=c.UserId
left join Base_User d on a.endby=d.UserId

where 1=1  ", ManageProvider.Provider.Current().DepartmentId);
            if(ManageProvider.Provider.Current().ObjectId.IndexOf("c2a1ed38-01f5-4311-902d-afc98bec3ad9")<0)
            {
                strSql.AppendFormat(" and a.departmentid='{0}' ", ManageProvider.Provider.Current().DepartmentId);
            }
            //if (!string.IsNullOrEmpty(keyword))
            //{
            //    strSql.Append(@" AND (fy_cus_name LIKE @keyword
            //                        )");
            //    parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            //}
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
