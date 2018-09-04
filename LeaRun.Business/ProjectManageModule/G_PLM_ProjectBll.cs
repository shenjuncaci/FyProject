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
    public class G_PLM_ProjectBll : RepositoryFactory<G_PLM_Project>
    {
        public DataTable GetDataTable(string sql)
        {
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public int ExecuteSql(StringBuilder sql)
        {
            int result = Repository().ExecuteBySql(sql);
            return result;
        }

        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select * from G_PLM_Project where 1=1  ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
           
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetUserList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.UserId,a.RealName,b.FullName,a.code from
Base_User a left join Base_Department b on a.DepartmentId=b.DepartmentId where 1=1  ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }

            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetMyTaskList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.UID,Name,Start,Finish,RealStartDate,RealFinishDate,PercentComplete from 
G_PLM_ProjectGantee a 
left join G_PLM_ProjectGanteeUser b on a.UID=b.UID
where b.userid='"+ManageProvider.Provider.Current().UserId+"'  ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }

            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public List<G_PLM_ProjectGanteeUser> GetDetailList(string ProjectID,string UID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"SELECT * FROM G_PLM_ProjectGanteeUser WHERE 1=1");
            strSql.Append(" AND ProjectID = @ProjectID and UID=@UID  ");
            parameter.Add(DbFactory.CreateDbParameter("@ProjectID", ProjectID));
            parameter.Add(DbFactory.CreateDbParameter("@UID", UID));
            return DataFactory.Database().FindListBySql<G_PLM_ProjectGanteeUser>(strSql.ToString(), parameter.ToArray());
        }
    }
}
