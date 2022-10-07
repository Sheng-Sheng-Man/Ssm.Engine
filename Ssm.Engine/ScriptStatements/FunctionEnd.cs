using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using egg;
using Sevm.Sir;

namespace Ssm.Engine.ScriptStatements {

    /// <summary>
    /// 函数结束
    /// </summary>
    public class FunctionEnd : ISemanticStatement {

        // 调试信息
        private List<string> debugs;

        /// <summary>
        /// 获取类型
        /// </summary>
        public ScriptSemanticTypes Type { get { return ScriptSemanticTypes.FunctionEnd; } }

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
            if (seg.ScriptType != ScriptSemanticTypes.Function) throw new SirException(line, 0, "语法错误：意外的函数结束语句");
            if (seg.Name == ScriptEngine.Segment_None) throw new SirException(line, 0, "语法错误：意外的函数结束语句");
            if (statement.IsEmpty()) {
                // 添加指令
                seg.Codes.Add(line, SirCodeInstructionTypes.Ret, 0);
            } else {
                // 获取真实的语句
                string str = engine.GetRealStatement(statement);
                if (!str.StartsWith("返回")) throw new SirException(line, 0, "不规范的返回语句");
                string name = str.Substring(2);
                // 添加调试
                debugs.Add($"Ret {name}");
                // 添加指令
                SirExpression ret = seg.GetValueExpression(name);
                seg.Codes.Add(line, SirCodeInstructionTypes.Ret, ret.Content);
            }
            return seg.Parent;
        }

    }
}
