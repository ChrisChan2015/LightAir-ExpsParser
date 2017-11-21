using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LightAir.Infrastructure.Transactions
{
    /// <summary>
    /// 包含查询结果排序信息的接口
    /// </summary>
    public interface ISortable
    {
        /// <summary>
        /// 排序方式
        /// </summary>
        OrderBy OrderBy { get; set; }

        /// <summary>
        /// 包含排序信息的表达式树
        /// </summary>
        Expression OrderByExpression { get; set; }
    }


}
