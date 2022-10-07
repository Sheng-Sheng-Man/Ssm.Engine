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
    public class IfEnd : ISemanticStatement {

        // 调试信息
        private List<string> debugs;

        /// <summary>
        /// 获取类型
        /// </summary>
        public ScriptSemanticTypes Type { get { return ScriptSemanticTypes.IfEnd; } }

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
                    // 添加跳转
                    if (!seg.Parent.HasFalse) {
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Label, seg.Parent.IndexForFalse);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Jmp, seg.Parent.IndexForEnd);
                    } else {
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Jmp, seg.Parent.IndexForEnd);
                    }
                    seg = seg.Parent.Parent;
                    break;
                case ScriptSemanticTypes.IfFalse:
                    // 添加跳转
                    if (!seg.Parent.HasTrue) {
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Label, seg.Parent.IndexForTrue);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Jmp, seg.Parent.IndexForEnd);
                    } else {
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Jmp, seg.Parent.IndexForEnd);
                    }
                    seg = seg.Parent.Parent;
                    break;
                case ScriptSemanticTypes.If:
                    // 添加跳转
                    if ((!seg.HasFalse) && (!seg.HasTrue)) throw new SirException(line, 0, "语法错误：意外的结束语句");
                    if (!seg.HasTrue) {
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Label, seg.IndexForTrue);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Jmp, seg.IndexForEnd);
                    } else if (!seg.HasFalse) {
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Label, seg.IndexForFalse);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Jmp, seg.IndexForEnd);
                    } else {
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Jmp, seg.IndexForEnd);
                    }
                    seg = seg.Parent;
                    break;
                default: throw new SirException(line, 0, "语法错误：意外的结束语句");
            }
            return seg;
        }

    }
}
