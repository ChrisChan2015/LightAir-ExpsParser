using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightAir.Models.Attributes
{
    /// <summary>
    /// ���ڱ�ʶģ������ΪSQL Select���������ֶε����ԣ�������һЩ�ֶ���Ϣ
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SelectFieldAttribute : Attribute
    {
        /// <summary>
        /// �ֶ�����
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ����ֵ���������SQL���ʱ���ֶ���������
        /// </summary>
        public int Index { get; set; }

        public SelectFieldAttribute(int index=0)
        {
            this.Index = index;
        }
    }
}
