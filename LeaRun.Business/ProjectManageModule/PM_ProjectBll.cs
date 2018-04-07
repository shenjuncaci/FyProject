﻿using LeaRun.DataAccess;
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
    public class PM_ProjectBll : RepositoryFactory<PM_Project>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.*,b.fullname from pm_project a left join base_department b on a.departmentid=b.departmentid where 1=1   ");
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

        public DataTable GetDataTable(string sql)
        {
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public int ExecuteSql(StringBuilder sql)
        {
            int result = Repository().ExecuteBySql(sql);
            return result;
        }

        public List<PM_ProjectMember> GetDetailList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select a.*,b.realname from PM_ProjectMember a left join Base_user b on a.userid=b.userid where a.ProjectID=@ProjectID");
            //strSql.Append(" AND CustomExamID = @CustomExamID and IsEnabe=1 order by SortNO ");
            parameter.Add(DbFactory.CreateDbParameter("@ProjectID", KeyValue));
            return DataFactory.Database().FindListBySql<PM_ProjectMember>(strSql.ToString(), parameter.ToArray());
        }

        public DataTable GetUserList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.*,b.fullname from base_user a left join base_department b on a.departmentid=b.departmentid where 1=1   ");
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

    }
}