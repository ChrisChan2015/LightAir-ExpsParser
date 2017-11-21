using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chris.Infrastructure.Transactions
{
    /// <summary>
    /// 一个表示数据表字段的接口
    /// </summary>
    public interface IField
    {
        /// <summary>
        /// 所属数据表的表名
        /// </summary>
        string TableName { get; set; }
        /// <summary>
        /// 字段名称
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    /// 表示数据库字段的类
    /// </summary>
    public class Field : IField
    {
        /// <summary>
        /// 所属数据表的表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; set; }

        public Field() { }

        public Field(string tableName, string name)
        {
            this.TableName = tableName;
            this.Name = name;
        }
    }
}
