using GalacticARM.CodeGen.Translation.aarch64;
using GalacticARM.Decoding;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using GalacticARM.Runtime.Fallbacks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation
{
    public static class Translator
    {
        public static bool CompileByFunction { get; set; } = true;

        public static bool EnableInstructionCache { get; set; } = false;

        public static string ContextName;
        const string CachePath = @"Cache\";

        static string TCachePath => CachePath + ContextName + "\\";

        public static Dictionary<ulong,GuestFunction> Functions     { get; set; }
        static Dictionary<ulong,ABasicBlock> BasicBlocks            { get; set; }

        static bool IsOpen = false;

        static void InitTranslator()
        {
            if (IsOpen)
                return;

            IsOpen = true;

            Functions = new Dictionary<ulong, GuestFunction>();
            BasicBlocks = new Dictionary<ulong, ABasicBlock>();

            if (EnableInstructionCache)
            {
                if (!Directory.Exists(TCachePath))
                {
                    Directory.CreateDirectory(TCachePath);
                }

                string[] Files = Directory.GetFiles(TCachePath);

                foreach (string file in Files)
                {
                    ulong Address = ulong.Parse(Path.GetFileName(file));

                    GuestFunction function = new GuestFunction(File.ReadAllBytes(file));

                    Functions.Add(Address,function);

                    Console.WriteLine(function);

                    Console.WriteLine(@$"Loaded Function {Path.GetFileName(file)}");
                }

                Console.WriteLine("Loaded Jit Cache!!");
            }
        }

        public static GuestFunction GetOrTranslateFunction(ulong Address, Optimizations optimizations = Optimizations.None)
        {
            InitTranslator();

            GuestFunction Out;

            if (Functions.TryGetValue(Address, out Out))
            {
                return Out;
            }

            Out = TranslateFunction(Address,optimizations);

            lock (Functions)
            {
                if (!Functions.ContainsKey(Address))

                Functions.Add(Address,Out);

                if (EnableInstructionCache)
                {
                    string path = @$"{TCachePath}{Address}";

                    if (!File.Exists(path))
                        File.WriteAllBytes(path, Out.Buffer);
                }
            }

            return Out;
        }

        static ABasicBlock GetOrTranslateBasicBlock(ulong Address)
        {
            ABasicBlock block;

            if (BasicBlocks.TryGetValue(Address,out block))
            {
                return block;
            }

            block = new ABasicBlock(Address);

            lock (BasicBlocks)
            {
                if (!BasicBlocks.ContainsKey(Address))

                BasicBlocks.Add(Address,block);
            }

            return block;
        }

        static GuestFunction TranslateFunction(ulong Address, Optimizations optimizations)
        {
            TranslationContext context = new TranslationContext();

            TranslateFunction(context,Address,optimizations);

            //Console.WriteLine(context);

            return context.CompileFunction();
        }

        static void TranslateFunction(TranslationContext context, ulong Address, Optimizations optimizations)
        {
            if (context.Blocks.ContainsKey(Address))
                return;

            ABasicBlock block = GetOrTranslateBasicBlock(Address);

            context.CurrentBlock = block;

            Operand Label = context.CreateLabel();

            context.MarkLabel(Label);
            context.Blocks.Add(Address,Label);

            context.KnwonReturns = new List<Operand>();

            foreach (AOpCode opCode in block.Instructions)
            {
                //Emit
                context.CurrentSize = IntSize.Int64;
                context.CurrentOpCode = opCode;

                context.Advance();

                opCode.emit(context);

                //Other
                context.CurrentSize = IntSize.Int64;
            }

            Operand CurrentReturn = context.GetRegRaw(nameof(ExecutionContext.Return));

            if (CompileByFunction)
            {
                foreach (Operand kr in context.KnwonReturns)
                {
                    context.CurrentSize = IntSize.Int64;

                    EmitUniversal.EmitIf(context,

                        context.Ceq(context.Const(kr.Data), CurrentReturn),

                        delegate ()
                        {
                            if (!context.Blocks.ContainsKey(kr.Data))
                            {
                                TranslateFunction(context, kr.Data, optimizations);
                            }
                            else
                            {
                                context.Jump(context.Blocks[kr.Data]);
                            }
                        }

                        );
                }
            }

            context.Return(CurrentReturn);
        }
    }
}
