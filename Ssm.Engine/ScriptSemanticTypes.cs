using System;
using System.Collections.Generic;
using System.Text;

namespace Ssm.Engine {

    /// <summary>
    /// 脚本类型
    /// </summary>
    public enum ScriptSemanticTypes {

        /// <summary>
        /// 空
        /// </summary>
        None = 0x00,

        /// <summary>
        /// 引入
        /// </summary>
        Include = 0x01,

        /// <summary>
        /// 定义
        /// </summary>
        Define = 0x02,

        /// <summary>
        /// 定义一个变量
        /// </summary>
        Variable = 0x03,

        /// <summary>
        /// 定义变量集合
        /// </summary>
        Variables = 0x04,

        /// <summary>
        /// 赋值
        /// </summary>
        Set = 0x11,

        /// <summary>
        /// 计算
        /// </summary>
        Calculate = 0x13,

        /// <summary>
        /// 计算
        /// </summary>
        Judge = 0x14,

        /// <summary>
        /// 加法
        /// </summary>
        Add = 0x21,

        /// <summary>
        /// 减法
        /// </summary>
        Sub = 0x22,

        /// <summary>
        /// 乘法
        /// </summary>
        Mul = 0x23,

        /// <summary>
        /// 除法
        /// </summary>
        Div = 0x24,

        /// <summary>
        /// 连接
        /// </summary>
        Join = 0x31,

        /// <summary>
        /// 等于
        /// </summary>
        Equal = 0x41,

        /// <summary>
        /// 不等于
        /// </summary>
        NotEqual = 0x42,

        /// <summary>
        /// 大于
        /// </summary>
        Large = 0x43,

        /// <summary>
        /// 大于等于
        /// </summary>
        LargeEqual = 0x44,

        /// <summary>
        /// 小于
        /// </summary>
        Small = 0x45,

        /// <summary>
        /// 小于等于
        /// </summary>
        SmallEqual = 0x46,

        /// <summary>
        /// 同时
        /// </summary>
        And = 0x51,

        /// <summary>
        /// 或者
        /// </summary>
        Or = 0x52,

        /// <summary>
        /// 调用
        /// </summary>
        UseCall = 0x0101,

        /// <summary>
        /// 指向
        /// </summary>
        TargetCall = 0x102,

        /// <summary>
        /// 判断语句
        /// </summary>
        If = 0x0201,

        /// <summary>
        /// 判断为真
        /// </summary>
        IfTrue = 0x0202,

        /// <summary>
        /// 判断为假
        /// </summary>
        IfFalse = 0x0203,

        /// <summary>
        /// 判断为假
        /// </summary>
        IfEnd = 0x0204,

        /// <summary>
        /// 简易判断语句
        /// </summary>
        IfSimple = 0x0211,

        /// <summary>
        /// 简易循环语句
        /// </summary>
        Loop = 0x0221,

        /// <summary>
        /// 跳转语句
        /// </summary>
        Goto = 0x0301,

        /// <summary>
        /// 函数定义语句
        /// </summary>
        Function = 0x0401,

        /// <summary>
        /// 函数参数语句
        /// </summary>
        FunctionArg = 0x0402,

        /// <summary>
        /// 函数返回语句
        /// </summary>
        FunctionReturn = 0x0403,

        /// <summary>
        /// 函数结束语句
        /// </summary>
        FunctionEnd = 0x0404,

        /// <summary>
        /// 使用语句
        /// </summary>
        Use = 0x0501,

        /// <summary>
        /// 导入语句
        /// </summary>
        Import = 0x0502,

        /// <summary>
        /// 注释
        /// </summary>
        Note = 0x9999,

    }
}
