// See https://aka.ms/new-console-template for more information
using egg;
using Sevm;
using Sevm.Sir;
using Sevm.Engine;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;
using System.IO;

namespace SsmConsole {

    /// <summary>
    /// 脚本引擎
    /// </summary>
    public static class Program {

        // 输出帮助信息
        private static void Help() {
            System.Console.WriteLine("[参数说明]");
            System.Console.WriteLine();
            System.Console.WriteLine("<path> [options]");
            System.Console.WriteLine();
            System.Console.WriteLine("options:");
            System.Console.WriteLine();
            System.Console.WriteLine("  -? -h --help : 帮助");
            System.Console.WriteLine("  -d --debug : 调试模式");
            System.Console.WriteLine("  -b --sbc : 从汇编文件生成字节码");
            System.Console.WriteLine("  -c --sc : 从字节码翻译生成汇编文件");
            System.Console.WriteLine("  -oc : 根据ssm生成sc文件");
            System.Console.WriteLine("  -ob : 根据ssm生成sbc文件");
            System.Console.WriteLine("  -cb : 根据sc生成sbc文件");
        }

        public static void Main(string[] args) {
            it.Initialize(args, false);
            System.Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (args.Length > 0) {
                string path = args[0];
                if (!path.IsEmpty()) {
                    if (path.Length > 2 && path.StartsWith("\"") && path.EndsWith("\"")) path = path.Substring(1, path.Length - 2);
                    if (path.Length > 2 && path.StartsWith("'") && path.EndsWith("'")) path = path.Substring(1, path.Length - 2);
                    bool isDebug = false;
                    string file = "ssm";
                    for (int i = 1; i < args.Length; i++) {
                        if (args[i] == "-d" || args[i] == "--debug") isDebug = true;
                        if (args[i] == "-b" || args[i] == "--sbc") file = "sbc";
                        if (args[i] == "-c" || args[i] == "--sc") file = "sc";
                        if (args[i] == "-oc") file = "+sc";
                        if (args[i] == "-ob") file = "+sbc";
                        if (args[i] == "-cb") file = "sc+sbc";
                        if (args[i] == "-?" || args[i] == "-h" || args[i] == "--help") file = "help";
                    }
                    System.Console.Title = $"声声慢脚本引擎 Ver:{it.Version} - {path}";
                    string libsPath = $"{it.ExecPath}libs";
                    eggs.IO.CreateFolder(libsPath);
                    switch (file) {
                        case "+sc": // 根据ssm生成sc
                            System.Console.WriteLine($"正在加载文件'{path}'...");
                            using (Ssm.ScriptEngine engine = new Ssm.ScriptEngine()) {
                                engine.LoadScript(eggs.IO.GetUtf8FileContent(path));
                                engine.SirScript.Imports.Add(SirImportTypes.Use, "控制台");
                                string targetPath = it.GetClosedDirectoryPath(System.IO.Path.GetDirectoryName(path)) + System.IO.Path.GetFileNameWithoutExtension(path) + ".sc";
                                egg.File.UTF8File.WriteAllText(targetPath, engine.SirScript.ToString());
                                System.Console.WriteLine($"成功生成汇编文件'{targetPath}'!");
                            }
                            break;
                        case "+sbc": // 根据ssm生成sbc
                            System.Console.WriteLine($"正在加载文件'{path}'...");
                            using (Ssm.ScriptEngine engine = new Ssm.ScriptEngine()) {
                                engine.LoadScript(eggs.IO.GetUtf8FileContent(path));
                                engine.SirScript.Imports.Add(SirImportTypes.Use, "控制台");
                                string targetPath = it.GetClosedDirectoryPath(System.IO.Path.GetDirectoryName(path)) + System.IO.Path.GetFileNameWithoutExtension(path) + ".sbc";
                                egg.File.BinaryFile.WriteAllBytes(targetPath, engine.SirScript.ToBytes());
                                System.Console.WriteLine($"成功生成字节码文件'{targetPath}'!");
                            }
                            break;
                        case "sc+sbc": // 根据sc生成sbc
                            System.Console.WriteLine($"正在加载文件'{path}'...");
                            string script = eggs.IO.GetUtf8FileContent(path);
                            string sbcPath = it.GetClosedDirectoryPath(System.IO.Path.GetDirectoryName(path)) + System.IO.Path.GetFileNameWithoutExtension(path) + ".sbc";
                            using (Sevm.Sir.SirScript ss = Sevm.Sir.Parser.GetScript(script)) {
                                egg.File.BinaryFile.WriteAllBytes(sbcPath, ss.ToBytes());
                                System.Console.WriteLine($"成功生成字节码文件'{sbcPath}'!");
                            }
                            //System.Console.ReadKey();
                            break;
                        case "ssm":
                            #region [=====声声慢脚本=====]
                            using (Ssm.ScriptEngine engine = new Ssm.ScriptEngine()) {
                                try {
                                    // 加载脚本内容
                                    engine.LoadScript(eggs.IO.GetUtf8FileContent(path));
                                    // 添加引用路径
                                    engine.AddPath(it.GetClosedDirectoryPath(System.IO.Path.GetDirectoryName(path)));
                                    engine.AddPath(it.GetClosedDirectoryPath(libsPath));
                                } catch (SirException ex) {
                                    System.Console.ForegroundColor = ConsoleColor.Red;
                                    System.Console.Write($"第{ex.SourceLine}行代码 第{ex.CodeLine}条指令 ");
                                    System.Console.WriteLine(ex);
                                    System.Console.ForegroundColor = ConsoleColor.DarkGreen;
                                } catch (Exception ex) {
                                    System.Console.ForegroundColor = ConsoleColor.Red;
                                    System.Console.WriteLine(ex);
                                    System.Console.ForegroundColor = ConsoleColor.DarkGreen;
                                }
                                // 添加默认的控制台动态库
                                engine.SirScript.Imports.Add(SirImportTypes.Use, "控制台");
                                if (isDebug) {
                                    string tempPath = $"{it.ExecPath}temp";
                                    eggs.IO.CreateFolder(tempPath);
                                    string scTemp = it.GetClosedDirectoryPath(tempPath) + "temp.sc";
                                    egg.File.UTF8File.WriteAllText(scTemp, engine.SirScript.ToString());
                                }
                                if (isDebug) System.Console.WriteLine("==================== 运行前信息 ====================");
                                if (isDebug) Task.Delay(100).Wait();
                                if (isDebug) System.Console.WriteLine();
                                if (isDebug) System.Console.WriteLine(engine.ToString());
                                if (isDebug) System.Console.WriteLine();
                                if (isDebug) Task.Delay(100).Wait();
                                if (isDebug) System.Console.WriteLine("==================== 运行中信息 ====================");
                                if (isDebug) Task.Delay(100).Wait();
                                if (isDebug) System.Console.WriteLine();
                                try {
                                    // 脚本执行
                                    engine.Execute();
                                } catch (SirException ex) {
                                    System.Console.ForegroundColor = ConsoleColor.Red;
                                    System.Console.Write($"第{ex.SourceLine}行代码 第{ex.CodeLine}条指令 ");
                                    System.Console.WriteLine(ex);
                                    System.Console.ForegroundColor = ConsoleColor.DarkGreen;
                                } catch (Exception ex) {
                                    System.Console.ForegroundColor = ConsoleColor.Red;
                                    System.Console.WriteLine(ex);
                                    System.Console.ForegroundColor = ConsoleColor.DarkGreen;
                                }
                                if (isDebug) System.Console.WriteLine();
                                if (isDebug) Task.Delay(100).Wait();
                                if (isDebug) System.Console.WriteLine("==================== 运行后信息 ====================");
                                if (isDebug) Task.Delay(100).Wait();
                                if (isDebug) System.Console.WriteLine();
                                if (isDebug) System.Console.WriteLine(engine.ToString());
                                if (isDebug) System.Console.WriteLine();

                            }
                            System.Console.ReadKey();
                            #endregion
                            break;
                        case "sc":
                            #region [=====Sir脚本=====]
                            script = eggs.IO.GetUtf8FileContent(path);
                            using (Sevm.Sir.SirScript ss = Sevm.Sir.Parser.GetScript(script)) {
                                using (Sevm.ScriptEngine ce = new ScriptEngine(ss)) {
                                    ce.Paths.Add(it.GetClosedDirectoryPath(System.IO.Path.GetDirectoryName(path)));
                                    ce.Paths.Add(it.GetClosedDirectoryPath(libsPath));
                                    //ce.OnRegFunction += Ce_OnRegFunction;
                                    ce.Execute();
                                }
                            }
                            System.Console.ReadKey();
                            #endregion
                            break;
                        case "sbc":
                            #region [=====Sir字节码=====]
                            byte[] bytes = egg.File.BinaryFile.ReadAllBytes(path, false);
                            using (Sevm.Sir.SirScript ss = Sevm.Sir.Parser.GetScript(bytes)) {
                                using (Sevm.ScriptEngine ce = new ScriptEngine(ss)) {
                                    //ce.OnRegFunction += Ce_OnRegFunction;
                                    ce.Paths.Add(it.GetClosedDirectoryPath(System.IO.Path.GetDirectoryName(path)));
                                    ce.Paths.Add(it.GetClosedDirectoryPath(libsPath));
                                    ce.Execute();
                                }
                            }
                            System.Console.ReadKey();
                            #endregion
                            break;
                        case "help": Help(); break;
                        default: throw new SirException($"不支持的文件类型");
                    }
                    return;
                }
            }

            // 进入注册模式
            System.Console.Title = $"声声慢脚本引擎 Ver:{it.Version}";
            // 注册软件路径
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                System.Console.WriteLine("[Windows注册程序]");
                System.Console.WriteLine();
                // Windows
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                var isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (isElevated) {
                    System.Console.Write("正在 注册 ssm 关联文件 ... ");
                    Registry.RegisterFileAssociations(".ssm", "ShengShengMan.Script", "ShengShengMan Chinese Language Programming Script", $"\"{it.ExecFile}\" \"%1\"", $"{it.ExecPath}script.ico");
                    System.Console.Write("正在 注册 调试运行 命令 ... ");
                    Registry.RegisterFileAssociations("ShengShengMan.Script", "调试运行", $"\"{it.ExecFile}\" \"%1\" \"-d\"");
                    System.Console.Write("正在 注册 生成汇编文件 命令 ... ");
                    Registry.RegisterFileAssociations("ShengShengMan.Script", "生成汇编文件", $"\"{it.ExecFile}\" \"%1\" \"-oc\"");
                    System.Console.Write("正在 注册 生成字节码 命令 ... ");
                    Registry.RegisterFileAssociations("ShengShengMan.Script", "生成字节码", $"\"{it.ExecFile}\" \"%1\" \"-ob\"");
                    System.Console.Write("正在 注册 sc 关联文件 ... ");
                    Registry.RegisterFileAssociations(".sc", "Sir.Script", "Script Inter-language", $"\"{it.ExecFile}\" \"%1\" \"-c\"", $"{it.ExecPath}script.ico");
                    System.Console.Write("正在 注册 生成字节码 命令 ... ");
                    Registry.RegisterFileAssociations("Sir.Script", "生成字节码", $"\"{it.ExecFile}\" \"%1\" \"-ocb\"");
                    System.Console.Write("正在 注册 sbc 关联文件 ... ");
                    Registry.RegisterFileAssociations(".sbc", "Sir.Script.Bytecode", "Script Inter-language Bytecode", $"\"{it.ExecFile}\" \"%1\" \"-b\"", $"{it.ExecPath}script.ico");
                    System.Console.WriteLine();
                    // 输出帮助
                    Help();
                } else {
                    System.Console.WriteLine($"使用管理员身份运行可进行Windows文件关联注册！");
                    System.Console.WriteLine();
                    // 输出帮助
                    Help();
                }
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                System.Console.WriteLine("[Linux安装说明]");
                System.Console.WriteLine();
                System.Console.WriteLine($"使用命令行执行'{it.ExecPath}install.sh'可进行进行安装。");
                System.Console.WriteLine();
                // 输出帮助
                Help();
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                // 输出帮助
                Help();
            } else {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"不支持的操作系统'{RuntimeInformation.OSDescription}'");
                System.Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            System.Console.ReadKey();
        }

    }
}
