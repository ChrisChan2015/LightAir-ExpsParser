using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightAir.Models
{
    /// <summary>
    /// 定义一个存储字段集合的接口
    /// </summary>
    public interface IFieldCollection
    {
        /// <summary>
        /// 获取存储字段的集合
        /// </summary>
        ISet<IField> Fields { get; }
    }

    /// <summary>
    /// 表示一个存储字段集合的类
    /// </summary>
    public class FieldCollection : IFieldCollection
    {
        private ISet<IField> _fields;

        /// <summary>
        /// 获取存储字段的集合
        /// </summary>
        public ISet<IField> Fields
        {
            get { return _fields; }
        }

        public FieldCollection()
        {
            this._fields = new HashSet<IField>();
        }

        public static IFieldCollection operator +(FieldCollection fields, IField field)
        {
            fields.Fields.Add(field);
            return fields;
        }

        public static IFieldCollection operator -(FieldCollection fields, IField field)
        {
            fields.Fields.Remove(field);
            return fields;
        }
    }
}
