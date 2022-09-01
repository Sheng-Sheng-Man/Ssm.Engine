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
    public class TargetCall : ISemanticStatement {

        // 调试信息
        private List<string> debugs;

        /// <summary>
        /// 获取类型
        /// </summary>
        public ScriptSemanticTypes Type { get { return ScriptSemanticTypes.TargetCall; } }

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
            if (strs.Length < 2) throw new SirException(line, 0, "缺少函数名称定义");
            Debug.WriteLine($"{this.Type.ToString()} strs.Length:{strs.Length}");
            string retName = strs[0];
            string funName = strs[1];
            // 添加变量名称调试
            debugs.Add(retName);
            debugs.Add($"Fn {funName}");
            // 组织变量名称
            SirExpression ret = seg.GetValueExpression(retName);
            SirExpression func = engine.GetFunction(funName);
            // 定义参数列表
            int argIndex = 0;
            Dictionary<string, SirExpression> args = new Dictionary<string, SirExpression>();
            // 遍历剩下的
            for (int i = 2; i < strs.Length; i++) {
                string str = strs[i];
                bool isResolved = false;
                Debug.WriteLine($"Define strs[{i}]:{strs[i]}");
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
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, obj);
            seg.Codes.Add(line, SirCodeInstructionTypes.Obj, obj);
            // 获取对象键列表
            seg.Codes.Add(line, SirCodeInstructionTypes.Leak, SirExpression.Register(2), obj);
            SirExpression objKeys = SirExpression.Variable(engine.VariableIndexer.GetNewIndex());
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, objKeys, SirExpression.Register(2));
            // 获取对象值列表
            seg.Codes.Add(line, SirCodeInstructionTypes.Leav, SirExpression.Register(2), obj);
            SirExpression objValues = SirExpression.Variable(engine.VariableIndexer.GetNewIndex());
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, objValues, SirExpression.Register(2));
            string[] keys = args.Keys.ToArray();
            // 绑定键
            for (int i = 0; i < keys.Length; i++) {
                // 创建键字符串表达式
                SirExpression key = engine.GetStringIntPtr(keys[i]);
                // 将键值绑定到键列表中
                seg.Codes.Add(line, SirCodeInstructionTypes.Lea, SirExpression.Register(2), key);
                seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, objKeys, i, SirExpression.Register(2));
            }
            // 绑定值
            for (int i = 0; i < keys.Length; i++) {
                // 将键值绑定到值列表中
                SirExpression arg = args[keys[i]];
                switch (arg.Type) {
                    case SirExpressionTypes.Value:
                        // 新建一个值变量
                        SirExpression value = engine.GetNewVariable();
                        seg.Codes.Add(line, SirCodeInstructionTypes.Lea, SirExpression.Register(2), value);
                        seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, objValues, i, SirExpression.Register(2));
                        seg.Codes.Add(line, SirCodeInstructionTypes.Mov, value, arg);
                        break;
                    case SirExpressionTypes.IntPtr:
                        seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, objValues, i, arg.Content);
                        break;
                    case SirExpressionTypes.Variable:
                        seg.Codes.Add(line, SirCodeInstructionTypes.Lea, SirExpression.Register(2), arg);
                        seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, objValues, i, SirExpression.Register(2));
                        break;
                    default: throw new SirException(line, 0, $"不支持的参数类型'{arg.Type.ToString()}'");
                }
            }
            // 添加参数列表
            SirExpression param = engine.GetNewVariable();
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, param);
            seg.Codes.Add(line, SirCodeInstructionTypes.List, param);
            seg.Codes.Add(line, SirCodeInstructionTypes.Lea, SirExpression.Register(2), obj);
            seg.Codes.Add(line, SirCodeInstructionTypes.Ptrl, param, 0, SirExpression.Register(2));
            seg.Codes.Add(line, SirCodeInstructionTypes.Lea, SirExpression.Register(0), param);
            // 添加执行语句
            seg.Codes.Add(line, SirCodeInstructionTypes.Call, ret, func);
            #endregion
            return seg;
        }

    }
}
