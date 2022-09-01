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
    public class If : ISemanticStatement {

        // 调试信息
        private List<string> debugs;

        // 名称
        private string name;

        /// <summary>
        /// 获取类型
        /// </summary>
        public ScriptSemanticTypes Type { get { return ScriptSemanticTypes.If; } }

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
            if (statement.IsEmpty()) throw new SirException(line, 0, "缺少条件定义");
            // 转存语句
            string[] strs = statement.Split(",");
            if (strs[0].Length <= 3) throw new SirException(line, 0, "缺少条件定义");
            if (!strs[0].StartsWith("条件是")) throw new SirException(line, 0, "缺少条件定义");
            strs[0] = strs[0].Substring(3);
            // 创建新的程序段
            int labIndex = engine.Segments.Indexer.GetNewIndex();
            seg = new ScriptSegment(engine, labIndex, "", ScriptSemanticTypes.If, seg);
            seg.IndexForTrue = engine.Segments.Indexer.GetNewIndex();
            seg.IndexForFalse = engine.Segments.Indexer.GetNewIndex();
            seg.IndexForEnd = engine.Segments.Indexer.GetNewIndex();
            engine.Segments.Add(seg);
            // 添加跳转
            seg.Parent.Codes.Add(line, SirCodeInstructionTypes.Jmp, SirExpression.Label(labIndex));
            seg.Parent.Codes.Add(line, SirCodeInstructionTypes.Label, SirExpression.Label(seg.IndexForEnd));
            // 带名称定义
            if (!name.IsEmpty()) {
                engine.Labels[name] = seg.Index;
            }
            // 初始化连接模式
            int tpLogic = -1;
            // 遍历所有条件
            for (int i = 0; i < strs.Length; i++) {
                string str = strs[i];
                bool isResolved = false;
                Debug.WriteLine($"{this.Type.ToString()} strs[{i}]:{strs[i]}");
                // 获取真实的语句
                str = engine.GetRealStatement(str);
                // 判断连接词
                if (str.StartsWith("同时")) {
                    if (tpLogic != 0) throw new SirException(line, 0, "多余的连接词");
                    tpLogic = 1;
                    str = str.Substring(2);
                    if (str.IsEmpty()) isResolved = true;
                }
                if (str.StartsWith("或者")) {
                    if (tpLogic != 0) throw new SirException(line, 0, "多余的连接词");
                    tpLogic = 1;
                    str = str.Substring(2);
                    if (str.IsEmpty()) isResolved = true;
                }
                #region [=====判断语句=====]
                int idx = str.IndexOf("大于等于");
                if (idx > 0 && !isResolved) {
                    string targetName = str.Substring(0, idx);
                    string sourceName = str.Substring(idx + 4);
                    // 添加相关指令
                    SirExpression target = seg.GetValueExpression(targetName);
                    SirExpression source = seg.GetValueExpression(sourceName);
                    if (tpLogic == -1) { // 普通
                        // 添加调试
                        debugs.Add($"{targetName}>={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Small, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Not, SirExpression.Register(0));
                    } else if (tpLogic == 1) { // and
                        // 添加调试
                        debugs.Add($"And {targetName}>={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Small, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Not, SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.And, SirExpression.Register(0), SirExpression.Register(2));
                    } else if (tpLogic == 2) { // or
                        // 添加调试
                        debugs.Add($"Or {targetName}>={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Small, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Not, SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Or, SirExpression.Register(0), SirExpression.Register(2));
                    } else {
                        throw new SirException(line, 0, "缺少连接模式设定");
                    }
                    tpLogic = 0;
                    // 设置为解析成功
                    isResolved = true;
                }
                idx = str.IndexOf("小于等于");
                if (idx > 0 && !isResolved) {
                    string targetName = str.Substring(0, idx);
                    string sourceName = str.Substring(idx + 4);
                    // 添加相关指令
                    SirExpression target = seg.GetValueExpression(targetName);
                    SirExpression source = seg.GetValueExpression(sourceName);
                    if (tpLogic == -1) { // 普通
                        // 添加调试
                        debugs.Add($"{targetName}<={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Large, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Not, SirExpression.Register(0));
                    } else if (tpLogic == 1) { // and
                        // 添加调试
                        debugs.Add($"And {targetName}<={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Large, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Not, SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.And, SirExpression.Register(0), SirExpression.Register(2));
                    } else if (tpLogic == 2) { // or
                        // 添加调试
                        debugs.Add($"Or {targetName}<={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Large, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Not, SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Or, SirExpression.Register(0), SirExpression.Register(2));
                    } else {
                        throw new SirException(line, 0, "缺少连接模式设定");
                    }
                    tpLogic = 0;
                    // 设置为解析成功
                    isResolved = true;
                }
                idx = str.IndexOf("不等于");
                if (idx > 0 && !isResolved) {
                    string targetName = str.Substring(0, idx);
                    string sourceName = str.Substring(idx + 3);
                    // 添加相关指令
                    SirExpression target = seg.GetValueExpression(targetName);
                    SirExpression source = seg.GetValueExpression(sourceName);
                    if (tpLogic == -1) { // 普通
                        // 添加调试
                        debugs.Add($"{targetName}!={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Equal, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Not, SirExpression.Register(0));
                    } else if (tpLogic == 1) { // and
                        // 添加调试
                        debugs.Add($"And {targetName}!={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Equal, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Not, SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.And, SirExpression.Register(0), SirExpression.Register(2));
                    } else if (tpLogic == 2) { // or
                        // 添加调试
                        debugs.Add($"Or {targetName}!={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Equal, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Not, SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Or, SirExpression.Register(0), SirExpression.Register(2));
                    } else {
                        throw new SirException(line, 0, "缺少连接模式设定");
                    }
                    tpLogic = 0;
                    // 设置为解析成功
                    isResolved = true;
                }
                idx = str.IndexOf("等于");
                if (idx > 0 && !isResolved) {
                    string targetName = str.Substring(0, idx);
                    string sourceName = str.Substring(idx + 2);
                    // 添加相关指令
                    SirExpression target = seg.GetValueExpression(targetName);
                    SirExpression source = seg.GetValueExpression(sourceName);
                    if (tpLogic == -1) { // 普通
                        // 添加调试
                        debugs.Add($"{targetName}={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Equal, SirExpression.Register(0), target, source);
                    } else if (tpLogic == 1) { // and
                        // 添加调试
                        debugs.Add($"And {targetName}={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Equal, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.And, SirExpression.Register(0), SirExpression.Register(2));
                    } else if (tpLogic == 2) { // or
                        // 添加调试
                        debugs.Add($"Or {targetName}={sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Equal, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Or, SirExpression.Register(0), SirExpression.Register(2));
                    } else {
                        throw new SirException(line, 0, "缺少连接模式设定");
                    }
                    tpLogic = 0;
                    // 设置为解析成功
                    isResolved = true;
                }
                idx = str.IndexOf("大于");
                if (idx > 0 && !isResolved) {
                    string targetName = str.Substring(0, idx);
                    string sourceName = str.Substring(idx + 2);
                    // 添加相关指令
                    SirExpression target = seg.GetValueExpression(targetName);
                    SirExpression source = seg.GetValueExpression(sourceName);
                    if (tpLogic == -1) { // 普通
                        // 添加调试
                        debugs.Add($"{targetName}>{sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Large, SirExpression.Register(0), target, source);
                    } else if (tpLogic == 1) { // and
                        // 添加调试
                        debugs.Add($"And {targetName}>{sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Large, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.And, SirExpression.Register(0), SirExpression.Register(2));
                    } else if (tpLogic == 2) { // or
                        // 添加调试
                        debugs.Add($"Or {targetName}>{sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Large, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Or, SirExpression.Register(0), SirExpression.Register(2));
                    } else {
                        throw new SirException(line, 0, "缺少连接模式设定");
                    }
                    tpLogic = 0;
                    // 设置为解析成功
                    isResolved = true;
                }
                idx = str.IndexOf("小于");
                if (idx > 0 && !isResolved) {
                    string targetName = str.Substring(0, idx);
                    string sourceName = str.Substring(idx + 2);
                    // 添加相关指令
                    SirExpression target = seg.GetValueExpression(targetName);
                    SirExpression source = seg.GetValueExpression(sourceName);
                    if (tpLogic == -1) { // 普通
                        // 添加调试
                        debugs.Add($"{targetName}<{sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Small, SirExpression.Register(0), target, source);
                    } else if (tpLogic == 1) { // and
                        // 添加调试
                        debugs.Add($"And {targetName}<{sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Small, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.And, SirExpression.Register(0), SirExpression.Register(2));
                    } else if (tpLogic == 2) { // or
                        // 添加调试
                        debugs.Add($"Or {targetName}<{sourceName}");
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Mov, SirExpression.Register(2), SirExpression.Register(0));
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Small, SirExpression.Register(0), target, source);
                        seg.Codes.Add(line, Sevm.Sir.SirCodeInstructionTypes.Or, SirExpression.Register(0), SirExpression.Register(2));
                    } else {
                        throw new SirException(line, 0, "缺少连接模式设定");
                    }
                    tpLogic = 0;
                    // 设置为解析成功
                    isResolved = true;
                }
                #endregion
                // 未成功解析，则弹出错误
                if (!isResolved) throw new SirException(line, 0, $"不支持的语句'{str}'");
            }
            // 添加调试
            debugs.Add($"Name {name}");
            return seg;
        }

        /// <summary>
        /// 对象实例化
        /// </summary>
        /// <param name="str"></param>
        public If(string str = "") {
            this.name = str;
        }

    }
}
