using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightAir.Infrastructure.Transactions
{

    /// <summary>
    /// 表联接类型
    /// </summary>
    public enum JoinType
    {
        /// <summary>
        /// 内联接
        /// </summary>
        InnerJoin,
        /// <summary>
        /// 左联接
        /// </summary>
        LeftJoin,
        /// <summary>
        /// 右联接
        /// </summary>
        RightJoin
    }
}
