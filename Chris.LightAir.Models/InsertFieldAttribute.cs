using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightAir.Models.Attributes
{
    /// <summary>
    /// 用于标识模型属性为SQL Insert语句包含的字段的特性，并包含一些字段信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class InsertFieldAttribute : Attribute
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 索引值，用于组成SQL语句时，字段名的排序
        /// </summary>
        public int Index { get; set; }

        public InsertFieldAttribute(int index = 0)
        {
            this.Index = index;
        }
    }
}
