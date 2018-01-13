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
    public class DM_RoomCostBll : RepositoryFactory<DM_RoomCost>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.*,b.DormName from DM_RoomCost a left join 
DM_Dorm b on a.dormid=b.dormid where 1=1  ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (DormName LIKE @keyword
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

        public DataTable GetRoomCostJson(string keyword)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            //strSql.Append(@"select * from ##list ");
            //StringBuilder proc = new StringBuilder();
            //proc.AppendFormat(@"RapidMonthlyReport 2017 ");

            //Repository().ExecuteBySql(proc);
            //if (!string.IsNullOrEmpty(keyword))
            //{

            //parameter.Add(DbFactory.CreateDbParameter("@keyword", 2017));
            //}
            parameter.Add(DbFactory.CreateDbParameter("@InputDate", keyword));
            

            return Repository().FindDataSetByProc("Proc_RoomCostByEveryOne", parameter.ToArray()).Tables[0];
        }

    }
}
