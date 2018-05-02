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
    public class TR_SkillBll : RepositoryFactory<TR_Skill>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select * from tr_skill where Enable=1 and IsAudit=1  ");

            if(ManageProvider.Provider.Current().ObjectId.IndexOf("33a32375-8723-46d3-b4c9-ca19de367d34")<0)
            {
                strSql.AppendFormat(" and  (DepartmentID='" + ManageProvider.Provider.Current().DepartmentId + "' or DepartmentID in (select DepartmentId from Base_Department where ParentId='" + ManageProvider.Provider.Current().DepartmentId + "'))  ");
            }

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

        public DataTable GetFileList(string keyword, ref JqGridParam jqgridparam, string ParameterJson,string SkillID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select a.*,b.SkillType from TR_SkillFile a left join TR_Skill b on a.SkillID=b.SkillID where a.skillid='{0}'  ", SkillID);
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

        //技能矩阵的报表
        public DataTable GetSkillMatrixData(string DepartmentID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(DbFactory.CreateDbParameter("@DepartmentID", DepartmentID));
           
            return Repository().FindDataSetByProc("SkillMatrix", parameter.ToArray()).Tables[0];
        }

        //人员多功能报表
        public DataTable GetMultifunctionData(string DepartmentID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(DbFactory.CreateDbParameter("@DepartmentID", DepartmentID));

            return Repository().FindDataSetByProc("MultifunctionMatrix", parameter.ToArray()).Tables[0];
        }

        //修改技能矩阵的制作思路,选出基本的框架，内里的数据根据行列在一个个计算，效率不敢恭维，幸好服务器的配置高
        public DataTable GetSkill(string DepartmentID)
        {
            string sql = @"select f.SkillID,f.SkillName
from TR_UserPost a 
left join TR_PostDepartmentRelation b on a.DepartmentPostID=b.RelationID
left join TR_Post c on b.PostID=c.PostID 
left join Base_User d on a.UserID=d.UserId
left join TR_PostDepartmentRelationDetail e on e.RelationID=b.RelationID
left join tr_skill f on e.SkillID=f.SkillID
where d.Enabled=1 and f.SkillID is not null and d.departmentID='" + DepartmentID+"' ";

            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        //我的学习
        public DataTable GetMyStudyList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select *,
(select top 1 (Isnull(Score,0)) from TR_Paper where SkillID=a.SkillID and UserID='{0}' order by PaperDate desc ) as ExamScore,
(select top 1 (PaperID) from TR_Paper where SkillID=a.SkillID and FromSource=0 order by PaperDate desc)  as paperID,
(select max(SkillRequire) from TR_UserPost aa
left join TR_PostDepartmentRelation bb on aa.DepartmentPostID=bb.RelationID
left join TR_PostDepartmentRelationDetail cc on bb.RelationID=cc.RelationID
where aa.UserID='{0}' and cc.SkillID=a.skillid) as SkillRequire
from TR_Skill a
where SkillID in (select SkillID from TR_PostDepartmentRelationDetail
where RelationID in (select DepartmentPostID from TR_UserPost where UserID='{0}'))   ", ManageProvider.Provider.Current().UserId);
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (SkillName LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        //申请列表
        public DataTable GetApplyList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select ApplyID,ApplyDate,ExamType=case when a.Source=0 then '技能考试' when a.Source=1 then '自定义考试' else '异常' end,
b.RealName,c.SkillName,
ExamStatus=case when a.IsOK=0 then '等待审核' when a.IsOk=1 then '审核通过，等待开始' when a.IsOk=2 then '已进入考试' else '异常' end
from 
TR_ExamApply a left join Base_User b on a.UserID=b.UserId
left join (select skillid,SkillName from TR_Skill union select CustomExamID,CustomeExamName from TR_CustomExam) c on a.ExamID=c.SkillID  ", ManageProvider.Provider.Current().UserId);
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (b.realname LIKE @keyword or Skillname like @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetExamScoreList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select *,type=case when fromsource=0 then '技能考试' when fromsource=1 then '专项考试' when fromsource=2 then '人才培养' when fromsource=4 then '主管评价' else '错误'  end 
from V_ExamScore where 1=1 ");
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

        public List<TR_ChoiceQuestion> GetQuestionList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from TR_ChoiceQuestion WHERE 1=1 and IsEnable=1 ");
            strSql.Append(" AND SkillID = @SkillID order by SortNO ");
            parameter.Add(DbFactory.CreateDbParameter("@SkillID", KeyValue));
            return DataFactory.Database().FindListBySql<TR_ChoiceQuestion>(strSql.ToString(), parameter.ToArray());
        }
    }
}
