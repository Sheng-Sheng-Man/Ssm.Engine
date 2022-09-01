using egg;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ssm.Engine {

    /// <summary>
    /// 引擎脚本
    /// </summary>
    public class Script {

        /// <summary>
        /// 操作类型
        /// </summary>
        public ScriptSemanticTypes Type { get; set; }

        /// <summary>
        /// 目标
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// 源头
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 获取字符串表示形式
        /// </summary>
        /// <returns></returns>
        public new string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append($"({Type.ToString()})");
            sb.Append(Source.IsEmpty() ? "[None]" : Source);
            sb.Append("->");
            sb.Append(Target.IsEmpty() ? "[None]" : Target);
            return sb.ToString();
        }

    }
}
