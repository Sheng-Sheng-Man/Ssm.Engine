using System;
using System.Collections.Generic;
using System.Text;
using egg;
using Sevm.Sir;

namespace Ssm.Engine.ScriptStatements {

    /// <summary>
    /// 定义一个变量
    /// </summary>
    public class Use : ISemanticStatement {

        // 调试信息
        private List<string> debugs;

        /// <summary>
        /// 获取类型
        /// </summary>
        public ScriptSemanticTypes Type { get { return ScriptSemanticTypes.Use; } }

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
            // 遍历所有变量定义
            for (int i = 0; i < strs.Length; i++) {
                string name = strs[i];
                // 判断变量是否合法
                if (name.IsEmpty()) throw new SirException(line, 0, "缺少文件定义");
                if (!name.StartsWith("$")) throw new SirException(line, 0, "请使用字符串定义");
                int idx = name.Substring(1).ToInteger();
                for (int j = 0; j < engine.SirScript.Datas.Count; j++) {
                    var data = engine.SirScript.Datas[j];
                    if (data.Index == idx) {
                        string fileName = data.GetString();
                        // 添加调试
                        debugs.Add($"{name}->\"{fileName}\"");
                        engine.SirScript.Imports.Add(SirImportTypes.Use, fileName);
                        engine.SirScript.Datas.RemoveAt(j);
                        break;
                    }
                }
            }
            return segment;
        }

    }
}
