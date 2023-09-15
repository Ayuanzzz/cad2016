using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;


namespace rdtxt
{

    public class TypedValueList : List<TypedValue>
    {
        //接受可变参数的构造函数
        public TypedValueList(params TypedValue[] args)
        {
            AddRange(args);
        }
        //添加DXF组码及对应的类型
        public void Add(int typecode, object value)
        {
            base.Add(new TypedValue(typecode, value));
        }
        //添加DXF组码及对应的类型，DXF组码直接用DXFCode参数指定
        public void Add(DxfCode typecode, object value)
        {
            base.Add(new TypedValue((int)typecode, value));
        }
        //添加图元类型，DXF组码缺省为0
        public void Add(Type entityType)
        {
            base.Add(new TypedValue(0, RXClass.GetClass(entityType).DxfName));
        }
        //TypeValueList隐式转换为TypedValue[]
        public static implicit operator TypedValue[](TypedValueList src)
        {
            return src != null ? src.ToArray() : null;
        }
        // TypedValueList隐式转换为ResultBuffer
        public static implicit operator ResultBuffer(TypedValueList src)
        {
            return src != null ? new ResultBuffer(src) : null;
        }
        //ResultBuffer隐式转换为TypedValueList
        public static implicit operator TypedValueList(ResultBuffer src)
        {
            return src != null ? new TypedValueList(src.AsArray()) : null;
        }
    }
}
