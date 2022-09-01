using System;
using System.Collections.Generic;
using System.Text;

namespace Ssm.Engine {

    /// <summary>
    /// 计数器
    /// </summary>
    public class ScriptIndexer {

        // 计数器
        private int indexer;

        /// <summary>
        /// 获取一个新的索引
        /// </summary>
        /// <returns></returns>
        public int GetNewIndex() { indexer++; return indexer; }

        /// <summary>
        /// 建立计数器
        /// </summary>
        /// <param name="index"></param>
        public ScriptIndexer(int index = 0) {
            indexer = index;
        }

    }
}
