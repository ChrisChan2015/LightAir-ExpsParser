using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LightAir.Infrastructure.Transactions
{
    /// <summary>
    /// 包含SQL表联接信息的接口
    /// </summary>
    public interface IJoinTable
    {
        /// <summary>
        /// 表联接类型
        /// </summary>
        JoinType JoinType { get; set; }

        /// <summary>
        /// 外键表名称
        /// </summary>
        string FKTableName { get; set; }

        /// <summary>
        /// 包含表联接信息的表达式树
        /// </summary>
        Expression JoinExpression { get; set; }
    }

    /// <summary>
    /// 包含SQL表联接信息的类型
    /// </summary>
    public class JoinTable : IJoinTable
    {
        /// <summary>
        /// 表联接类型
        /// </summary>
        public JoinType JoinType { get; set; }

        /// <summary>
        /// 外键表名称
        /// </summary>
        public string FKTableName
        {
            get;
            set;
        }

        /// <summary>
        /// 包含表联接信息的表达式树
        /// </summary>
        public Expression JoinExpression { get; set; }

        public JoinTable() { }

        public JoinTable(JoinType joinType,string fkTableName, Expression joinExp)
        {
            this.JoinType = joinType;
            this.FKTableName = fkTableName;
            this.JoinExpression = joinExp;
        }
    }
}
