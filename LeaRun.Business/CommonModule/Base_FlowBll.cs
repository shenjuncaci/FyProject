using LeaRun.DataAccess;
using LeaRun.Entity;
using LeaRun.Repository;
using LeaRun.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace LeaRun.Business
{
    public class Base_FlowBll : RepositoryFactory<Base_Flow>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select * from base_flow where 1=1 ");
//            if (!string.IsNullOrEmpty(keyword))
//            {
//                strSql.Append(@" AND (code LIKE @keyword
//                                    OR FullName LIKE @keyword
//                                    OR CreateUserName LIKE @keyword
//                                    )");
//                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
//            }

            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }



        public DataTable NodeList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select PostId,Code,FullName,SortCode from Base_Post where 1=1");
            //if (!ManageProvider.Provider.Current().IsSystem)
            //{
            //    strSql.Append(" AND ( RoleId IN ( SELECT ResourceId FROM Base_DataScopePermission WHERE");
            //    strSql.Append(" ObjectId IN ('" + ManageProvider.Provider.Current().ObjectId.Replace(",", "','") + "') ");
            //    strSql.Append(" ) )");
            //}
            //strSql.Append(" AND r.CompanyId = @CompanyId");
            //parameter.Add(DbFactory.CreateDbParameter("@UserId", UserId));
            //parameter.Add(DbFactory.CreateDbParameter("@CompanyId", CompanyId));
            return Repository().FindTableBySql(strSql.ToString(), parameter.ToArray());
        }

        //当前流程图
        public DataTable CurrentFlow(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select b.FullName from Base_FlowStep a left join Base_Post b on a.CurrentPostID=b.PostId where a.FlowID=@FlowID ");
            parameter.Add(DbFactory.CreateDbParameter("@FlowID", KeyValue));
            return Repository().FindTableBySql(strSql.ToString(), parameter.ToArray());
        }



        public int BatchAddObject(string FlowID, string[] arrayObjectId, string Category)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                StringBuilder sbDelete = new StringBuilder("DELETE FROM base_flowstep WHERE FlowID = @FlowID ");
                List<DbParameter> parameter = new List<DbParameter>();
                parameter.Add(DbFactory.CreateDbParameter("@FlowID", FlowID));
                
                database.ExecuteBySql(sbDelete, parameter.ToArray(), isOpenTrans);
                int index = 1;
                foreach (string item in arrayObjectId)
                {
                    if (item.Length > 0)
                    {
                        Base_FlowStep entity = new Base_FlowStep();
                        entity.FlowStepID = CommonHelper.GetGuid;
                        entity.FlowID = FlowID;
                        entity.CurrentPostID = item;
                        
                        entity.Create();

                        //流程节点顺序
                        entity.StepNO = index.ToString();
                        index++;
                        database.Insert(entity, isOpenTrans);
                    }
                }
                DataFactory.Database().Commit();
                return 1;
            }
            catch
            {
                database.Rollback();
                database.Close();
                throw;
            }
        }

        /// <summary>
        /// 根据flowno注册流程，注册的流程保存在base_flowlog中
        /// </summary>
        /// <returns></returns>
        public string RegistFlow(string FlowNO,string NoteID,string NodeUser)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            Base_FlowLog entity = new Base_FlowLog();
            Base_FlowLogDetail entityD = new Base_FlowLogDetail();
            //StringBuilder strSql = new StringBuilder();    //最后insert的语句，不用了，用model方便多了~
            string FlowID = "";
            string FindFlow = " select * from base_flow where FlowNO='"+FlowNO+"'  ";
            string FindFlowDetail = "";
            DataTable dt= Repository().FindDataSetBySql(FindFlow).Tables[0];
            if(dt.Rows.Count>0)
            {
                FlowID = dt.Rows[0][0].ToString();
                FindFlowDetail = " select * from Base_FlowStep where FlowID='"+FlowID+ "' order by StepNO ";
            }
            DataTable dtDetail= Repository().FindDataSetBySql(FindFlowDetail).Tables[0];
            if(dtDetail.Rows.Count>0)
            {
                entity.Create();
                entity.Approvestatus = 0;
                //注册流程的时候不要添加审核人，转为提交以后再添加
                //entity.CurrentPost = dtDetail.Rows[0]["CurrentPostID"].ToString();
                //entity.CurrentPerson = GetPersonID(dtDetail.Rows[0]["CurrentPostID"].ToString());
                entity.FlowNo = FlowNO;
                entity.NoteID = NoteID;
                database.Insert(entity, isOpenTrans);
                
                //添加流程明细表的数据
                for(int i=0;i<dtDetail.Rows.Count;i++)
                {
                    entityD.Create();
                    entityD.FlowID = entity.FlowID;
                    entityD.Approvestatus = "等待处理";
                    entityD.StepNO = i;

                    //几个特殊节点的处理
                    //责任人节点，选择表单中的责任人
                    if (dtDetail.Rows[i]["CurrentPostID"].ToString() == "1538456f-0a50-4f1f-a6e2-5b3a5862f53a"|| dtDetail.Rows[i]["CurrentPostID"].ToString() == "3efe8c99-fcaa-4efd-9523-ef0fa652557c"
                        || dtDetail.Rows[i]["CurrentPostID"].ToString() == "893b3b24-4f61-413d-8298-fe3237e3e790")
                    {
                        entityD.ApproveBy = NodeUser;
                    }
                    //部门负责人节点，特殊处理
                    else if(dtDetail.Rows[i]["CurrentPostID"].ToString() == "42ffa89f-b40b-4260-a9b2-974f3585e05f")
                    {
                        entityD.ApproveBy = GetDepartMentMaster();
                    }
                    else
                    {
                        entityD.ApproveBy = GetPersonID(dtDetail.Rows[i]["CurrentPostID"].ToString());
                    }
                    entityD.ApprovePost = dtDetail.Rows[i]["CurrentPostID"].ToString();

                    entityD.IsFinish = 0;
                    database.Insert(entityD, isOpenTrans);
                }

                database.Commit();
            }

            
            
            return entity.FlowID;
        }
        /// <summary>
        /// 通过noteid获取单据上的责任人
        /// </summary>
        /// <param name="NoteID"></param>
        /// <returns></returns>
        public string GetResponseID(string NoteID,string FlowNo)
        {
            string ID = "";
            string sql= "";
            if (FlowNo == "Sj_HrProblem")
            {
                 sql = "select ResponseBy from FY_HrProblem where ProblemID='" + NoteID + "'";
            }
            DataTable dt = Repository().FindDataSetBySql(sql).Tables[0];
            if(dt.Rows.Count>0)
            {
                ID = dt.Rows[0][0].ToString();
            }
            return ID;
        }

        public string GetPersonID(string PostID)
        {
            string ID = "";
            
            string sql = "select UserId from Base_ObjectUserRelation where Category=3 and ObjectId='"+PostID+"'";
            DataTable dt = Repository().FindDataSetBySql(sql).Tables[0];
            if(dt.Rows.Count>0)
            {
                ID = dt.Rows[0][0].ToString();
            }
            return ID;
        }

        public string GetDepartMentMaster()
        {
            string ID = "";
            string sql = @" select a.UserId from Base_ObjectUserRelation a
left join Base_User b on a.UserId=b.UserId
 where a.ObjectId='91c17ca4-0cbf-43fa-829e-3021b055b6c4' and b.DepartmentId='"+ManageProvider.Provider.Current().DepartmentId+"' ";
            DataTable dt = Repository().FindDataSetBySql(sql).Tables[0];
            if (dt.Rows.Count > 0)
            {
                ID = dt.Rows[0][0].ToString();
            }
            return ID;
        }

        /// <summary>
        /// 页面显示流程状态
        /// </summary>
        /// <param name="FlowID"></param>
        /// <returns></returns>
        public FlowDisplay FlowDisplay(string FlowID)
        {
            string strSql = " select a.*,b.FullName,c.RealName,d.FlowName,case when a.approvestatus=9 then '完成' when a.approvestatus=7 then '进行中' else '未提交' end as ApprovestatusCN from Base_Flowlog a " +
                "left join Base_Post b on a.currentpost=b.PostId left join Base_User c on a.currentperson=c.UserId " +
                "left join Base_Flow d on a.flowno=d.flowno " +
                " where a.flowid='" + FlowID+"' ";
            string strSqlDetail = " select a.*,b.FullName,c.RealName from base_flowlogdetail a " +
                "left join Base_Post b on a.approvepost=b.PostId left join Base_User c on a.approveby=c.UserId " +
                "where flowid='" + FlowID+"' ";
            DataTable dt = Repository().FindDataSetBySql(strSql).Tables[0];
            DataTable dtDetail = Repository().FindDataSetBySql(strSqlDetail).Tables[0];
            FlowDisplay bfl = DtConvertHelper.ConvertToModel<FlowDisplay>(dt,0);
            IList<FlowDetailDisplay> bfllist = DtConvertHelper.ConvertToModelList<FlowDetailDisplay>(dtDetail);

            bfl.LogList = bfllist;

            return bfl;
        }

        //提交流程，返回流程状态，int类型
        public int SubmitFlow(string FlowID)
        {
            string strSql = " select a.*,b.FullName,c.RealName from Base_Flowlog a " +
                "left join Base_Post b on a.currentpost=b.PostId left join Base_User c on a.currentperson=c.UserId" +
                " where flowid='" + FlowID + "' ";
            string strSqlDetail = " select a.*,b.FullName,c.RealName from base_flowlogdetail a " +
                "left join Base_Post b on a.approvepost=b.PostId left join Base_User c on a.approveby=c.UserId " +
                "where flowid='" + FlowID + "' order by a.stepno ";
            DataTable dt = Repository().FindDataSetBySql(strSql).Tables[0];
            DataTable dtDetail = Repository().FindDataSetBySql(strSqlDetail).Tables[0];
            Base_FlowLog bfl= DtConvertHelper.ConvertToModel<Base_FlowLog>(dt, 0);
            IList<Base_FlowLogDetail> bfldlist = DtConvertHelper.ConvertToModelList<Base_FlowLogDetail>(dtDetail);

            if(bfl.Approvestatus==0)
            {
                //为0表示未提交，明细表无需操作，主表状态修改即可
                bfl.Approvestatus = 7;
                bfl.CurrentPerson = bfldlist[0].ApproveBy;
                bfl.CurrentPost = bfldlist[0].ApprovePost;
                bfldlist[0].Approvestatus = "等待操作";
                DataFactory.Database().Update<Base_FlowLogDetail>(bfldlist[0]);
            }
            //if(bfl.Approvestatus==7)
            else
            {
                //7为待审核的状态
                for(int i=0;i<bfldlist.Count;i++)
                {
                    if(bfldlist[i].IsFinish==0)
                    {
                        if(i==bfldlist.Count-1) //等于最大的时候表示审批完成以后流程结束
                        {
                            bfl.Approvestatus = 9;
                            bfldlist[i].IsFinish = 1;
                            bfldlist[i].Approvestatus = ManageProvider.Provider.Current().UserName+" 审批通过";
                            bfl.CurrentPost = "";
                            bfl.CurrentPerson = "";
                        }
                        else
                        {
                            bfl.Approvestatus = 7;
                            bfldlist[i].IsFinish = 1;
                            bfldlist[i].Approvestatus = ManageProvider.Provider.Current().UserName + " 审批通过";
                            bfl.CurrentPerson = bfldlist[i + 1].ApproveBy;
                            bfl.CurrentPost = bfldlist[i + 1].ApprovePost;
                        }
                        DataFactory.Database().Update<Base_FlowLogDetail>(bfldlist[i]);
                        //跳出循环
                        break;
                    }
                }
            }
            //if(bfl.Approvestatus==-1)
            //{
            //    //表示流程被退回
            //    if()
            //}

            DataFactory.Database().Update<Base_FlowLog>(bfl);
            return bfl.Approvestatus;
        }

        //流程退回
        public int RejectFlow(string FlowID)
        {
            string strSql = " select a.*,b.FullName,c.RealName from Base_Flowlog a " +
                "left join Base_Post b on a.currentpost=b.PostId left join Base_User c on a.currentperson=c.UserId" +
                " where flowid='" + FlowID + "' ";
            string strSqlDetail = " select a.*,b.FullName,c.RealName from base_flowlogdetail a " +
                "left join Base_Post b on a.approvepost=b.PostId left join Base_User c on a.approveby=c.UserId " +
                "where flowid='" + FlowID + "' order by a.stepno ";
            DataTable dt = Repository().FindDataSetBySql(strSql).Tables[0];
            DataTable dtDetail = Repository().FindDataSetBySql(strSqlDetail).Tables[0];
            Base_FlowLog bfl = DtConvertHelper.ConvertToModel<Base_FlowLog>(dt, 0);
            IList<Base_FlowLogDetail> bfldlist = DtConvertHelper.ConvertToModelList<Base_FlowLogDetail>(dtDetail);
            if(bfl.Approvestatus!=7)
            {
                return 0;  //只有在进行中的流程可以退回
            }
            else
            {
                for(int i=0;i<bfldlist.Count;i++)
                {
                    if(bfldlist[i].IsFinish==0)
                    {
                        if(i==0)
                        {
                            //如果在第一步就被退回，只需要修改主表的状态
                            bfl.Approvestatus = 0;
                            bfl.CurrentPerson = "";
                            bfl.CurrentPost = "";
                            bfldlist[i].Approvestatus = ManageProvider.Provider.Current().UserName +" 退回";
                            DataFactory.Database().Update<Base_FlowLogDetail>(bfldlist[i]);
                        }
                        else
                        {
                            //如果不是第一步的话，上一节点的操作者状态需要修改
                            bfl.Approvestatus = 7;
                            bfldlist[i].Approvestatus = ManageProvider.Provider.Current().UserName + " 退回";
                            bfldlist[i - 1].IsFinish = 0;
                            bfldlist[i - 1].Approvestatus = "等待操作";
                            bfl.CurrentPost = bfldlist[i - 1].ApprovePost;
                            bfl.CurrentPerson = bfldlist[i - 1].ApproveBy;
                            DataFactory.Database().Update<Base_FlowLogDetail>(bfldlist[i]);
                            DataFactory.Database().Update<Base_FlowLogDetail>(bfldlist[i-1]);
                        }
                        break;
                    }
                }
            }
            DataFactory.Database().Update<Base_FlowLog>(bfl);
            return bfl.Approvestatus;

        }

        
    }
}