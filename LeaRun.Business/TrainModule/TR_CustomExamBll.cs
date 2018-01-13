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
    public class TR_CustomExamBll : RepositoryFactory<TR_CustomExam>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.*,b.RealName,c.FullName from tr_Customexam a left join Base_User b on a.CreateBy=b.UserId
left join Base_Department c on a.CreateDept=c.DepartmentId where 1=1 and a.IsEnable=1   ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (CustomeExamName LIKE @keyword
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


        public DataTable GetExamList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select a.*,b.RealName,c.FullName,(select top 1 Score from TR_Paper where SkillID=a.CustomExamID and FromSource='1' and UserID='{0}' order by paperdate desc) as topscore,
(select top 1 (PaperID) from TR_Paper where SkillID=a.CustomExamID and FromSource=1 and UserID='{0}' order by PaperDate desc)  as paperID
from tr_Customexam a left join Base_User b on a.CreateBy=b.UserId
left join Base_Department c on a.CreateDept=c.DepartmentId where 1=1 and exists (select * from TR_CustomExamUser where userid='{0}' and 
CustomExamid=a.CustomExamID)   ", ManageProvider.Provider.Current().UserId);
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (CustomeExamName LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public List<TR_CustomExamChoice> GetDetailList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from TR_CustomExamChoice WHERE 1=1");
            strSql.Append(" AND CustomExamID = @CustomExamID and IsEnabe=1 order by SortNO ");
            parameter.Add(DbFactory.CreateDbParameter("@CustomExamID", KeyValue));
            return DataFactory.Database().FindListBySql<TR_CustomExamChoice>(strSql.ToString(), parameter.ToArray());
        }

        public List<TR_CustomExamUserForDisplay> GetUserList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select a.UserID,b.Code,b.RealName,c.FullName from TR_CustomExamUser a left join Base_User b on a.UserID=b.UserId
left join Base_Department c on b.DepartmentId=c.DepartmentId where 1=1 and a.CustomExamID=@CustomExamID
order by c.FullName ");
            
            parameter.Add(DbFactory.CreateDbParameter("@CustomExamID", KeyValue));
            return DataFactory.Database().FindListBySql<TR_CustomExamUserForDisplay>(strSql.ToString(), parameter.ToArray());
        }

        public DataTable GetBaseUserList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.UserID,a.Code,a.RealName,b.FullName from
Base_User a left join Base_Department b on a.DepartmentId=b.DepartmentId where 1=1  ");
            //if (!string.IsNullOrEmpty(keyword))
            //{
            //    strSql.Append(@" AND (SkillName LIKE @keyword or SkillType like @keyword
            //                        )");
            //    parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            //}
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.AppendFormat(keyword);
            }
            
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetDeptList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select DepartmentId,FullName from Base_Department where 1=1  ");
            //if (!string.IsNullOrEmpty(keyword))
            //{
            //    strSql.Append(@" AND (SkillName LIKE @keyword or SkillType like @keyword
            //                        )");
            //    parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            //}
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.AppendFormat(keyword);
            }

            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }
    }
}
