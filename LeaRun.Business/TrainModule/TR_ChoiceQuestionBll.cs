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
    public class TR_ChoiceQuestionBll : RepositoryFactory<TR_ChoiceQuestion>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson, 
            string QuestionDescripe, string SkillType, string SkillID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.*,b.skillname,b.skilltype from TR_ChoiceQuestion a left join TR_Skill b on a.SkillID=b.SkillID where 1=1   ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (QuestionDescripe LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            if (QuestionDescripe != "" && QuestionDescripe != null && QuestionDescripe != "undefined")
            {
                strSql.AppendFormat(@" and QuestionDescripe like '%{0}%' ", QuestionDescripe);
            }
            if (SkillType != "" && SkillType != null && SkillType != "undefined")
            {
                strSql.AppendFormat(@" and b.skilltype like '%{0}%' ", SkillType);
            }
            if (SkillID != "" && SkillID != null && SkillID != "undefined")
            {
                strSql.AppendFormat(@" and b.SkillID ='{0}' ", SkillID);
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
