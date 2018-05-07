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
    public class DM_RoomRepairBll : RepositoryFactory<DM_RoomRepair>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.*,b.DormName from dm_roomrepair a left join DM_Dorm b on a.dormID=b.DormID 
where 1=1 ");
            if(ManageProvider.Provider.Current().ObjectId.IndexOf("62485fb6-acaf-4c70-b17b-f5941009c9d9")<0)
            {
                strSql.Append(@" and a.createbyid='" + ManageProvider.Provider.Current().UserId + "' ");
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (UserName LIKE @keyword or RepairState like @keyword
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

    }
}
