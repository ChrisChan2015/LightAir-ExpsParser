using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightAir.Models.Attributes
{
    /// <summary>
    /// 用于标识模型属性为SQL Update语句包含的字段的特性，并包含一些字段信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UpdateFieldAttribute : Attribute
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; set; }
    }
}
