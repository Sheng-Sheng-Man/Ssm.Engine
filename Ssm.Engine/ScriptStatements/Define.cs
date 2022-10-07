using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using egg;
using Sevm.Sir;

namespace Ssm.Engine.ScriptStatements {

    /// <summary>
    /// 申请一个变量
    /// </summary>
    public class Define : ISemanticStatement {

        // 调试信息
        private List<string> debugs;

        /// <summary>
        /// 获取类型
        /// </summary>
        public ScriptSemanticTypes Type { get { return ScriptSemanticTypes.Define; } }

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
            Debug.WriteLine($"Define strs.Length:{strs.Length}");
            string name = strs[0];
            // 判断变量是否合法
            if (!engine.CheckVariableName(name)) throw new SirException(line, 0, $"不规范的变量名称'{name}'");
            // 添加变量名称调试
            debugs.Add(name);
            // 申请定义变量
            int target = engine.VariableIndexer.GetNewIndex();
            // 组织变量名称
            string varName = seg.GetFullVariableName(name);
            //SirExpression target = SirExpression.Variable(idx);
            // 添加定义信息及相关指令
            engine.SirScript.Defines.Add(SirScopeTypes.Private, target, varName);
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, target);
            // 遍历剩下的
            for (int i = 1; i < strs.Length; i++) {
                string str = strs[i];
                bool isResolved = false;
                Debug.WriteLine($"Define strs[{i}]:{strs[i]}");
                // 获取真实的语句
                str = engine.GetRealStatement(str);
                #region [=====赋值语句=====]
                // 定义语句的赋值
                if (str.StartsWith("是")) {
                    string sourceName = str.Substring(1);
                    // 添加变量名称调试
                    debugs.Add($"Is {sourceName}");
                    // 定义数据源
                    SirExpression source = seg.GetValueExpression(sourceName);
                    // 添加相关指令
                    seg.Codes.Add(line, SirCodeInstructionTypes.Mov, target, source.Content);
                    // 设置为解析成功
                    isResolved = true;
                }
                // 定义语句的赋值
                if (str.StartsWith("它是") || str.StartsWith("他是") || str.StartsWith("她是") || str.StartsWith("就是") || str.StartsWith("等于")) {
                    string sourceName = str.Substring(2);
                    // 添加变量名称调试
                    debugs.Add($"Is {sourceName}");
                    // 定义数据源
                    SirExpression source = seg.GetValueExpression(sourceName);
                    // 添加相关指令
                    seg.Codes.Add(line, SirCodeInstructionTypes.Mov, target, source.Content);
                    // 设置为解析成功
                    isResolved = true;
                }
                // 定义语句的赋值
                if (str.StartsWith("它等于") || str.StartsWith("他等于") || str.StartsWith("她等于")) {
                    string sourceName = str.Substring(3);
                    // 添加变量名称调试
                    debugs.Add($"Is {sourceName}");
                    // 定义数据源
                    SirExpression source = seg.GetValueExpression(sourceName);
                    // 添加相关指令
                    seg.Codes.Add(line, SirCodeInstructionTypes.Mov, target, source.Content);
                    // 设置为解析成功
                    isResolved = true;
                }
                #endregion
                #region [=====计算语句=====]
                //// 定义算式计算
                //if (str.StartsWith("计算")) {
                //    string source = str.Substring(2);
                //    // 添加一个语句
                //    engineScript.Scripts.Add(new EngineScript() { Type = EngineScriptTypes.Calculate, Source = source });
                //    // 分号为语句结束
                //    if (chr == ';' || chr == '；') { seg = engineScript.Resolve(); engineScript = null; };
                //    break;
                //}
                // 定义加法
                if (str.StartsWith("加上")) {
                    string sourceName = str.Substring(2);
                    // 添加调试
                    debugs.Add($"Add {sourceName}");
                    // 定义数据源
                    SirExpression source = seg.GetValueExpression(sourceName);
                    // 添加相关指令
                    seg.Codes.Add(line, SirCodeInstructionTypes.Add, target, source.Content);
                    // 设置为解析成功
                    isResolved = true;
                }
                // 定义减法
                if (str.StartsWith("减去")) {
                    string sourceName = str.Substring(2);
                    // 添加调试
                    debugs.Add($"Sub {sourceName}");
                    // 定义数据源
                    SirExpression source = seg.GetValueExpression(sourceName);
                    // 添加相关指令
                    seg.Codes.Add(line, SirCodeInstructionTypes.Sub, target, source.Content);
                    // 设置为解析成功
                    isResolved = true;
                }
                // 定义乘法
                if (str.StartsWith("乘以")) {
                    string sourceName = str.Substring(2);
                    // 添加调试
                    debugs.Add($"Mul {sourceName}");
                    // 定义数据源
                    SirExpression source = seg.GetValueExpression(sourceName);
                    // 添加相关指令
                    seg.Codes.Add(line, SirCodeInstructionTypes.Mul, target, source.Content);
                    // 设置为解析成功
                    isResolved = true;
                }
                // 定义除法
                if (str.StartsWith("除以")) {
                    string sourceName = str.Substring(2);
                    // 添加调试
                    debugs.Add($"Div {sourceName}");
                    // 定义数据源
                    SirExpression source = seg.GetValueExpression(sourceName);
                    // 添加相关指令
                    seg.Codes.Add(line, SirCodeInstructionTypes.Div, target, source.Content);
                    // 设置为解析成功
                    isResolved = true;
                }
                // 定义字符串连接
                if (str.StartsWith("连接")) {
                    string sourceName = str.Substring(2);
                    // 添加调试
                    debugs.Add($"Join {sourceName}");
                    // 定义数据源
                    SirExpression source = seg.GetValueExpression(sourceName);
                    // 添加相关指令
                    SirExpression ls = engine.GetNewVariable();
                    // 新建列表
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, ls.Content);
                    seg.Codes.Add(line, SirCodeInstructionTypes.List, ls.Content);
                    // 添加第一个项目
                    seg.Codes.Add(line, SirCodeInstructionTypes.Lea, 3, target);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, ls.Content, engine.GetReadonlyValue(0).Content, 3);
                    // 添加第二个项目
                    seg.Codes.Add(line, SirCodeInstructionTypes.Lea, 3, source.Content);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, ls.Content, engine.GetReadonlyValue(1).Content, 3);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Join, target, ls.Content);
                    // 设置为解析成功
                    isResolved = true;
                }
                #endregion
                // 未成功解析，则弹出错误
                if (!isResolved) throw new SirException(line, 0, $"不支持的语句'{str}'");
            }
            return segment;
        }

    }
}
