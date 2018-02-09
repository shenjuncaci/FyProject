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
    public class VP_RiskDownFollowBll : RepositoryFactory<VP_RiskDownFollow>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson,string ResponseBy)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.*,b.realname,
case when exists (select * from VP_RiskDownFollowFile where FollowID=a.followid) then '查看' else '尚未上传' end as evidence
from VP_RiskDownFollow a 
left join base_user b on a.ResponseBy=b.UserID 
where 1=1   ");
            if (!string.IsNullOrEmpty(keyword))
            {
                //strSql.Append(@" AND (ReviewContent LIKE @keyword
                //                    )");
                //parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
                strSql.Append(keyword);
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            if(!string.IsNullOrEmpty(ResponseBy))
            {
                strSql.AppendFormat(@" and a.ResponseBy='{0}' ",ResponseBy);
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
