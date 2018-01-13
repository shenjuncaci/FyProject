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
    public class TC_ShortContractBll : RepositoryFactory<TC_ShortContract>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select * from TC_ShortContract where 1=1 and Enable=1  ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (productname LIKE @keyword
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

        //public List<TC_AttachContractDetail> GetDetailList(string KeyValue)
        //{
        //    StringBuilder strSql = new StringBuilder();
        //    List<DbParameter> parameter = new List<DbParameter>();
        //    strSql.Append(@"select * from TC_AttachContractDetail WHERE 1=1");
        //    strSql.Append(" AND ContractID = @ContractID order by SortNO ");
        //    parameter.Add(DbFactory.CreateDbParameter("@ContractID", KeyValue));
        //    return DataFactory.Database().FindListBySql<TC_AttachContractDetail>(strSql.ToString(), parameter.ToArray());
        //}
    }
}
