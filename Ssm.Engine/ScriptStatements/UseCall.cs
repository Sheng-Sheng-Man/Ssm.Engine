using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using egg;
using Sevm.Sir;

namespace Ssm.Engine.ScriptStatements {

    /// <summary>
    /// 申请一个变量
    /// </summary>
    public class UseCall : ISemanticStatement {

        // 调试信息
        private List<string> debugs;

        /// <summary>
        /// 获取类型
        /// </summary>
        public ScriptSemanticTypes Type { get { return ScriptSemanticTypes.UseCall; } }

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
            string[] strs = statement.Split(","); ;
            Debug.WriteLine($"{this.Type.ToString()} strs.Length:{strs.Length}");
            string funName = strs[0];
            // 添加变量名称调试
            debugs.Add($"{funName}");
            // 组织变量名称
            SirExpression ret = null;
            SirExpression func = engine.GetFunction(funName);
            // 定义参数列表
            int argIndex = 0;
            Dictionary<string, SirExpression> args = new Dictionary<string, SirExpression>();
            // 遍历剩下的
            for (int i = 1; i < strs.Length; i++) {
                string str = strs[i];
                bool isResolved = false;
                Debug.WriteLine($"Define strs[{i}]:{strs[i]}");
                #region [=====指向语句=====]
                // 指定返回
                if (str.StartsWith("获取的结果给") || str.StartsWith("调用的结果给")) {
                    string targetName = str.Substring(6);
                    // 添加调试
                    debugs.Add($"Ret {targetName}");
                    if (ret != null) throw new SirException(line, 0, "不允许重复定义返回结果");
                    ret = seg.GetValueExpression(targetName);
                    // 设置为解析成功
                    isResolved = true;
                }
                // 指定返回
                if (str.StartsWith("结果给")) {
                    string targetName = str.Substring(3);
                    // 添加调试
                    debugs.Add($"Ret {targetName}");
                    if (ret != null) throw new SirException(line, 0, "不允许重复定义返回结果");
                    ret = seg.GetValueExpression(targetName);
                    // 设置为解析成功
                    isResolved = true;
                }
                #endregion
                #region [=====赋值语句=====]
                // 判断是关键字
                int idx = str.IndexOf("是");
                if (idx > 0) {
                    // 读取定义
                    string targetName = str.Substring(0, idx);
                    string sourceName = str.Substring(idx + 1);
                    debugs.Add($"[{argIndex}]{targetName}:{sourceName}");
                    // 添加参数定义
                    args[targetName] = seg.GetValueExpression(sourceName);
                    argIndex++;
                    // 设置为解析成功
                    isResolved = true;
                }
                // 判断等于关键字
                idx = str.IndexOf("等于");
                if (idx > 0) {
                    string targetName = str.Substring(0, idx);
                    string sourceName = str.Substring(idx + 2);
                    debugs.Add($"[{argIndex}]{targetName}:{sourceName}");
                    // 添加参数定义
                    args[targetName] = seg.GetValueExpression(sourceName);
                    argIndex++;
                    // 设置为解析成功
                    isResolved = true;
                }
                #endregion
                // 未成功解析，则直接作为变量处理
                if (!isResolved) {
                    debugs.Add($"[{argIndex}]:{str}");
                    args[argIndex.ToString()] = seg.GetValueExpression(str);
                    argIndex++;
                }
            }
            #region [=====添加处理指令=====]
            // 添加对象定义
            SirExpression obj = engine.GetNewVariable();
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, obj.Content);
            seg.Codes.Add(line, SirCodeInstructionTypes.Obj, obj.Content);
            // 获取对象键列表
            seg.Codes.Add(line, SirCodeInstructionTypes.Leak, 2, obj.Content);
            SirExpression objKeys = SirExpression.Variable(engine.VariableIndexer.GetNewIndex());
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, objKeys.Content, 2);
            // 获取对象值列表
            seg.Codes.Add(line, SirCodeInstructionTypes.Leav, 2, obj.Content);
            SirExpression objValues = SirExpression.Variable(engine.VariableIndexer.GetNewIndex());
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, objValues.Content, 2);
            string[] keys = args.Keys.ToArray();
            // 绑定键
            for (int i = 0; i < keys.Length; i++) {
                // 创建键字符串表达式
                SirExpression key = engine.GetStringIntPtr(keys[i]);
                // 将键值绑定到键列表中
                seg.Codes.Add(line, SirCodeInstructionTypes.Lea, 2, key.Content);
                seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, objKeys.Content, engine.GetReadonlyValue(i).Content, 2);
            }
            // 绑定值
            for (int i = 0; i < keys.Length; i++) {
                // 将键值绑定到值列表中
                SirExpression arg = args[keys[i]];
                switch (arg.Type) {
                    case SirExpressionTypes.Value:
                        // 新建一个值变量
                        SirExpression value = engine.GetNewVariable();
                        seg.Codes.Add(line, SirCodeInstructionTypes.Lea, 2, value.Content);
                        seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, objValues.Content, engine.GetReadonlyValue(i).Content, 2);
                        seg.Codes.Add(line, SirCodeInstructionTypes.Mov, value.Content, arg.Content);
                        break;
                    case SirExpressionTypes.IntPtr:
                        seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, objValues.Content, engine.GetReadonlyValue(i).Content, arg.Content);
                        break;
                    case SirExpressionTypes.Variable:
                        seg.Codes.Add(line, SirCodeInstructionTypes.Lea, 2, arg.Content);
                        seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, objValues.Content, engine.GetReadonlyValue(i).Content, 2);
                        break;
                    default: throw new SirException(line, 0, $"不支持的参数类型'{arg.Type.ToString()}'");
                }
            }
            // 添加参数列表
            SirExpression param = engine.GetNewVariable();
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, param.Content);
            seg.Codes.Add(line, SirCodeInstructionTypes.List, param.Content);
            seg.Codes.Add(line, SirCodeInstructionTypes.Lea, 3, obj.Content);
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, param.Content, engine.GetReadonlyValue(0).Content, 3);
            seg.Codes.Add(line, SirCodeInstructionTypes.Lea, 1, param.Content);
            // 添加执行语句
            if (ret == null) ret = SirExpression.IntPtr(0);
            seg.Codes.Add(line, SirCodeInstructionTypes.Call, ret.Content, func.Content);
            #endregion
            return seg;
        }

    }
}
