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
    public class TR_KnowledgeBaseBll : RepositoryFactory<TR_KnowledgeBase>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.*,b.SkillName

from TR_KnowledgeBase a 
left join TR_Skill b on a.SkillID=b.SkillID  where 1=1   ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (KnowledgeName LIKE @keyword
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

        public DataTable GetMyList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select *,(select MAX(Isnull(Score,0)) from TR_Paper where KnowledgeBaseID=a.KnowledgeBaseID and UserID='{0}') as ExamScore,
(select top 1 (PaperID) from TR_Paper where KnowledgeBaseID=a.KnowledgeBaseID and FromSource=0)  as paperID,
(select max(SkillRequire) from TR_UserPost aa
left join TR_PostDepartmentRelation bb on aa.DepartmentPostID=bb.RelationID
left join TR_PostDepartmentRelationDetail cc on bb.RelationID=cc.RelationID
where aa.UserID='{0}' and cc.SkillID=a.skillid) as SkillRequire,'查看错题' as cuoti
from TR_KnowledgeBase a
where SkillID in (select SkillID from TR_PostDepartmentRelationDetail
where RelationID in (select DepartmentPostID from TR_UserPost where UserID='{0}'))   ", ManageProvider.Provider.Current().UserId);
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (KnowledgeName LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetHistoryList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.PaperID,a.paperdate,b.KnowledgeName,c.SkillName,a.FromSource,a.Score  from TR_Paper a 
left join TR_KnowledgeBase b on a.SkillID=b.KnowledgeBaseID 
left join TR_Skill c on b.SkillID=c.SkillID  where a.userid='" + ManageProvider.Provider.Current().UserId+ "' and a.FromSource=0 ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (KnowledgeName LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            strSql.Append(@" union select a.PaperID,a.paperdate,b.CustomeExamName,'自定义考试',a.FromSource,a.Score  from TR_Paper a 
left join tr_Customexam b on a.SkillID=b.CustomExamid 
  where a.userid='" + ManageProvider.Provider.Current().UserId + "' and a.FromSource=1 ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (KnowledgeName LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }
    }
}
