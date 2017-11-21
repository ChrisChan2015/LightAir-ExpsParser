using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightAir.Infrastructure.Transactions
{
    /// <summary>
    /// 数据库操作类型
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// 插入操作
        /// </summary>
        Insert,
        /// <summary>
        /// 查询操作
        /// </summary>
        Select,        
        /// <summary>
        /// 更新操作
        /// </summary>
        Update,
        /// <summary>
        /// 删除操作
        /// </summary>
        Delete
    }
}
