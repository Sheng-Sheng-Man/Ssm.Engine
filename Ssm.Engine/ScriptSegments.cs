using System;
using System.Collections.Generic;
using System.Text;

namespace Ssm.Engine {

    /// <summary>
    /// 程序段集合
    /// </summary>
    public class ScriptSegments : List<ScriptSegment> {

        /// <summary>
        /// 获取计数器
        /// </summary>
        public ScriptIndexer Indexer { get; private set; }

        /// <summary>
        /// 对象实例化
        /// </summary>
        public ScriptSegments() {
            this.Indexer = new ScriptIndexer(-1);
        }

        /// <summary>
        /// 获取程序段
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ScriptSegment this[string name] {
            get {
                for (int i = 0; i < base.Count; i++) {
                    ScriptSegment seg = base[i];
                    if (seg.Name == name) return seg;
                }
                return null;
            }
        }

        /// <summary>
        /// 检测名称是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsKey(string name) {
            for (int i = 0; i < base.Count; i++) {
                ScriptSegment seg = base[i];
                if (seg.Name == name) return true;
            }
            return false;
        }

    }
}
