using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using egg;
using Sevm.Sir;

namespace Ssm.Engine.ScriptStatements {

    /// <summary>
    /// 定义一个变量
    /// </summary>
    public class IfFalse : ISemanticStatement {

        // 调试信息
        private List<string> debugs;

        /// <summary>
        /// 获取类型
        /// </summary>
        public ScriptSemanticTypes Type { get { return ScriptSemanticTypes.IfFalse; } }

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
            // 创建新的程序段
            switch (seg.ScriptType) {
                case ScriptSemanticTypes.IfTrue:
                    if (seg.Parent.HasFalse) throw new SirException(line, 0, "语法错误：意外的不满足语句");
                    // 建立完整标签
                    seg.Parent.HasFalse = true;
                    seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Jmp, SirExpression.Label(seg.Parent.IndexForEnd));
                    seg = new ScriptSegment(engine, seg.Parent.IndexForFalse, "", ScriptSemanticTypes.IfFalse, seg.Parent);
                    engine.Segments.Add(seg);
                    break;
                case ScriptSemanticTypes.If:
                    if (seg.HasFalse) throw new SirException(line, 0, "语法错误：意外的不满足语句");
                    // 建立标签
                    seg.HasFalse = true;
                    seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Jmpf, SirExpression.Register(0), SirExpression.Label(seg.IndexForTrue));
                    seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Jmp, SirExpression.Label(seg.IndexForFalse));
                    seg = new ScriptSegment(engine, seg.IndexForFalse, "", ScriptSemanticTypes.IfFalse, seg);
                    engine.Segments.Add(seg);
                    break;
                default: throw new SirException(line, 0, "语法错误：意外的满足语句");
            }
            return seg;
        }

    }
}
