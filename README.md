# 声声慢汉语言编程脚本

声声慢是一个模拟自然汉语言进行程序设计的脚本语言

## 一 标点符号

声声慢脚本使用贴近汉字书面语言中的标点符号。

### 1.1 字符串定义

使用“”或""来定义普通字符串，普通字符串中不能出现会引起歧义的双引号字符，同时不支持换行

使用““””或"''"来定义强字符串，强字符串中可以出现任何字符，同时支持换行

```
使用这些库：“Sevm.System”；
设置文件内容，他等于““图形处理””；
```

### 1.2 语句标志

使用分号(;；)和句号(。)来表示普通语句的结束

使用冒号(:：)来表示定义性内容的标志性区域划分

使用逗号(,，)来进行断句

```
我想要一个数字；
我想要一些变量：宽度，长度，面积；
```

## 二 语句 

### 2.1 使用外部函数

外部函数分为动态库文件中的函数与其他脚本文件中的函数两种

使用“使用这些库/使用库”定义语句引入动态库文件中的函数

使用“导入这些脚本/导入脚本”定义语句引入其他脚本文件中的函数

```
使用这些库：“Sevm.System”；
导入这些脚本：“图形处理”；
```

### 2.2 变量定义

纯定义：我想要/想要/我要定义/定义+变量名称

带数量的定义：定义一个变量/添加一个变量/想要一个变量/我要一个变量/我要定义一个变量/我想要一个变量:+变量名称

批量定义：定义一些变量/添加一些变量/想要一些变量/我要一些变量/我要定义一些变量/我想要一些变量:+变量名称1，变量名称2，...，变量名称n

定义带赋值和运算：我想要/想要/我要定义/定义+变量名称，它是/他是/她是/就是/等于+值，加上/减去/乘以/除以+值

```
我想要一个数字；
定义长方形的宽，等于10；
我要定义长方形的面积，等于10，乘以5；
添加一个变量：长度；
我想要一些变量：宽度，长度，面积；
```

### 2.3 变量赋值和运算

赋值和运算：我想让/想让/让+变量名称，是/它是/他是/她是/就是/等于+值，加上/减去/乘以/除以+值

```
让一个数字，等于10；
我想让长方形的面积，等于10，乘以5；
```

### 2.4 数学算式支持

数学算式运算：计算+算式，获取的结果给/计算的结果给/结果给+变量名称

```
计算面积=圆周率*(半径*半径);
计算圆周率*(半径*半径)，结果给面积;
```

### 2.5 函数调用

陈述式：使用/调用+函数名，参数列表，获取的结果给/调用的结果给/结果给+变量名称

反向式：对着/为了/冲着/指向+变量名称，函数名，参数列表

省略式：函数名，参数列表，获取的结果给/调用的结果给/结果给+变量名称

```
使用控制台输出，内容是“你好”；
使用控制台读取数字，结果给长方形的宽；
冲着长方形的宽，控制台读取数字；
控制台输出，内容是“你好”；
```

### 2.6 判断语句

不带名称的判断定义：定义一个判断/添加一个判断/想要一个判断/我要一个判断/我要定义一个判断/我想要一个判断，条件是+判断条件1，同时/或者+判断条件2，...，同时/或者+判断条件n

带名称的判断定义：定义一个叫做/添加一个叫做/想要一个叫做/我要一个叫做/我要定义一个叫做/我想要一个叫做+判断名称+的判断，条件是+判断条件

为真语句定义：满足条件的话/满足的话/满足

为假语句定义：不满足条件的话/不满足的话/不满足

结束定义：结束判断/结束判断定义

```
添加一个判断，条件是这个数大于10：
满足条件的话：输出，输出内容是“这个数大于10”；
不满足的话：输出，输出内容是“这个数小于等于10”；
结束判断；
```

### 2.7 循环语句

使用带名称的判断定义，配合“判断/重新判断+判断名称”语句进行循环实现

```
定义一个叫做循环一的判断，条件是临时数小于等于这个数：
满足条件的话：
    让累加和，加上临时数；
    让临时数，加上1；
    然后重新判断循环一；
结束判断定义;
```

### 2.8 函数定义

定义语句：定义一个叫做/添加一个叫做/想要一个叫做/我要一个叫做/我要定义一个叫做/我想要一个叫做+判断名称+的函数，参数有+参数名称1，参数名称2，...，参数名称n

结束定义：结束判断/结束判断定义，返回+返回变量

```
定义一个叫做输出的函数，参数有输出内容：
    使用控制台换行输出，内容是输出内容；
结束函数定义；

定义一个叫做获取测试数据的公开函数：
    定义数据；
    获取测试，结果给数据；
结束函数定义，再返回数据；
```

## 三 基础函数简介

### 3.1 控制台相关函数

* (空白)控制台输出：内容
* (空白)控制台带换行输出：内容
* (数值)控制台读取数字