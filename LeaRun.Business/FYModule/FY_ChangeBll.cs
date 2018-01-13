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
    public class FY_ChangeBll : RepositoryFactory<FY_Change>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson,string IsMy)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from FY_Change  where 1=1 ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (ProjectName LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            //我的待办内容
            if(IsMy=="1")
            {
                strSql.AppendFormat("and (");
                //部门经理的内容
                string IsManager = "0";
                string sqlIsManager = "select * from Base_ObjectUserRelation where ObjectId='e65b5d68-3d6e-4865-9de4-c79a631fda48'";
                DataTable dt = GetDataTable(sqlIsManager);
                if(dt.Rows.Count>0)
                {
                    IsManager = "1";
                }
                
                if(IsManager=="1")
                {
                    strSql.AppendFormat(@"  ChangeState in ('等待库存确认','等待评审') and 
(ChangeID in (select distinct ChangeID from FY_ChangeBreak where BreakState in ('未提交','退回') and ChangeBreakByID='{0}'
union
select distinct ChangeID from FY_ChangeReview where ChangeReviewState in ('未提交','退回') and ReviewBy='{0}') ", ManageProvider.Provider.Current().UserId);
                    strSql.AppendFormat(@" or changeid in (select distinct ChangeID from FY_ChangeBreak where BreakState ='进行中' and ChangeBreakDeptID='{0}'
union
select distinct ChangeID from FY_ChangeReview where ChangeReviewState ='进行中' and ReviewDepart='{0}') ) ",ManageProvider.Provider.Current().DepartmentId);
                }
                //总经理的内容
                string IsTopManager = "0";
                sqlIsManager = " select * from Base_ObjectUserRelation where ObjectId='15f14d9c-e74c-46ac-8641-b3c1bac26940' and UserId='" + ManageProvider.Provider.Current().UserId + "'  ";
                dt = GetDataTable(sqlIsManager);
                if (dt.Rows.Count > 0)
                {
                    IsTopManager = "1";
                }
                if(IsTopManager=="1")
                {
                    strSql.AppendFormat(@" or changestate in ('等待总经理批准') ");
                }
                //财务部分
                string IsFi = "0";
                sqlIsManager = " select * from Base_ObjectUserRelation where ObjectId='259b1ace-50ca-42b3-b85e-44e72ab7dd64' and UserId='" + ManageProvider.Provider.Current().UserId + "'  ";
                dt = GetDataTable(sqlIsManager);
                if (dt.Rows.Count > 0)
                {
                    IsFi = "1";
                }
                if(IsFi=="1")
                {
                    strSql.AppendFormat(@" or changestate in ('等待财务确认') ");
                }
                //项目主管部分
                string IsProjectLeader = "0";
                sqlIsManager = " select * from Base_ObjectUserRelation where ObjectId='6ed93a98-4031-4c2f-919a-18dc3abe71ed' and UserId='" + ManageProvider.Provider.Current().UserId + "'  ";
                dt = GetDataTable(sqlIsManager);
                if (dt.Rows.Count > 0)
                {
                    IsProjectLeader = "1";
                }
                if(IsProjectLeader=="1")
                {
                    strSql.AppendFormat(@" or changestate in ('等待项目主管评审') ");
                }
                strSql.AppendFormat(")");
                strSql.AppendFormat(@" or (changestate!='已完成' and createbyid='{0}') ",ManageProvider.Provider.Current().UserId);

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


        //明细表数据获取
        /// <summary>
        /// 明细列表
        /// </summary>
        /// <param name="ChangeID">变更主表ID</param>
        /// <returns></returns>
        public List<FY_ChangeReview> GetChangeReviewList(string ChangeID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"SELECT a.*,b.RealName,c.FullName FROM FY_ChangeReview a left join Base_User b on a.ReviewBy=b.UserId left join Base_Department c on a.ReviewDepart=c.DepartmentId WHERE 1=1");
            strSql.Append(" AND a.ChangeID = @ChangeID order by ReviewDepart ");
            parameter.Add(DbFactory.CreateDbParameter("@ChangeID", ChangeID));
            return DataFactory.Database().FindListBySql<FY_ChangeReview>(strSql.ToString(), parameter.ToArray());
        }

        public List<FY_ChangeCost> GetChangeCostList(string ChangeID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"SELECT * FROM FY_ChangeCost WHERE 1=1");
            strSql.Append(" AND ChangeID = @ChangeID order by CostType ");
            parameter.Add(DbFactory.CreateDbParameter("@ChangeID", ChangeID));
            return DataFactory.Database().FindListBySql<FY_ChangeCost>(strSql.ToString(), parameter.ToArray());
        }

        public List<FY_ChangeBreak> GetChangeBreakList(string ChangeID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"SELECT * FROM fy_changebreak WHERE 1=1");
            strSql.Append(" AND ChangeID = @ChangeID order by changebreakname ");
            parameter.Add(DbFactory.CreateDbParameter("@ChangeID", ChangeID));
            return DataFactory.Database().FindListBySql<FY_ChangeBreak>(strSql.ToString(), parameter.ToArray());
        }
    }
}
