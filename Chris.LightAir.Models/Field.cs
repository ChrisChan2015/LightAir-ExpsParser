using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightAir.Models
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

        /// <summary>
        /// 是否参与update语句中的set子句
        /// </summary>
        bool CanUpdate { get; set; }

        /// <summary>
        /// 是否参与insert语句
        /// </summary>
        bool CanInsert { get; set; }
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

        public Field() 
        {
            this.CanUpdate = true;
            this.CanInsert = true;
        }

        public Field(string tableName, string name):this()
        {
            this.TableName = tableName;
            this.Name = name;
        }

        public Field(string tableName, string name,bool canUpdate,bool canInsert)
            : this(tableName,name)
        {
            this.TableName = tableName;
            this.Name = name;
            this.CanUpdate = canUpdate;
            this.CanInsert = true;
        }

        /// <summary>
        /// 是否参与update语句中的set子句
        /// </summary>
        public bool CanUpdate
        {
            get;
            set;
        }

        /// <summary>
        /// 是否参与insert语句
        /// </summary>
        public bool CanInsert
        {
            get;
            set;
        }
    }
}
