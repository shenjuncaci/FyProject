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
    public class FY_ProcessBll : RepositoryFactory<FY_ProcessBll>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson,string type)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from fy_process where 1=1  ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (ProcessName LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            if(type=="1")
            {
                strSql.AppendFormat(" and ProcessName in ('通用','系统监督') ");

            }
            else
            {
                strSql.AppendFormat(" and DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "' and ProcessName not in ('通用','系统监督') ");
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
