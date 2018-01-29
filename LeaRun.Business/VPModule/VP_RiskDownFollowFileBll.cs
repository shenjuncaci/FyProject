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
    public class VP_RiskDownFollowFileBll : RepositoryFactory<VP_RiskDownFollowFile>
    {
//        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
//        {
//            StringBuilder strSql = new StringBuilder();
//            List<DbParameter> parameter = new List<DbParameter>();
//            strSql.Append(@" select a.*,b.realname from VP_RiskDownFollow a 
//left join base_user b on a.ResponseBy=b.UserID 
//where 1=1   ");
//            if (!string.IsNullOrEmpty(keyword))
//            {
//                //strSql.Append(@" AND (ReviewContent LIKE @keyword
//                //                    )");
//                //parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
//            }
//            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
//            {
//                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
//            }
//            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
//        }

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
