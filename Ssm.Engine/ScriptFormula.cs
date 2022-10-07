using egg;
using Sevm.Sir;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ssm.Engine {

    /// <summary>
    /// 脚本算式
    /// </summary>
    public class ScriptFormula {

        // 解析算式
        private static SirExpression FormulaResolve(ScriptSegment seg, int line, string str) {
            // 定义等号两边
            SirExpression target = null;
            SirExpression source = null;
            // 定义括号数量
            int left = 0;
            int right = 0;
            // 定义子算式
            StringBuilder sbParent = new StringBuilder();
            StringBuilder sbChild = new StringBuilder();
            #region [=====处理子算式=====]
            for (int i = 0; i < str.Length; i++) {
                char chr = str[i];
                switch (chr) {
                    case '=':
                    case '＝':
                        if (left > 0) throw new SirException("语法错误：多余的等号");
                        string targetName = sbParent.ToString();
                        if (targetName.IsEmpty()) throw new SirException("语法错误：缺少计算结果变量");
                        target = seg.GetValueExpression(targetName);
                        //if (target < 0) throw new SirException($"变量'{targetName}'尚未定义");
                        sbParent.Clear();
                        break;
                    case '（':
                    case '(': //左括号处理
                        if (left == 0 && sbChild.Length > 0) throw new SirException("语法错误：多余的左括号");
                        if (left > 0) sbChild.Append(chr);
                        left++;
                        break;
                    case '）':
                    case ')': //右括号处理
                        right++;
                        if (right > left) throw new SirException("语法错误：多余的右括号");
                        if (right < left) {
                            sbChild.Append(chr);
                        } else {
                            // 添加子算式的运算结果
                            sbParent.Append(FormulaResolve(seg, line, sbChild.ToString()).ToString());
                            // 清理数据
                            sbChild.Clear();
                            left = 0;
                            right = 0;
                        }
                        break;
                    case ' ': //忽略空格
                        break;
                    default:
                        if (left > 0) {
                            sbChild.Append(chr);
                        } else {
                            sbParent.Append(chr);
                        }
                        break;
                }
            }
            #endregion
            // 判断限定语法
            if (left != right || sbChild.Length > 0) throw new SirException("语法错误：多余的括号");
            SirExpression tempVar = null;
            string num1 = null;
            string num2 = null;
            SirCodeInstructionTypes tp = SirCodeInstructionTypes.None;
            #region [=====处理乘除法=====]
            // 处理乘法和除法
            str = sbParent.ToString();
            sbParent.Clear();
            for (int i = 0; i < str.Length; i++) {
                char chr = str[i];
                switch (chr) {
                    case '×':
                    case '*': //乘法
                        if (tempVar != null) {
                            // 当存在临时变量，则代表可以顺序运算
                            // 赋值给第一个数
                            num1 = sbChild.ToString();
                            sbChild.Clear();
                            // 添加运算指令
                            seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num1).Content);
                            //seg.Codes.Add(tp, tempVar, seg.GetValueExpression(num1));
                            // 指定下一次运算的指令类型
                            tp = SirCodeInstructionTypes.Mul;
                            num1 = null;
                            num2 = null;
                        } else {
                            // 不存在临时变量，则代表是新运算的开始
                            if (num1.IsEmpty()) {
                                // 赋值第一个数
                                num1 = sbChild.ToString();
                                sbChild.Clear();
                                tp = SirCodeInstructionTypes.Mul;
                            } else {
                                // 赋值第二个数
                                num2 = sbChild.ToString();
                                sbChild.Clear();
                                // 创建临时变量
                                tempVar = seg.Engine.GetNewVariable();
                                // 添加指令
                                seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, tempVar.Content);
                                seg.Codes.Add(line, SirCodeInstructionTypes.Mov, tempVar.Content, seg.GetValueExpression(num1).Content);
                                //seg.Codes.Add(SirCodeInstructionTypes.Mov, tempVar, seg.GetValueExpression(num1));
                                seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num2).Content);
                                //seg.Codes.Add(tp, tempVar, seg.GetValueExpression(num2));
                                // 指定下一次运算的指令类型
                                tp = SirCodeInstructionTypes.Mul;
                                num1 = null;
                                num2 = null;
                            }
                        }
                        break;
                    case '÷':
                    case '/':
                        if (tempVar != null) {
                            // 当存在临时变量，则代表可以顺序运算
                            // 赋值给第一个数
                            num1 = sbChild.ToString();
                            sbChild.Clear();
                            // 添加运算指令
                            seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num1).Content);
                            //seg.Codes.Add(tp, tempVar, seg.GetValueExpression(num1));
                            // 指定下一次运算的指令类型
                            tp = SirCodeInstructionTypes.Div;
                            num1 = null;
                            num2 = null;
                        } else {
                            // 不存在临时变量，则代表是新运算的开始
                            if (num1.IsEmpty()) {
                                // 赋值第一个数
                                num1 = sbChild.ToString();
                                sbChild.Clear();
                                tp = SirCodeInstructionTypes.Div;
                            } else {
                                // 赋值第二个数
                                num2 = sbChild.ToString();
                                sbChild.Clear();
                                // 创建临时变量
                                tempVar = seg.Engine.GetNewVariable();
                                // 添加指令
                                seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, tempVar.Content);
                                seg.Codes.Add(line, SirCodeInstructionTypes.Mov, tempVar.Content, seg.GetValueExpression(num1).Content);
                                seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num2).Content);
                                // 指定下一次运算的指令类型
                                tp = SirCodeInstructionTypes.Div;
                                num1 = null;
                                num2 = null;
                            }
                        }
                        break;
                    case '＋':
                    case '+':
                    case '—':
                    case '-':
                        if (tempVar != null) {
                            // 当存在临时变量，则代表可以顺序运算
                            // 赋值给第一个数
                            num1 = sbChild.ToString();
                            sbChild.Clear();
                            // 添加运算指令
                            seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num1).Content);
                            // 将临时变量添加至算式中
                            sbParent.Append(tempVar.ToString());
                            // 跳出运算
                            tp = SirCodeInstructionTypes.None;
                            num1 = null;
                            num2 = null;
                            tempVar = null;
                        } else {
                            // 清空缓存
                            sbParent.Append(sbChild);
                            sbChild.Clear();
                            sbParent.Append(chr);
                        }
                        break;
                    default:
                        sbChild.Append(chr);
                        break;
                }
            }
            // 乘除法结束时处理
            if (tempVar != null) {
                // 当存在临时变量，则代表可以顺序运算
                // 赋值给第一个数
                num1 = sbChild.ToString();
                sbChild.Clear();
                // 添加运算指令
                seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num1).Content);
                // 将临时变量添加至算式中
                sbParent.Append(tempVar.ToString());
                // 跳出运算
                tp = SirCodeInstructionTypes.None;
                num1 = null;
                num2 = null;
                tempVar = null;
            } else {
                // 不存在临时变量，则代表是新运算的开始
                if (num1.IsEmpty()) {
                    // 清空缓存
                    sbParent.Append(sbChild);
                    sbChild.Clear();
                } else {
                    // 赋值第二个数
                    num2 = sbChild.ToString();
                    sbChild.Clear();
                    // 创建临时变量
                    tempVar = seg.Engine.GetNewVariable();
                    // 添加指令
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, tempVar.Content);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Mov, tempVar.Content, seg.GetValueExpression(num1).Content);
                    seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num2).Content);
                    // 将临时变量添加至算式中
                    sbParent.Append(tempVar.ToString());
                    // 指定下一次运算的指令类型
                    tp = SirCodeInstructionTypes.None;
                    num1 = null;
                    num2 = null;
                    tempVar = null;
                }
            }
            #endregion
            #region [=====处理加减法=====]
            str = sbParent.ToString();
            sbParent.Clear();
            for (int i = 0; i < str.Length; i++) {
                char chr = str[i];
                switch (chr) {
                    case '＋':
                    case '+': //乘法
                        if (tempVar != null) {
                            // 当存在临时变量，则代表可以顺序运算
                            // 赋值给第一个数
                            num1 = sbChild.ToString();
                            sbChild.Clear();
                            // 添加运算指令
                            seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num1).Content);
                            // 指定下一次运算的指令类型
                            tp = SirCodeInstructionTypes.Add;
                        } else {
                            // 不存在临时变量，则代表是新运算的开始
                            if (num1.IsEmpty()) {
                                // 赋值第一个数
                                num1 = sbChild.ToString();
                                sbChild.Clear();
                                tp = SirCodeInstructionTypes.Add;
                            } else {
                                // 赋值第二个数
                                num2 = sbChild.ToString();
                                sbChild.Clear();
                                // 创建临时变量
                                tempVar = seg.Engine.GetNewVariable();
                                // 添加指令
                                seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, tempVar.Content);
                                seg.Codes.Add(line, SirCodeInstructionTypes.Mov, tempVar.Content, seg.GetValueExpression(num1).Content);
                                seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num2).Content);
                                // 指定下一次运算的指令类型
                                tp = SirCodeInstructionTypes.Add;
                            }
                        }
                        break;
                    case '—':
                    case '-':
                        if (tempVar != null) {
                            // 当存在临时变量，则代表可以顺序运算
                            // 赋值给第一个数
                            num1 = sbChild.ToString();
                            sbChild.Clear();
                            // 添加运算指令
                            seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num1).Content);
                            // 指定下一次运算的指令类型
                            tp = SirCodeInstructionTypes.Div;
                        } else {
                            // 不存在临时变量，则代表是新运算的开始
                            if (num1.IsEmpty()) {
                                // 赋值第一个数
                                num1 = sbChild.ToString();
                                sbChild.Clear();
                                tp = SirCodeInstructionTypes.Div;
                            } else {
                                // 赋值第二个数
                                num2 = sbChild.ToString();
                                sbChild.Clear();
                                // 创建临时变量
                                tempVar = seg.Engine.GetNewVariable();
                                // 添加指令
                                seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, tempVar.Content);
                                seg.Codes.Add(line, SirCodeInstructionTypes.Mov, tempVar.Content, seg.GetValueExpression(num1).Content);
                                seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num2).Content);
                                // 指定下一次运算的指令类型
                                tp = SirCodeInstructionTypes.Div;
                            }
                        }
                        break;
                    default:
                        sbChild.Append(chr);
                        break;
                }
            }
            // 最后处理
            if (tempVar != null) {
                // 当存在临时变量，则代表可以顺序运算
                // 赋值给第一个数
                num1 = sbChild.ToString();
                sbChild.Clear();
                // 添加运算指令
                seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num1).Content);
                source = tempVar;
            } else {
                // 不存在临时变量，则代表是新运算的开始
                if (num1.IsEmpty()) {
                    // 清空缓存
                    sbParent.Append(sbChild);
                    sbChild.Clear();
                    source = seg.GetValueExpression(sbParent.ToString());
                    //source = GetPtr(seg, sbParent.ToString());
                } else {
                    // 赋值第二个数
                    num2 = sbChild.ToString();
                    sbChild.Clear();
                    // 创建临时变量
                    tempVar = seg.Engine.GetNewVariable();
                    // 添加指令
                    seg.Codes.Add(line, SirCodeInstructionTypes.Ptr, tempVar.Content);
                    seg.Codes.Add(line, SirCodeInstructionTypes.Mov, tempVar.Content, seg.GetValueExpression(num1).Content);
                    seg.Codes.Add(line, tp, tempVar.Content, seg.GetValueExpression(num2).Content);
                    source = tempVar;
                }
            }
            #endregion
            // 判断是否为完整算式
            if (target != null) {
                seg.Codes.Add(line, SirCodeInstructionTypes.Mov, target.Content, source.Content);
                //seg.Mov(target, source, true);
                return null;
            }
            return source;
        }

        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="line"></param>
        /// <param name="formula"></param>
        /// <returns></returns>
        public static SirExpression Resolve(ScriptSegment seg, int line, string formula) {
            return FormulaResolve(seg, line, formula);
        }

    }
}
