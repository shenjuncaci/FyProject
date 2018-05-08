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
    public class FY_ProduceLineBll : RepositoryFactory<FY_ProduceLineBll>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select *,(SELECT '[ '+PostName+' ]' FROM FY_LinePost where lineid=a.lineid FOR XML PATH('') ) as postname from FY_ProduceLine a where 1=1 and DepartmentID='" + ManageProvider.Provider.Current().DepartmentId+"' ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (LineName LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
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

        public List<FY_LinePost> GetDetailList(string LineID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"SELECT * FROM FY_LinePost WHERE 1=1");
            strSql.Append(" AND LineID = @LineID  ");
            parameter.Add(DbFactory.CreateDbParameter("@LineID", LineID));
            return DataFactory.Database().FindListBySql<FY_LinePost>(strSql.ToString(), parameter.ToArray());
        }
    }
}
