using Ssm.Engine;
using System;
using System.Collections.Generic;
using egg;
using Sevm.Sir;
using System.Text;
using Sevm.Engine;

namespace Ssm {

    /// <summary>
    /// 脚本引擎
    /// </summary>
    public class ScriptEngine : IDisposable {

        /// <summary>
        /// 空代码段
        /// </summary>
        public const string Segment_None = "[None]";

        /// <summary>
        /// 主代码段
        /// </summary>
        public const string Segment_Main = "main";

        // 不允许的首字符
        private const string Unuse_Fisrt_Chars = "0123456789";

        // 不允许的字符
        private const string Unuse_Chars = "@+-*/()[]{}#!！《》，；“”\"'<>?:：||\\/%^`~是的";

        /// <summary>
        /// 检测是否为有效变量名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckVariableName(string name) {
            if (name.IsEmpty()) return false;
            if (Unuse_Fisrt_Chars.IndexOf(name[0]) >= 0) return false;
            for (int i = 0; i < name.Length; i++) {
                char chr = name[i];
                if (Unuse_Chars.IndexOf(chr) >= 0) return false;
            }
            return true;
        }

        /// <summary>
        /// 获取真实的语句
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string GetRealStatement(string str) {
            // 处理虚词
            if (str.StartsWith("先") || str.StartsWith("再")) {
                str = str.Substring(1);
            }
            if (str.StartsWith("首先") || str.StartsWith("然后") || str.StartsWith("接着") || str.StartsWith("最后")) {
                str = str.Substring(2);
            }
            return str;
        }


        /// <summary>
        /// 中间脚本对象
        /// </summary>
        public Sevm.Sir.SirScript SirScript { get; private set; }

        // 虚拟机
        private Sevm.ScriptEngine sevmEngine;

        /// <summary>
        /// 获取所有语义解释
        /// </summary>
        public List<ScriptStatement> Statements { get; private set; }

        /// <summary>
        /// 获取所有段集合
        /// </summary>
        public ScriptSegments Segments { get; private set; }

        /// <summary>
        /// 获取虚拟内存计数器
        /// </summary>
        public ScriptIndexer MemoryIndexer { get; private set; }

        /// <summary>
        /// 获取变量计数器
        /// </summary>
        public ScriptIndexer VariableIndexer { get; private set; }

        // 函数信息缓存
        private ScriptFunctionCache funcs;

        // 标签缓存
        internal Dictionary<string, int> Labels { get; private set; }

        /// <summary>
        /// 获取函数定义表达式
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SirExpression GetFunction(string name) {
            // 优先查找内置函数
            for (int i = 0; i < this.SirScript.Funcs.Count; i++) {
                var func = this.SirScript.Funcs[i];
                if (func.Name == name) return SirExpression.Label(func.Index);
            }
            // 从缓存中获取外部函数定义地址
            if (funcs.ContainsKey(name)) return SirExpression.IntPtr(funcs[name]);
            // 添加新的缓存信息
            int idx = this.VariableIndexer.GetNewIndex();
            this.SirScript.Datas.Add(idx, name);
            return SirExpression.Variable(idx);
        }

        /// <summary>
        /// 从字符串中获取表达式
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        /// <exception cref="SirException"></exception>
        public string GetString(string strValue) {
            // 为空判断
            if (strValue.IsEmpty()) return "";
            if (strValue == "空白") return "";
            if (strValue == "空的") return "";
            if (strValue == "没有东西") return "";
            if (strValue == "真的") return "True";
            if (strValue == "假的") return "False";
            if (strValue == "真") return "True";
            if (strValue == "假") return "False";
            if (strValue.IsDouble()) { // 判断是否为数字
                return strValue;
            } else if (strValue.Length >= 4 && ((strValue.StartsWith("\"'") && strValue.EndsWith("'\"")) || (strValue.StartsWith("““") && strValue.EndsWith("””")))) { // 判断是否为强字符串
                return strValue.Substring(2, strValue.Length - 4);
            } else if ((strValue.StartsWith("\"") && strValue.EndsWith("\"")) || (strValue.StartsWith("“") && strValue.EndsWith("”"))) { // 判断是否为字符串
                return strValue.Substring(1, strValue.Length - 2);
            }
            throw new SirException($"不符合规则的字符串'{strValue}'");
        }

        /// <summary>
        /// 获取一个新的变量
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public SirExpression GetStringIntPtr(string str) {
            // 获取新的虚拟内存索引
            //int ptr = this.MemoryIndexer.GetNewIndex();
            int idx = this.VariableIndexer.GetNewIndex();
            // 添加数据定义
            this.SirScript.Datas.Add(idx, str);
            return SirExpression.Variable(idx);
        }

        /// <summary>
        /// 获取一个新的变量
        /// </summary>
        /// <returns></returns>
        public SirExpression GetNewVariable() {
            int idx = this.VariableIndexer.GetNewIndex();
            return SirExpression.Variable(idx);
        }

        /// <summary>
        /// 获取变量是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsVariable(string name) {
            for (int i = 0; i < this.SirScript.Defines.Count; i++) {
                if (this.SirScript.Defines[i].Name == name) return true;
            }
            return false;
        }

        /// <summary>
        /// 加载脚本
        /// </summary>
        /// <param name="script"></param>
        public void LoadScript(string script) {
            // 申请新的代码段
            ScriptSegment seg = new ScriptSegment(this, this.Segments.Indexer.GetNewIndex(), ScriptEngine.Segment_None, ScriptSemanticTypes.Function);
            this.Segments.Add(seg);
            // 定义临时变量
            StringBuilder sb = new StringBuilder();
            StringBuilder sz = new StringBuilder();
            bool inString = false;
            bool inNote = false;
            int line = 1;
            int offset = 0;
            try {
                for (int i = 0; i < script.Length; i++) {
                    offset++;
                    char chr = script[i];
                    switch (chr) {
                        case '，':
                            // 在注释中
                            if (inNote) { sb.Append(chr); break; }
                            // 在字符串中
                            if (inString) { sz.Append(chr); break; }
                            // 将，转换为,
                            sb.Append(',');
                            break;
                        case ';':
                        case '；':
                        case '。':
                            #region [=====语句结束=====]
                            // 在注释中
                            if (inNote) { sb.Append(chr); break; }
                            // 在字符串中
                            if (inString) { sz.Append(chr); break; }
                            // 获取脚本字符串，并清理缓存
                            string str = sb.ToString();
                            sb.Clear();
                            // 添加语句集合
                            ScriptStatement statement = new ScriptStatement(ScriptStatementTypes.Normal, this, seg, line, str);
                            this.Statements.Add(statement);
                            seg = statement.Resolve();
                            #endregion
                            break;
                        case ':':
                        case '：':
                            #region [=====定义语句=====]
                            // 在注释中
                            if (inNote) { sb.Append(chr); break; }
                            // 在字符串中
                            if (inString) { sz.Append(chr); break; }
                            // 获取脚本字符串，并清理缓存
                            str = sb.ToString();
                            sb.Clear();
                            // 添加语句集合
                            statement = new ScriptStatement(ScriptStatementTypes.Define, this, seg, line, str);
                            this.Statements.Add(statement);
                            seg = statement.Resolve();
                            #endregion
                            break;
                        case '“':
                            #region [=====字符串定义=====]
                            // 在注释中
                            if (inNote) { sb.Append(chr); break; }
                            // 在字符串中
                            if (inString) {
                                // 出现在字符串第二个位置，直接添加
                                if (sz.Length == 1) { sz.Append(chr); break; }
                                // 当以“开头，又并非强强字符串模式时，出错
                                if (sz[0] == '“' && sz[1] != '“') throw new SirException(line, 0, $"意外的'{chr}'字符");
                                // 添加字符
                                sz.Append(chr);
                                break;
                            }
                            sz.Append(chr);
                            inString = true;
                            #endregion
                            break;
                        case '”':
                            #region [=====字符串结束=====]
                            // 在注释中
                            if (inNote) { sb.Append(chr); break; }
                            // 在字符串中
                            if (inString) {
                                // 空字符串的情况，直接退出字符串
                                if (sz.Length == 1 && sz[0] == '“') {
                                    sz.Append(chr);
                                    // 转化为变量
                                    string szStr = sz.ToString();
                                    sz.Clear();
                                    // 添加变量
                                    sb.Append(seg.GetValueExpression(szStr).ToString());
                                    inString = false; break;
                                }
                                // 其他字符串定义时，直接添加
                                if (sz.Length < 2) { sz.Append(chr); break; }
                                if (sz[0] == '“' && sz[1] == '“') { // 强字符串模式
                                    if (sz[sz.Length - 1] == '”') {
                                        // 强字符串定义结束
                                        sz.Append(chr);
                                        // 转化为变量
                                        string szStr = sz.ToString();
                                        sz.Clear();
                                        // 添加变量
                                        sb.Append(seg.GetValueExpression(szStr).ToString());
                                        inString = false; break;
                                    } else {
                                        // 添加字符
                                        sz.Append(chr); break;
                                    }
                                } else if (sz[0] == '“') { // 普通字符串
                                    sz.Append(chr);
                                    // 转化为变量
                                    string szStr = sz.ToString();
                                    sz.Clear();
                                    // 添加变量
                                    sb.Append(seg.GetValueExpression(szStr).ToString());
                                    inString = false; break;
                                }
                                // 添加字符
                                sz.Append(chr); break;
                            }
                            throw new SirException(line, 0, $"意外的'{chr}'字符");
                        #endregion
                        case '"':
                            #region [=====字符串定义=====]
                            // 在注释中
                            if (inNote) { sb.Append(chr); break; }
                            // 在字符串中
                            if (inString) {
                                // 空字符串的情况，直接退出字符串
                                if (sz.Length == 1 && sz[0] == '"') {
                                    sz.Append(chr);
                                    // 转化为变量
                                    string szStr = sz.ToString();
                                    sz.Clear();
                                    // 添加变量
                                    sb.Append(seg.GetValueExpression(szStr).ToString());
                                    inString = false; break;
                                }
                                // 其他字符串定义时，直接添加
                                if (sz.Length < 2) { sz.Append(chr); break; }
                                // 只有一个内容的字符串的情况，直接退出字符串
                                if (sz.Length == 2 && sz[0] == '"') {
                                    sz.Append(chr);
                                    // 转化为变量
                                    string szStr = sz.ToString();
                                    sz.Clear();
                                    // 添加变量
                                    sb.Append(seg.GetValueExpression(szStr).ToString());
                                    inString = false; break;
                                }
                                // 强字符串模式
                                if (sz[0] == '"' && sz[1] == '\"') {
                                    if (sz[sz.Length - 1] == '\'') {
                                        // 强字符串定义结束
                                        sz.Append(chr);
                                        // 转化为变量
                                        string szStr = sz.ToString();
                                        sz.Clear();
                                        // 添加变量
                                        sb.Append(seg.GetValueExpression(szStr).ToString());
                                        inString = false; break;
                                    } else {
                                        // 添加字符
                                        sz.Append(chr); break;
                                    }
                                } else if (sz[0] == '"') { // 普通字符串
                                    sz.Append(chr);
                                    // 转化为变量
                                    string szStr = sz.ToString();
                                    sz.Clear();
                                    // 添加变量
                                    sb.Append(seg.GetValueExpression(szStr).ToString());
                                    inString = false; break;
                                }
                                // 添加字符
                                sz.Append(chr); break;
                            }
                            // 进入字符串模式
                            sz.Append(chr);
                            inString = true;
                            #endregion
                            break;
                        case '\r': break;
                        case '\n':
                            #region [=====语句分隔符=====]
                            // 在注释中
                            if (inNote) {
                                // 感叹号注释时直接退出注释
                                if (sb[0] == '!' || sb[0] == '！') {
                                    inNote = false;
                                    sb.Clear();
                                    line++; offset = 0; break;
                                }
                                // 其他注释支持换行
                                sb.Append(chr);
                                line++; offset = 0; break;
                            }
                            // 在字符串中
                            if (inString) {
                                // 强字符串时可换行
                                if (sz.Length > 2) {
                                    if (sz[0] == '“' && sz[1] == '“') { sz.Append("\n"); line++; offset = 0; break; }
                                    if (sz[0] == '"' && sz[1] == '\'') { sz.Append("\n"); line++; offset = 0; break; }
                                }
                            }
                            // 有内容时
                            if (sb.Length > 0) throw new SirException(line, 0, $"语句尚未结束");
                            // 处理定位信息
                            line++; offset = 0;
                            #endregion
                            break;
                        case ' ':
                            #region [=====语句分隔符=====]
                            // 在注释中
                            if (inNote) { sb.Append(chr); break; }
                            // 在字符串中
                            if (inString) { sz.Append(chr); break; }
                            #endregion
                            break;
                        case '《':
                            #region [=====注释开始=====]
                            // 在字符串中
                            if (inString) { sz.Append(chr); break; }
                            if (inNote) { sb.Append(chr); break; }
                            if (sb.Length > 0) throw new SirException(line, 0, $"意外的'{chr}'字符");
                            // 定义为注释
                            inNote = true;
                            sb.Append(chr);
                            #endregion
                            break;
                        case '》':
                            #region [=====注释结束=====]
                            // 在字符串中
                            if (inString) { sz.Append(chr); break; }
                            if (!inNote) throw new SirException(line, 0, $"意外的'{chr}'字符");
                            if (sb[0] == '《') {
                                // 清理注释状态
                                inNote = false;
                                sb.Clear();
                            } else {
                                sb.Append(chr);
                            }
                            #endregion
                            break;
                        case '#':
                            #region [=====注释开始或者结束=====]
                            // 在字符串中
                            if (inString) { sz.Append(chr); break; }
                            if (inNote) {
                                if (sb[0] == '#') {
                                    // 清理注释状态
                                    inNote = false;
                                    sb.Clear();
                                } else {
                                    sb.Append(chr);
                                }
                                break;
                            }
                            if (sb.Length > 0) throw new SirException(line, 0, $"意外的'{chr}'字符");
                            // 定义为注释
                            inNote = true;
                            sb.Append(chr);
                            #endregion
                            break;
                        case '！':
                        case '!':
                            #region [=====单行注释=====]
                            // 在字符串中
                            if (inString) { sz.Append(chr); break; }
                            if (inNote) { sb.Append(chr); break; }
                            if (sb.Length > 0) throw new SirException(line, 0, $"意外的'{chr}'字符");
                            // 定义为注释
                            inNote = true;
                            sb.Append(chr);
                            #endregion
                            break;
                        default:
                            // 在字符串中
                            if (inString) { sz.Append(chr); break; }
                            sb.Append(chr);
                            break;
                    }
                }
                if (sb.Length > 0) throw new SirException(line, 0, $"尚未结束的脚本:{sb.ToString()}");
            } catch (SirException ex) {
                if (ex.SourceLine > 0) throw ex;
                throw new SirException(line, 0, ex);
            } catch (Exception ex) {
                throw new SirException(line, 0, ex);
            }
            // 将所有程序段依次添加到中间脚本
            line = 0;
            for (int i = 0; i < this.Segments.Count; i++) {
                var sg = this.Segments[i];
                // 判断是否为函数
                if (sg.ScriptType == ScriptSemanticTypes.Function) {
                    if (sg.Name == Segment_None) {
                        if (sg.Codes.Count > 1) {
                            // 添加主函数注册
                            this.SirScript.Funcs.Add(SirScopeTypes.Public, sg.Index, Segment_Main);
                            // 如果没有ret结尾则添加一条指令
                            if (sg.Codes[sg.Codes.Count - 1].Instruction != SirCodeInstructionTypes.Ret) sg.Codes.Add(0, SirCodeInstructionTypes.Ret, 0);
                            // 添加标签指令
                            for (int j = 0; j < sg.Codes.Count; j++) {
                                this.SirScript.Codes.Add(sg.Codes[j]);
                            }
                        }
                    } else {
                        // 如果没有ret结尾则添加一条指令
                        if (sg.Codes[sg.Codes.Count - 1].Instruction != SirCodeInstructionTypes.Ret) sg.Codes.Add(0, SirCodeInstructionTypes.Ret, 0);
                        // 添加标签指令
                        for (int j = 0; j < sg.Codes.Count; j++) {
                            this.SirScript.Codes.Add(sg.Codes[j]);
                        }
                    }
                } else {
                    // 添加标签指令
                    for (int j = 0; j < sg.Codes.Count; j++) {
                        this.SirScript.Codes.Add(sg.Codes[j]);
                    }
                }
            }
        }

        /// <summary>
        /// 对象实例化
        /// </summary>
        public ScriptEngine() {
            // 初始化变量
            this.SirScript = new Sevm.Sir.SirScript();
            sevmEngine = new Sevm.ScriptEngine(this.SirScript);
            this.Statements = new List<ScriptStatement>();
            this.Segments = new ScriptSegments();
            this.MemoryIndexer = new ScriptIndexer();
            this.VariableIndexer = new ScriptIndexer();
            this.funcs = new ScriptFunctionCache();
            this.Labels = new Dictionary<string, int>();
        }

        /// <summary>
        /// 添加查询路径
        /// </summary>
        /// <param name="path"></param>
        public void AddPath(string path) {
            sevmEngine.Paths.Add(path);
        }

        /// <summary>
        /// 执行函数
        /// </summary>
        /// <param name="func"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Sevm.MemoryPtr Execute(string func, Params args = null) {
            return sevmEngine.Execute(func, args, true);
        }

        /// <summary>
        /// 执行主函数
        /// </summary>
        /// <returns></returns>
        public Sevm.MemoryPtr Execute() {
            return sevmEngine.Execute();
        }

        /// <summary>
        /// 获取字符串表示形式
        /// </summary>
        /// <returns></returns>
        public new string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append($"脚本解析集合 [{this.Statements.Count}]:\r\n");
            for (int i = 0; i < this.Statements.Count; i++) {
                sb.Append($"    {i + 1}) ");
                sb.Append(this.Statements[i].ToString());
                sb.Append("\r\n");
            }
            sb.Append("\r\n");
            sb.Append($"脚本引入集合 [{this.SirScript.Imports.Count}]:\r\n");
            for (int i = 0; i < this.SirScript.Imports.Count; i++) {
                sb.Append($"    {i + 1}) ");
                sb.Append(this.SirScript.Imports[i].ToString());
                sb.Append("\r\n");
            }
            sb.Append("\r\n");
            sb.Append($"脚本数据集合 [{this.SirScript.Datas.Count}]:\r\n");
            for (int i = 0; i < this.SirScript.Datas.Count; i++) {
                sb.Append($"    {i + 1}) ");
                sb.Append(this.SirScript.Datas[i].ToString());
                sb.Append("\r\n");
            }
            sb.Append("\r\n");
            sb.Append($"脚本定义集合 [{this.SirScript.Defines.Count}]:\r\n");
            for (int i = 0; i < this.SirScript.Defines.Count; i++) {
                sb.Append($"    {i + 1}) ");
                sb.Append(this.SirScript.Defines[i].ToString());
                sb.Append("\r\n");
            }
            sb.Append("\r\n");
            sb.Append($"虚拟机 SEVM 寄存器 [{sevmEngine.Registers.Count}]:\r\n");
            for (int i = 0; i < sevmEngine.Registers.Count; i++) {
                sb.Append($"    #{i} ");
                sb.Append(sevmEngine.Registers[i]);
                sb.Append("\r\n");
            }
            sb.Append("\r\n");
            sb.Append($"虚拟机 SEVM 变量 [{sevmEngine.Variables.Count}]:\r\n");
            for (int i = 0; i < sevmEngine.Variables.Count; i++) {
                sb.Append($"    ${i} ");
                if (sevmEngine.Variables[i] == null) {
                    sb.Append(" NULL \r\n");
                } else {
                    sb.Append(sevmEngine.Variables[i].Name);
                    sb.Append(" -> [0x");
                    sb.Append(sevmEngine.Variables[i].MemoryPtr.IntPtr.ToString("x"));
                    sb.Append("]\r\n");
                }
            }
            sb.Append("\r\n");
            sb.Append($"虚拟机 SEVM 标签 [{sevmEngine.Labels.Count}]:\r\n");
            for (int i = 0; i < sevmEngine.Labels.Count; i++) {
                sb.Append($"    @{i} ");
                if (sevmEngine.Labels[i] == null) {
                    sb.Append("NULL\r\n");
                } else {
                    sb.Append(sevmEngine.Labels[i].Name);
                    sb.Append(" -> Line:");
                    sb.Append(sevmEngine.Labels[i].IntPtr);
                    sb.Append("\r\n");
                }
            }
            sb.Append("\r\n");
            sb.Append($"虚拟机 SEVM 虚拟内存使用量 [{sevmEngine.Memory.SpaceOccupied}]\r\n");
            return sb.ToString();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose() {
            //throw new NotImplementedException();
            this.SirScript.Dispose();
            sevmEngine.Dispose();
            this.Statements.Clear();
            this.Segments.Clear();
            this.funcs.Clear();
            this.Labels.Clear();
        }
    }
}
