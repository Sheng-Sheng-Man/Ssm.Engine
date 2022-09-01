using System;
using System.Collections.Generic;
using System.Text;

namespace Ssm.Engine {

    /// <summary>
    /// 可解析语句
    /// </summary>
    public interface ISemanticStatement {

        /// <summary>
        /// 获取类型
        /// </summary>
        ScriptSemanticTypes Type { get; }

        /// <summary>
        /// 解析
        /// </summary>
        /// <returns></returns>
        ScriptSegment Resolve(ScriptEngine engine, ScriptSegment segment, int line, string statement);

        /// <summary>
        /// 获取字符串表示形式
        /// </summary>
        /// <returns></returns>
        string GetString();

    }
}
