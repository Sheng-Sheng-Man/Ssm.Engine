using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using egg;
using Sevm.Sir;

namespace Ssm.Engine.ScriptStatements {

    /// <summary>
    /// 判断语句
    /// </summary>
    public class Function : ISemanticStatement {

        // 调试信息
        private List<string> debugs;

        /// <summary>
        /// 获取名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 获取范围类型
        /// </summary>
        public SirScopeTypes ScopeType { get; private set; }

        /// <summary>
        /// 获取类型
        /// </summary>
        public ScriptSemanticTypes Type { get { return ScriptSemanticTypes.Function; } }

        /// <summary>
        /// 获取字符串表示形式
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Type.ToString());
            if (debugs != null) {
                for (int i = 0; i < debugs.Count; i++) {
                    if (i > 0) sb.Append(',');
                    sb.Append(' ');
                    sb.Append(debugs[i]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 执行执行解析
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="segment"></param>
        /// <param name="line"></param>
        /// <param name="statement"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ScriptSegment Resolve(ScriptEngine engine, ScriptSegment segment, int line, string statement) {
            // 新建调试信息
            debugs = new List<string>();
            ScriptSegment seg = segment;
            // 添加调试
            debugs.Add($"{this.ScopeType.ToString()} {this.Name}");
            // 初始化信息
            int labIndex = engine.Segments.Indexer.GetNewIndex();
            for (int i = 0; i < engine.SirScript.Funcs.Count; i++) {
                if (engine.SirScript.Funcs[i].Name == this.Name) throw new SirException(line, 0, $"函数'{this.Name}'重复定义");
            }
            // 向引擎注册函数
            engine.SirScript.Funcs.Add(this.ScopeType, labIndex, this.Name);
            // 创建新的程序段
            seg = new ScriptSegment(engine, labIndex, this.Name, ScriptSemanticTypes.Function, seg);
            engine.Segments.Add(seg);
            // 当存在参数
            if (!statement.IsEmpty()) {
                // 转存语句
                string[] strs = statement.Split(",");
                if (strs[0].Length <= 3) throw new SirException(line, 0, "不规范的参数定义");
                if (!strs[0].StartsWith("参数有")) throw new SirException(line, 0, "不规范的参数定义");
                strs[0] = strs[0].Substring(3);
                // 遍历所有条件
                for (int i = 0; i < strs.Length; i++) {
                    string name = strs[i];
                    debugs.Add($"Param {name}");
                    // 申请定义变量
                    int idx = engine.VariableIndexer.GetNewIndex();
                    // 组织变量名称
                    string varName = seg.GetFullVariableName(name);
                    SirExpression target = SirExpression.Variable(idx);
                    // 添加定义信息及相关指令
                    engine.SirScript.Defines.Add(SirScopeTypes.Private, idx, varName);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, target.Content);
                    // 获取参数信息
                    SirExpression key = engine.GetStringIntPtr(name);
                    var ls = SirExpression.Variable(engine.VariableIndexer.GetNewIndex());
                    var obj = SirExpression.Variable(engine.VariableIndexer.GetNewIndex());
                    var objKeys = SirExpression.Variable(engine.VariableIndexer.GetNewIndex());
                    var objValues = SirExpression.Variable(engine.VariableIndexer.GetNewIndex());
                    var objValue = SirExpression.Variable(engine.VariableIndexer.GetNewIndex());
                    // 获取参数列表
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, ls.Content, 1);
                    // 获取参数中的对象
                    seg.Codes.Add(line, SirCodeInstructionTypes.Leal, 3, ls.Content, 0);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, obj.Content, 3);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Leak, 3, obj.Content);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, objKeys.Content, 3);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Leav, 3, obj.Content);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, objValues.Content, 3);
                    // 获取属性索引
                    seg.Codes.Add(line, SirCodeInstructionTypes.Idx, 3, objKeys.Content, key.Content);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Leal, 4, objValues.Content, 3);
                    // 获取属性值
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, objValue.Content, 4);
                    // 将属性值给变量
                    seg.Codes.Add(line, SirCodeInstructionTypes.Mov, target.Content, objValue.Content);
                }
            }
            return seg;
        }

        /// <summary>
        /// 对象实例化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scope"></param>
        public Function(string name, SirScopeTypes scope) {
            this.Name = name;
            this.ScopeType = scope;
        }

    }
}
