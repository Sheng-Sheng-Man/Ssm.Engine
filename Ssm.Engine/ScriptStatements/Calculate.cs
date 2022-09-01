using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using egg;
using eggs;
using Sevm.Sir;

namespace Ssm.Engine.ScriptStatements {

    /// <summary>
    /// 申请一个变量
    /// </summary>
    public class Calculate : ISemanticStatement {

        // 调试信息
        private List<string> debugs;

        /// <summary>
        /// 获取类型
        /// </summary>
        public ScriptSemanticTypes Type { get { return ScriptSemanticTypes.Calculate; } }

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
            if (statement.IsEmpty()) throw new SirException(line, 0, "缺少变量名称定义");
            // 转存语句
            string[] strs = statement.Split(",");
            System.Diagnostics.Debug.WriteLine($"{this.Type.ToString()} strs.Length:{strs.Length}");
            string formula = strs[0];
            // 添加变量名称调试
            debugs.Add(formula);
            // 定义变量
            SirExpression target = null;
            SirExpression source = ScriptFormula.Resolve(segment, line, formula);
            if (source == null) target = SirExpression.IntPtr(0);
            // 遍历剩下的
            for (int i = 1; i < strs.Length; i++) {
                string str = strs[i];
                bool isResolved = false;
                System.Diagnostics.Debug.WriteLine($"{this.Type.ToString()} strs[{i}]:{strs[i]}");
                // 获取真实的语句
                str = engine.GetRealStatement(str);
                #region [=====指向语句=====]
                // 指定返回
                if (str.StartsWith("获取的结果给") || str.StartsWith("计算的结果给")) {
                    string targetName = str.Substring(6);
                    if (target != null) throw new SirException(line, 0, "不允许重复定义返回结果");
                    target = seg.GetValueExpression(targetName);
                    // 设置为解析成功
                    isResolved = true;
                }
                // 指定返回
                if (str.StartsWith("结果给")) {
                    string targetName = str.Substring(3);
                    if (target != null) throw new SirException(line, 0, "不允许重复定义返回结果");
                    target = seg.GetValueExpression(targetName);
                    // 设置为解析成功
                    isResolved = true;
                }
                #endregion
                // 未成功解析，则弹出错误
                if (!isResolved) throw new SirException(line, 0, $"不支持的语句'{str}'");
            }
            // 添加指令
            if (!(target.Type == SirExpressionTypes.IntPtr && target.Content == 0)) seg.Codes.Add(line, SirCodeInstructionTypes.Mov, target, source);
            return seg;
        }

    }
}
