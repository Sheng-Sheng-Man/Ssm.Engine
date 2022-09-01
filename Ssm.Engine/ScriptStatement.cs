using egg.Serializable.Config;
using Sevm.Sir;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Xml.Linq;

namespace Ssm.Engine {

    /// <summary>
    /// 脚本语句
    /// </summary>
    public class ScriptStatement {

        /// <summary>
        /// 获取语句类型
        /// </summary>
        public ScriptStatementTypes Type { get; private set; }

        /// <summary>
        /// 获取脚本引擎
        /// </summary>
        public ScriptEngine Engine { get; private set; }

        /// <summary>
        /// 获取脚本段
        /// </summary>
        public ScriptSegment Segment { get; private set; }

        /// <summary>
        /// 获取源代码行号
        /// </summary>
        public int SourceLine { get; private set; }

        /// <summary>
        /// 获取语句
        /// </summary>
        public string Statement { get; private set; }

        /// <summary>
        /// 获取关联的解析语句
        /// </summary>
        public ISemanticStatement SemanticStatement { get; private set; }

        /// <summary>
        /// 对象实例化
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="engine"></param>
        /// <param name="segment"></param>
        /// <param name="line"></param>
        /// <param name="statement"></param>
        public ScriptStatement(ScriptStatementTypes tp, ScriptEngine engine, ScriptSegment segment, int line, string statement) {
            this.Type = tp;
            this.Engine = engine;
            this.Segment = segment;
            this.SourceLine = line;
            this.Statement = statement;
        }

        // 执行定义解析
        private ScriptSegment ResolveNormal() {
            // 转存语句
            string str = this.Statement;
            // 获取真实的语句
            str = this.Engine.GetRealStatement(str);
            // 特殊类型语句处理
            if (this.Engine.Statements.Count > 1) {
                var semantic = this.Engine.Statements[this.Engine.Statements.Count - 2].SemanticStatement;
                if (semantic != null) {
                    // 一个变量定义
                    if (semantic.Type == ScriptSemanticTypes.Variable) return semantic.Resolve(this.Engine, this.Segment, this.SourceLine, str);
                    // 一些变量定义
                    if (semantic.Type == ScriptSemanticTypes.Variables) return semantic.Resolve(this.Engine, this.Segment, this.SourceLine, str);
                    // 使用库
                    if (semantic.Type == ScriptSemanticTypes.Use) return semantic.Resolve(this.Engine, this.Segment, this.SourceLine, str);
                    // 导入脚本
                    if (semantic.Type == ScriptSemanticTypes.Import) return semantic.Resolve(this.Engine, this.Segment, this.SourceLine, str);
                }
            }
            #region [=====定义语句=====]
            // 定义语句
            if (str.StartsWith("我想要")) {
                this.SemanticStatement = new ScriptStatements.Define();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(3));
            }
            // 定义语句
            if (str.StartsWith("我要定义")) {
                this.SemanticStatement = new ScriptStatements.Define();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(4));
            }
            // 定义语句
            if (str.StartsWith("定义") || str.StartsWith("想要")) {
                this.SemanticStatement = new ScriptStatements.Define();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(2));
            }
            #endregion
            #region [=====函数相关语句=====]
            // 处理调用语句
            if (str.StartsWith("对着") || str.StartsWith("冲着") || str.StartsWith("为了") || str.StartsWith("指向")) {
                this.SemanticStatement = new ScriptStatements.TargetCall();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(2));
            }
            // 定义语句的赋值
            if (str.StartsWith("使用") || str.StartsWith("调用")) {
                this.SemanticStatement = new ScriptStatements.UseCall();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(2));
            }
            #endregion
            #region [=====赋值语句=====]
            // 申请赋值语句
            if (str.StartsWith("我想让")) {
                this.SemanticStatement = new ScriptStatements.Set();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(3));
            }
            // 申请赋值语句
            if (str.StartsWith("想让") || str.StartsWith("设置")) {
                this.SemanticStatement = new ScriptStatements.Set();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(2));
            }
            // 申请赋值语句
            if (str.StartsWith("让")) {
                this.SemanticStatement = new ScriptStatements.Set();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(1));
            }
            #endregion
            #region [=====计算语句=====]
            // 定义算式计算
            if (str.StartsWith("计算")) {
                this.SemanticStatement = new ScriptStatements.Calculate();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(2));
            }
            #endregion
            #region [=====判断语句=====]
            // 结束判断
            if (str == "结束判断" || str == "结束判断定义") {
                this.SemanticStatement = new ScriptStatements.IfEnd();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, null);
            }
            // 结束判断
            if (str == "结束函数" || str == "结束函数定义") {
                this.SemanticStatement = new ScriptStatements.FunctionEnd();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, null);
            }
            // 结束判断
            if (str.StartsWith("结束函数,")) {
                this.SemanticStatement = new ScriptStatements.FunctionEnd();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(5));
            }
            // 结束判断
            if (str.StartsWith("结束函数定义,")) {
                this.SemanticStatement = new ScriptStatements.FunctionEnd();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(7));
            }
            #endregion
            #region [=====跳转语句=====]
            // 跳转语句
            if (str.StartsWith("判断")) {
                this.SemanticStatement = new ScriptStatements.Goto();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(2));
            }
            if (str.StartsWith("重新判断")) {
                this.SemanticStatement = new ScriptStatements.Goto();
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(4));
            }
            #endregion
            // 未知语句，当作函数处理
            this.SemanticStatement = new ScriptStatements.UseCall();
            return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str);
        }

        // 执行定义解析
        private ScriptSegment ResolveDefineIfOrFunction(string str) {
            int idx = str.IndexOf(',');
            if (idx < 0) {
                // 不带参数的函数
                if (str.EndsWith("的函数")) {
                    this.SemanticStatement = new ScriptStatements.Function(str.Substring(0, str.Length - 3), SirScopeTypes.Private);
                    return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, null);
                }
                // 不带参数的函数
                if (str.EndsWith("的公开函数")) {
                    this.SemanticStatement = new ScriptStatements.Function(str.Substring(0, str.Length - 5), SirScopeTypes.Public);
                    return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, null);
                }
                throw new SirException(this.SourceLine, 0, $"不支持的定义对象'{str}'");
            }
            string name = str.Substring(0, idx);
            // 带名称判断语句
            if (name.EndsWith("的判断")) {
                this.SemanticStatement = new ScriptStatements.If(name.Substring(0, name.Length - 3));
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(idx + 1));
            }
            // 带参数的函数定义
            if (name.EndsWith("的函数")) {
                this.SemanticStatement = new ScriptStatements.Function(name.Substring(0, name.Length - 3), SirScopeTypes.Private);
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(idx + 1));
            }
            // 带参数的公开函数定义
            if (name.EndsWith("的公开函数")) {
                this.SemanticStatement = new ScriptStatements.Function(name.Substring(0, name.Length - 5), SirScopeTypes.Public);
                return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(idx + 1));
            }
            return this.Segment;
        }

        // 执行定义解析
        private ScriptSegment ResolveDefine() {
            // 转存语句
            string str = this.Statement;
            if (str.StartsWith("定义") || str.StartsWith("添加") || str.StartsWith("想要") || str.StartsWith("我要") || str.StartsWith("我要定义") || str.StartsWith("我想要")) {
                // 字符串去头  
                if (str.StartsWith("我要定义")) {
                    str = str.Substring(4);
                } else if (str.StartsWith("我想要")) {
                    str = str.Substring(3);
                } else {
                    str = str.Substring(2);
                }
                // 判断语句类型
                switch (str) {
                    case "一个变量": this.SemanticStatement = new ScriptStatements.Variable(); return this.Segment;
                    case "一些变量": this.SemanticStatement = new ScriptStatements.Variables(); return this.Segment;
                    default:
                        // 判断语句
                        if (str.StartsWith("一个判断,")) {
                            this.SemanticStatement = new ScriptStatements.If();
                            return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, str.Substring(5));
                        }
                        // 判断或者函数语句
                        if (str.StartsWith("一个叫做")) {
                            return ResolveDefineIfOrFunction(str.Substring(4));
                        }
                        throw new SirException(this.SourceLine, 0, $"不支持的定义对象'{str}'");
                }
            } else if (str == "使用这些库") {
                this.SemanticStatement = new ScriptStatements.Use();
                return this.Segment;
            } else if (str == "使用库") {
                this.SemanticStatement = new ScriptStatements.Use();
                return this.Segment;
            } else if (str == "导入这些脚本") {
                this.SemanticStatement = new ScriptStatements.Import();
                return this.Segment;
            } else if (str == "导入脚本") {
                this.SemanticStatement = new ScriptStatements.Import();
                return this.Segment;
            } else {
                // 在判断返回内
                var seg = this.Segment;
                bool inIf = seg.ScriptType == ScriptSemanticTypes.If || seg.ScriptType == ScriptSemanticTypes.IfTrue || seg.ScriptType == ScriptSemanticTypes.IfFalse;
                if (inIf) {
                    // 判断区域判定
                    switch (str) {
                        case "满足条件的话":
                        case "满足的话":
                        case "满足":
                            this.SemanticStatement = new ScriptStatements.IfTrue();
                            return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, null);
                        case "不满足条件的话":
                        case "不满足的话":
                        case "不满足":
                            this.SemanticStatement = new ScriptStatements.IfFalse();
                            return this.SemanticStatement.Resolve(this.Engine, this.Segment, this.SourceLine, null);
                        default: throw new SirException(this.SourceLine, 0, $"意外的'{str}'语句");
                    }
                } else {
                    throw new SirException(this.SourceLine, 0, $"意外的'{str}'语句");
                }
            }
            return this.Segment;
        }

        /// <summary>
        /// 执行解析
        /// </summary>
        public ScriptSegment Resolve() {
            switch (this.Type) {
                case ScriptStatementTypes.Normal: // 常规语句
                    return ResolveNormal();
                case ScriptStatementTypes.Define: // 定义语句
                    return ResolveDefine();
                default: throw new SirException(this.SourceLine, 0, $"不支持的语句定义类型'{this.Type.ToString()}'");
            }
            // return this.Segment;
        }

        /// <summary>
        /// 获取字符串表示形式
        /// </summary>
        /// <returns></returns>
        public new string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append($"LINE[{this.SourceLine}] ");
            sb.Append(this.Statement);
            if (this.SemanticStatement != null) {
                sb.Append($" -> ");
                sb.Append(this.SemanticStatement.GetString());
            }
            return sb.ToString();
        }

    }
}
