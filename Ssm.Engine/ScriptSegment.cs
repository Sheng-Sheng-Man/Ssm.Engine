using Sevm.Sir;
using System;
using System.Collections.Generic;
using System.Text;
using egg;

namespace Ssm.Engine {

    /// <summary>
    /// 脚本代码段
    /// </summary>
    public class ScriptSegment {

        /// <summary>
        /// 获取代码集合
        /// </summary>
        public Sevm.Sir.SirCodes Codes { get; private set; }

        /// <summary>
        /// 获取代码集合
        /// </summary>
        public ScriptSemanticTypes ScriptType { get; private set; }

        /// <summary>
        /// 获取代码集合
        /// </summary>
        public ScriptSegment Parent { get; private set; }

        /// <summary>
        /// 所属脚本引擎
        /// </summary>
        public ScriptEngine Engine { get; private set; }

        /// <summary>
        /// 获取索引号
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 获取索引号
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// 获取或设置结束索引号
        /// </summary>
        public int IndexForTrue { get; set; }

        /// <summary>
        /// 获取或设置结束索引号
        /// </summary>
        public int IndexForFalse { get; set; }

        /// <summary>
        /// 获取或设置结束索引号
        /// </summary>
        public int IndexForEnd { get; set; }

        /// <summary>
        /// 是否存在
        /// </summary>
        public bool HasTrue { get; set; }

        /// <summary>
        /// 是否存在
        /// </summary>
        public bool HasFalse { get; set; }

        /// <summary>
        /// 从字符串中获取表达式
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public SirExpression GetValueExpression(string strValue) {
            // 为空判断
            if (strValue == "空白") return SirExpression.IntPtr(0);
            if (strValue == "空的") return SirExpression.IntPtr(0);
            if (strValue == "没有东西") return SirExpression.IntPtr(0);
            if (strValue == "真的") return SirExpression.Value(1);
            if (strValue == "假的") return SirExpression.Value(0);
            if (strValue == "真") return SirExpression.Value(1);
            if (strValue == "假") return SirExpression.Value(0);
            if (strValue.IsDouble()) { // 判断是否为数字
                // 小数则使用内存处理
                int idx = this.Engine.VariableIndexer.GetNewIndex();
                this.Engine.SirScript.Datas.Add(idx, strValue.ToDouble());
                return SirExpression.Variable(idx);
            } else if (strValue.Length >= 4 && ((strValue.StartsWith("\"'") && strValue.EndsWith("'\"")) || (strValue.StartsWith("““") && strValue.EndsWith("””")))) { // 判断是否为强字符串
                // 获取新的虚拟内存索引
                int idx = this.Engine.VariableIndexer.GetNewIndex();
                // 添加数据定义
                this.Engine.SirScript.Datas.Add(idx, strValue.Substring(2, strValue.Length - 4));
                return SirExpression.Variable(idx);
            } else if ((strValue.StartsWith("\"") && strValue.EndsWith("\"")) || (strValue.StartsWith("“") && strValue.EndsWith("”"))) { // 判断是否为字符串
                // 获取新的虚拟内存索引
                int idx = this.Engine.VariableIndexer.GetNewIndex();
                // 添加数据定义
                this.Engine.SirScript.Datas.Add(idx, strValue.Substring(1, strValue.Length - 2));
                return SirExpression.Variable(idx);
            } else if (strValue.StartsWith("$")) { // 内部变量
                // 获取新的虚拟内存索引
                int index = int.Parse(strValue.Substring(1));
                // 添加数据定义
                return SirExpression.Variable(index);
            } else { // 不然则为变量
                //varValue = new EngineVariable(strValue);
                // 获取变量定义信息
                var def = this.GetVariableDefine(strValue);
                if (eggs.Object.IsNull(def)) throw new SirException($"未定义的变量'{strValue}'");
                return SirExpression.Variable(def.Index);
            }
        }

        /// <summary>
        /// 获取变量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Sevm.Sir.SirDefine GetVariableDefine(string name) {
            // 获取当前程序段的变量名称
            string fullName = this.GetFullVariableName(name);
            // 查找匹配的名称
            for (int i = 0; i < this.Engine.SirScript.Defines.Count; i++) {
                if (this.Engine.SirScript.Defines[i].Name == fullName) return this.Engine.SirScript.Defines[i];
            }
            // 获取父对象中的变量定义
            if (this.Parent != null) return this.Parent.GetVariableDefine(name);
            // 返回空
            return null;
        }

        /// <summary>
        /// 获取完整的变量名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetFullVariableName(string name) {
            if (this.Name != ScriptEngine.Segment_None) return $"@{this.Index}::{name}";
            return name;
        }

        // 初始化
        private void Init() {
            // 属性初始化
            this.HasTrue = false;
            this.HasFalse = false;
            this.IndexForTrue = -1;
            this.IndexForFalse = -1;
            this.IndexForEnd = -1;
            // 初始化指令集
            this.Codes = new Sevm.Sir.SirCodes();
            // 添加标签指令
            this.Codes.Add(0, SirCodeInstructionTypes.Label, this.Index);
        }

        /// <summary>
        /// 新建一个程序段
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="index"></param>
        /// <param name="name"></param>
        public ScriptSegment(ScriptEngine engine, int index, string name) {
            this.Engine = engine;
            this.Index = index;
            this.Name = name;
            this.ScriptType = ScriptSemanticTypes.None;
            this.Parent = null;
            this.Init();
        }

        /// <summary>
        /// 新建一个程序段
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="tp"></param>
        public ScriptSegment(ScriptEngine engine, int index, string name, ScriptSemanticTypes tp) {
            this.Engine = engine;
            this.Index = index;
            this.Name = name;
            this.ScriptType = tp;
            this.Parent = null;
            this.Init();
        }

        /// <summary>
        /// 新建一个程序段
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public ScriptSegment(ScriptEngine engine, int index, string name, ScriptSegment parent) {
            this.Engine = engine;
            this.Index = index;
            this.Name = name;
            this.ScriptType = ScriptSemanticTypes.None;
            this.Parent = parent;
            this.Init();
        }

        /// <summary>
        /// 新建一个程序段
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="tp"></param>
        /// <param name="parent"></param>
        public ScriptSegment(ScriptEngine engine, int index, string name, ScriptSemanticTypes tp, ScriptSegment parent) {
            this.Engine = engine;
            this.Index = index;
            this.Name = name;
            this.ScriptType = tp;
            this.Parent = parent;
            this.Init();
        }

        /// <summary>
        /// 获取字符串表示形式
        /// </summary>
        /// <returns></returns>
        public new string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append($"代码段 @{Index} {Name} 指令集合[{Codes.Count}]:\r\n");
            for (int i = 0; i < Codes.Count; i++) {
                sb.Append($"    {i}) ");
                sb.Append(Codes[i].ToString());
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

    }
}
