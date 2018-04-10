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
    public class PM_NoticeBll : RepositoryFactory<PM_Notice>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select a.*,b.projectname from pm_notice a left join pm_project b on a.projectid=b.projectid where 1=1 
and exists (select * from PM_ProjectMember where ProjectID=b.ProjectID and UserID='{0}') ",ManageProvider.Provider.Current().UserId);
            //if (!string.IsNullOrEmpty(keyword))
            //{
            //    strSql.Append(@" AND (PostName LIKE @keyword
            //                        )");
            //    parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            //}
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
