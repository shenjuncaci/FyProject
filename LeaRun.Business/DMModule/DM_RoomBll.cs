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
    public class DM_RoomBll : RepositoryFactory<DM_Room>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson,
            string IsEmpty)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.*,b.dormname,
cast((select count(*) from DM_CheckIn where IsLeave=0 and RoomID=a.roomid) as nvarchar(100))+'/'+cast(a.standardPeople as nvarchar(10)) as State
from DM_Room a left join DM_Dorm b on a.dormid=b.dormid where 1=1   ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
            }
            if(IsEmpty=="有空位")
            {
                strSql.AppendFormat(@" and (select count(*) from DM_CheckIn where IsLeave=0 and RoomID=a.roomid)<a.standardPeople ");
            }
            else if(IsEmpty=="无空位")
            {
                strSql.AppendFormat(@" and (select count(*) from DM_CheckIn where IsLeave=0 and RoomID=a.roomid)>=a.standardPeople ");
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

        public List<DM_CheckIn> GetDetailList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from dm_checkin WHERE 1=1");
            strSql.Append(" AND roomid = @RoomID order by CheckInDate ");
            parameter.Add(DbFactory.CreateDbParameter("@RoomID", KeyValue));
            return DataFactory.Database().FindListBySql<DM_CheckIn>(strSql.ToString(), parameter.ToArray());
        }

        public List<DM_Assets> GetAssetsList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select * from dm_assets where roomid=@RoomID ");
            parameter.Add(DbFactory.CreateDbParameter("@RoomID", KeyValue));
            return DataFactory.Database().FindListBySql<DM_Assets>(strSql.ToString(), parameter.ToArray());
        }

    }
}
