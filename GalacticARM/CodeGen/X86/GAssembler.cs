using GalacticARM.CodeGen.Translation;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using GalacticARM.Runtime.X86;
using Iced.Intel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static Iced.Intel.AssemblerRegisters;

namespace GalacticARM.CodeGen.X86
{
    public delegate void EmitInstruction();

    public class GuestRegister
    {
        public int Host;
        public int Guest;
        public bool Loaded => Guest != -1;

        public bool Locked;

        public RequestType Type;
    }

    public enum RequestType
    {
        Read,
        Write,
        All
    }

    public delegate void LoadStore(GuestRegister reg);

    public class RegisterAllocator
    {
        public LoadStore Load;
        public LoadStore Store;

        public GuestRegister[] Guests;

        public RegisterAllocator(int HostSize)
        {
            Guests = new GuestRegister[HostSize];

            for (int i = 0; i < Guests.Length; i++)
            {
                Guests[i] = new GuestRegister();

                Guests[i].Host = i;
                Guests[i].Guest = -1;
            }
        }

        public void UnloadRegister(GuestRegister reg)
        {
            if (reg.Loaded)
            {
                Store(reg);
            }

            reg.Guest = -1;
            reg.Locked = false;
        }

        public void LoadRegister(GuestRegister reg, int guest, RequestType type)
        {
            if (reg.Loaded)
            {
                UnloadRegister(reg);
            }

            reg.Guest = guest;
            reg.Locked = true;

            if (type != RequestType.Write)
                Load(reg);
        }

        int spill = 0;

        public int AllocateRegister(int Guest, RequestType type)
        {
            foreach (GuestRegister reg in Guests)
            {
                if (!reg.Loaded)
                {
                    LoadRegister(reg, Guest, type);

                    return reg.Host;
                }

                if (reg.Guest == Guest)
                {
                    reg.Locked = true;

                    return reg.Host;
                }
            }

            while (true)
            {
                spill++;

                if (!(spill < Guests.Length))
                {
                    spill = 0;
                }

                if (!Guests[spill].Locked)
                {
                    LoadRegister(Guests[spill], Guest, type);

                    return spill;
                }
            }

            throw new Exception();
        }

        public void UnlockAllRegisters()
        {
            foreach (GuestRegister reg in Guests)
            {
                reg.Locked = false;
            }
        }

        public void UnloadAllRegisters()
        {
            foreach (GuestRegister reg in Guests)
            {
                UnloadRegister(reg);
            }
        }
    }

    public class GAssembler
    {
        OperationBlock SourceBlock { get; set; }
        Assembler c { get; set; }

        ControlFlowGraph cfg;

        List<int> ToBeCompiled = new List<int>();

        #region InitStuff
        void Init(OperationBlock block)
        {
            cfg = new ControlFlowGraph(block);

            Optimizers.RunMoveOpt(cfg);

            SourceBlock = block;

            c = new Assembler(64);

            c.mov(r15, rcx);

            c.push(rbx);
            c.push(rbp);
            c.push(rsi);
            c.push(rdi);

            c.sub(rsp, 0x58);

            Compiled = new HashSet<int>();

            foreach (Node node in cfg.Nodes)
            {
                Labels.Add((int)node.BaseAddress, c.CreateLabel());
            }

            CompileBasicBlock(0);

            foreach (Node node in cfg.Nodes)
            {
                if (node.BaseAddress != 0)
                {
                    CompileBasicBlock((int)node.BaseAddress);
                }
            }
        }

        public GAssembler(OperationBlock block)
        {
            Emit = new Dictionary<IntermediateRepresentation.Instruction, EmitInstruction>()
            {
                {IntermediateRepresentation.Instruction.Add,EmitAdd},
                {IntermediateRepresentation.Instruction.And,EmitAnd},
                {IntermediateRepresentation.Instruction.Call,EmitCall},
                {IntermediateRepresentation.Instruction.Ceq,EmitCeq},
                {IntermediateRepresentation.Instruction.Cgt,EmitCgt},
                {IntermediateRepresentation.Instruction.Cgt_Un,EmitCgt_Un},
                {IntermediateRepresentation.Instruction.Cgte,EmitCgte},
                {IntermediateRepresentation.Instruction.Cgte_Un,EmitCgte_Un},
                {IntermediateRepresentation.Instruction.Clt,EmitClt},
                {IntermediateRepresentation.Instruction.Clt_Un,EmitClt_Un},
                {IntermediateRepresentation.Instruction.Clte,EmitClte},
                {IntermediateRepresentation.Instruction.Clte_Un,EmitClte_Un},
                {IntermediateRepresentation.Instruction.Divide,EmitDivide},
                {IntermediateRepresentation.Instruction.Divide_Un,EmitDivide_Un},
                {IntermediateRepresentation.Instruction.Jump,EmitJump},
                {IntermediateRepresentation.Instruction.JumpIf,EmitJumpIf},
                {IntermediateRepresentation.Instruction.Load64,EmitLoad64},
                {IntermediateRepresentation.Instruction.Load16,EmitLoad16},
                {IntermediateRepresentation.Instruction.Load32,EmitLoad32},
                {IntermediateRepresentation.Instruction.Load8,EmitLoad8},
                {IntermediateRepresentation.Instruction.LoadContext,EmitLoadContext},
                {IntermediateRepresentation.Instruction.Move,EmitMove},
                {IntermediateRepresentation.Instruction.Multiply,EmitMultiply},
                {IntermediateRepresentation.Instruction.Not,EmitNot},
                {IntermediateRepresentation.Instruction.Or,EmitOr},
                {IntermediateRepresentation.Instruction.Return,EmitReturn},
                {IntermediateRepresentation.Instruction.ShiftLeft,EmitShiftLeft},
                {IntermediateRepresentation.Instruction.ShiftRight,EmitShiftRight},
                {IntermediateRepresentation.Instruction.ShiftRight_Singed,EmitShiftRight_Singed},
                {IntermediateRepresentation.Instruction.SignExtend16,EmitSignExtend16},
                {IntermediateRepresentation.Instruction.SignExtend32,EmitSignExtend32},
                {IntermediateRepresentation.Instruction.SignExtend8,EmitSignExtend8},

                {IntermediateRepresentation.Instruction.Store16,EmitStore16},
                {IntermediateRepresentation.Instruction.Store32,EmitStore32},
                {IntermediateRepresentation.Instruction.Store64,EmitStore64},
                {IntermediateRepresentation.Instruction.Store8,EmitStore8},
                {IntermediateRepresentation.Instruction.Subtract,EmitSubtract},
                {IntermediateRepresentation.Instruction.Xor,EmitXor},

                {IntermediateRepresentation.Instruction.Vector_ClearVector, EmitVector_ClearVector},
                {IntermediateRepresentation.Instruction.Vector_SetVectorElement,EmitVector_SetVectorElement },
                {IntermediateRepresentation.Instruction.Vector_Extract,EmitVector_Extract},
                {IntermediateRepresentation.Instruction.Vector_Move,EmitVector_Move },
                {IntermediateRepresentation.Instruction.Vector_Load,EmitVector_Load },
                {IntermediateRepresentation.Instruction.Vector_Store,EmitVector_Store },

                {IntermediateRepresentation.Instruction.Vector_And,EmitVector_And },
                {IntermediateRepresentation.Instruction.Vector_Or,EmitVector_Orr },
                {IntermediateRepresentation.Instruction.Vector_Xor,EmitVector_Xor },

                {IntermediateRepresentation.Instruction.Vector_ConvertToFloat,EmitVector_ConvertToFloat },
                {IntermediateRepresentation.Instruction.Vector_ConvertToInt,EmitVector_ConvertToInt },
                {IntermediateRepresentation.Instruction.Vector_ScalarOperation,EmitVector_ScalarOperation },
                {IntermediateRepresentation.Instruction.Vector_FloatVectorOperation,EmitVector_FloatVectorOperation },

                {IntermediateRepresentation.Instruction.HardPC, EmitHardPC}

            };

            Init(block);
        }

        HashSet<int> Compiled;
        Dictionary<int, Label> Labels = new Dictionary<int, Label>();

        public void EndAll()
        {
            UnloadAllRegisters();

            c.add(rsp, 0x58);
            c.pop(rdi);
            c.pop(rsi);
            c.pop(rbp);
            c.pop(rbx);
        }

        void CompileBasicBlock(int Address)
        {
            if (Compiled.Contains(Address))
            {
                return;
            }

            Node node = cfg.GetBlock(Address);

            Compiled.Add(Address);

            Label l = Labels[Address];

            c.nop();

            c.Label(ref l);

            InitRegAllocator();

            OperationBlock block = node.BasicBlock;

            foreach (Operation operation in block.Operations)
            {
                CurrentOperation = operation;

                Emit[operation.Instruction]();

                UnlockAllRegisters();
            }
        }

        static object JitLock = new object();

        public GuestFunction Compile()
        {
            const ulong RIP = 0x1234_5678_1000_0000;
            var stream = new MemoryStream();
            c.Assemble(new StreamCodeWriter(stream), RIP);

            byte[] Out = new byte[stream.Length];

            Buffer.BlockCopy(stream.GetBuffer(), 0, Out, 0, Out.Length);

            GuestFunction Out_F = new GuestFunction(Out);

            return Out_F;
        }
        #endregion

        #region RegisterAllocator

        RegisterAllocator BaseAllocator;
        RegisterAllocator VectorAllocator;

        int HostCount;

        void InitRegAllocator()
        {
            HostCount = _64.Length;

            BaseAllocator = new RegisterAllocator(HostCount);
            VectorAllocator = new RegisterAllocator(_Xmm.Length);

            BaseAllocator.Load = LoadReg;
            BaseAllocator.Store = StoreReg;

            VectorAllocator.Load = LoadVector;
            VectorAllocator.Store = StoreVector;
        }

        Operation CurrentOperation;

        Dictionary<IntermediateRepresentation.Instruction, EmitInstruction> Emit;

        static AssemblerRegister64[] _64 = new AssemblerRegister64[]
        {
            rax, 
            //rcx,
            rdx,
            rbx,
            //rsp,
            rbp,
            rsi,
            rdi,
            r8,
            r9,
            r10,
            r11,
            r12,
            r13,
            r14
        };

        static AssemblerRegister32[] _32 = new AssemblerRegister32[]
        {
            eax,     
            //ecx,    
            edx,
            ebx,     
            //esp,   
            ebp,
            esi,
            edi,
            r8d,
            r9d,
            r10d,
            r11d,
            r12d,
            r13d,
            r14d
        };

        static AssemblerRegister8[] _8 = new AssemblerRegister8[]
        {
            al,
            //cl,
            dl,
            bl,
            //spl,
            bpl,
            sil,
            dil,
            r8b,
            r9b,
            r10b,
            r11b,
            r12b,
            r13b,
            r14b
        };

        static AssemblerRegister16[] _16 = new AssemblerRegister16[]
        {
            ax,
            //cx,
            dx,
            bx,
            //sp,
            bp,
            si,
            di,
            r8w,
            r9w,
            r10w,
            r11w,
            r12w,
            r13w,
            r14w
        };

        static AssemblerRegisterXMM[] _Xmm = new AssemblerRegisterXMM[]
        {
            xmm0,
            xmm1,
            xmm2,
            xmm3,
            xmm4,
            xmm5,
            xmm6,
            xmm7,
            xmm8,
            xmm9,
            xmm10,
            xmm11,
            xmm12,
            xmm13,
            xmm14,
            xmm15,
        };

        int GetGuestReg(int guest, RequestType type = RequestType.All) => BaseAllocator.AllocateRegister(guest, type);

        void UnloadAllRegisters()
        {
            BaseAllocator.UnloadAllRegisters();
            VectorAllocator.UnloadAllRegisters();
        }

        void UnlockAllRegisters()
        {
            BaseAllocator.UnlockAllRegisters();
            VectorAllocator.UnlockAllRegisters();
        }

        dynamic GetRegPtr(int Guest)
        {
            //c.mov(r14, (Guest * 8));

            return __[r15 + (Guest * 8)];
        }

        dynamic GetVecPtr(int Guest)
        {
            return __[r15 + (ExecutionContext.VectorOffset + (Guest * 16))];
        }

        void LoadReg(GuestRegister reg)
        {
            c.mov(_64[reg.Host], GetRegPtr(reg.Guest));
        }

        void StoreReg(GuestRegister reg)
        {
            c.mov(GetRegPtr(reg.Guest), _64[reg.Host]);
        }

        void LoadVector(GuestRegister reg)
        {
            c.vmovupd(_Xmm[reg.Host], GetVecPtr(reg.Guest));
        }

        void StoreVector(GuestRegister reg)
        {
            c.vmovupd(GetVecPtr(reg.Guest), _Xmm[reg.Host]);
        }

        #endregion

        #region Emit

        dynamic GetArgument(int index, RequestType type = RequestType.All, bool AllowImm = true, bool FullImm = false)
        {
            Operand o = CurrentOperation.Operands[index];

            if (type == RequestType.All && index == 0 && CurrentOperation.Operands.Length != 1 && o.Type != OperandType.VectorRegister)
                type = RequestType.Write;

            if (o.Type == OperandType.Register)
            {
                int reg = GetGuestReg((int)o.Data, type);

                if (CurrentOperation.Size == IntSize.Int32)
                {
                    return _32[reg];
                }
                else if (CurrentOperation.Size == IntSize.Int64 || CurrentOperation.Size == IntSize.NULL)
                {
                    return _64[reg];
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (o.Type == OperandType.Immediate)
            {
                if (AllowImm)
                {
                    if (FullImm)
                    {
                        if (CurrentOperation.Size == IntSize.Int32)
                            return (uint)o.Data;

                        return o.Data;
                    }

                    if (o.Data < (uint.MaxValue >> 1))
                    {
                        return (int)o.Data;
                    }
                }

                c.mov(rcx, o.Data);

                if (CurrentOperation.Size == IntSize.Int32)
                    return ecx;

                return rcx;
            }
            else if (o.Type == OperandType.VectorRegister)
            {
                int reg = VectorAllocator.AllocateRegister((int)o.Data, type);

                return _Xmm[reg];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        int GetRegArgument(int index, RequestType type = RequestType.All)
        {
            Operand o = CurrentOperation.Operands[index];

            if (o.Type != OperandType.Register)
                throw new NotImplementedException();

            if (type == RequestType.All && index == 0 && CurrentOperation.Operands.Length != 1)
                type = RequestType.Write;

            return GetGuestReg((int)o.Data, type);
        }

        public void EmitAdd() => c.add(GetArgument(0, RequestType.Write), GetArgument(1, RequestType.Read, true));

        public void EmitAnd() => c.and(GetArgument(0, RequestType.Write), GetArgument(1, RequestType.Read, true));

        public void EmitCall()
        {
            UnloadAllRegisters();

            Operand Func = CurrentOperation.Operands[0];

            if (Func.Type != OperandType.Register)
            {
                throw new NotImplementedException();
            }

            int offset = 0x58;

            c.push(r15);
            c.sub(rsp, offset);

            c.mov(r14, GetRegPtr((int)Func.Data));

            for (int i = 1; i < CurrentOperation.Operands.Length; i++)
            {
                int a = i - 1;

                dynamic ra = GetRegPtr((int)CurrentOperation.Operands[i].Data);//GetArgument(i);

                if (a == 0)
                {
                    c.mov(rcx, ra);
                }
                else if (a == 1)
                {
                    c.mov(rdx, ra);
                }
                else if (a == 2)
                {
                    c.mov(r8, ra);
                }
                else if (a == 3)
                {
                    c.mov(r9, ra);
                }
                else
                {
                    Console.WriteLine(CurrentOperation);

                    throw new Exception();
                }
            }

            c.call(r14);

            c.add(rsp, offset);
            c.pop(r15);

            c.mov(__[r15 + (8 * (int)Func.Data)], rax);
        }

        public void EmitCeq()
        {
            c.cmp(GetArgument(0), GetArgument(1));

            int reg = GetRegArgument(0);

            c.sete(_8[reg]);

            c.and(GetArgument(0), 1);
        }

        public void EmitCgt()
        {
            c.cmp(GetArgument(0), GetArgument(1));

            int reg = GetRegArgument(0);

            c.setg(_8[reg]);

            c.and(GetArgument(0), 1);
        }

        public void EmitCgt_Un()
        {
            c.cmp(GetArgument(0), GetArgument(1));

            int reg = GetRegArgument(0);

            c.seta(_8[reg]);

            c.and(GetArgument(0), 1);
        }

        public void EmitCgte()
        {
            c.cmp(GetArgument(0), GetArgument(1));

            int reg = GetRegArgument(0);

            c.setge(_8[reg]);

            c.and(GetArgument(0), 1);
        }

        public void EmitCgte_Un()
        {
            c.cmp(GetArgument(0), GetArgument(1));

            int reg = GetRegArgument(0);

            c.setae(_8[reg]);

            c.and(GetArgument(0), 1);
        }

        public void EmitClt()
        {
            c.cmp(GetArgument(0), GetArgument(1));

            int reg = GetRegArgument(0);

            c.setl(_8[reg]);

            c.and(GetArgument(0), 1);
        }

        public void EmitClt_Un()
        {
            c.cmp(GetArgument(0), GetArgument(1));

            int reg = GetRegArgument(0);

            c.setb(_8[reg]);

            c.and(GetArgument(0), 1);
        }

        public void EmitClte()
        {
            c.cmp(GetArgument(0), GetArgument(1));

            int reg = GetRegArgument(0);

            c.setle(_8[reg]);

            c.and(GetArgument(0), 1);
        }

        public void EmitClte_Un()
        {
            c.cmp(GetArgument(0), GetArgument(1));

            int reg = GetRegArgument(0);

            c.setbe(_8[reg]);

            c.and(GetArgument(0), 1);
        }

        public void EmitDivide()
        {
            UnloadAllRegisters();

            Operand des = CurrentOperation.Operands[0];
            Operand src = CurrentOperation.Operands[1];

            c.mov(rax, __[r15 + ((int)des.Data * 8)]);

            if (src.Type == OperandType.Immediate)
            {
                c.mov(rcx, src.Data);
            }
            else
            {
                c.mov(rcx, __[r15 + ((int)src.Data * 8)]);
            }

            if (CurrentOperation.Size == IntSize.Int32)
            {
                c.cdq();
            }
            else
            {
                c.cqo();
            }

            if (CurrentOperation.Size == IntSize.Int32)
            {
                c.idiv(ecx);
            }
            else
            {
                c.idiv(rcx);
            }

            c.mov(__[r15 + ((int)des.Data * 8)], rax);
        }

        public void EmitDivide_Un()
        {
            //throw new NotImplementedException();

            UnloadAllRegisters();

            Operand des = CurrentOperation.Operands[0];
            Operand src = CurrentOperation.Operands[1];

            c.mov(rdx, 0);

            c.mov(rax, __[r15 + ((int)des.Data * 8)]);

            if (src.Type == OperandType.Immediate)
            {
                c.mov(rcx, src.Data);
            }
            else
            {
                c.mov(rcx, __[r15 + ((int)src.Data * 8)]);
            }

            if (CurrentOperation.Size == IntSize.Int32)
            {
                c.div(ecx);
            }
            else
            {
                c.div(rcx);
            }

            c.mov(__[r15 + ((int)des.Data * 8)], rax);
        }

        void AddCompQue(int i)
        {
            if (!Compiled.Contains(i))
            {
                ToBeCompiled.Add(i);
            }
        }

        public void EmitJump()
        {
            UnloadAllRegisters();

            int Des = (int)CurrentOperation.Operands[0].Data;

            if (!cfg.Contains(Des))
                return;

            c.jmp(Labels[Des]);
        }

        public void EmitJumpIf()
        {
            UnloadAllRegisters();

            int Des = (int)CurrentOperation.Operands[1].Data;
            int next = CurrentOperation.Address + 1;

            int test = (int)CurrentOperation.Operands[0].Data;

            c.mov(rax, __[r15 + (test * 8)]);

            c.cmp(rax, 1);

            c.je(Labels[Des]);
            c.jmp(Labels[next]);
        }

        public void EmitLoad64() => c.mov(_64[GetRegArgument(0)], __[GetArgument(0, RequestType.Read)]);

        public void EmitLoad16()
        {
            c.mov(_16[GetRegArgument(0)], __[GetArgument(0, RequestType.Read)]);

            c.and(_64[GetRegArgument(0)], ushort.MaxValue);
        }
        public void EmitLoad32() => c.mov(_32[GetRegArgument(0)], __[GetArgument(0, RequestType.Read)]);

        public void EmitLoad8()
        {
            c.mov(_8[GetRegArgument(0)], __[GetArgument(0, RequestType.Read)]);

            c.and(_64[GetRegArgument(0)], byte.MaxValue);
        }

        public void EmitLoadContext()
        {
            throw new NotImplementedException();
        }

        public void EmitMove() => c.mov(GetArgument(0), GetArgument(1, RequestType.Read, true, true));

        public void EmitMultiply()
        {
            UnloadAllRegisters();

            Operand des = CurrentOperation.Operands[0];
            Operand src = CurrentOperation.Operands[1];

            c.mov(rdx, 0);

            c.mov(rcx, __[r15 + ((int)des.Data * 8)]);

            if (src.Type == OperandType.Immediate)
            {
                c.mov(rax, src.Data);
            }
            else
            {
                c.mov(rax, __[r15 + ((int)src.Data * 8)]);
            }

            if (CurrentOperation.Size == IntSize.Int32)
                c.imul(ecx);
            else
                c.imul(rcx);

            c.mov(__[r15 + ((int)des.Data * 8)], rax);
        }

        public void EmitNot() => c.not(GetArgument(0));

        public void EmitOr() => c.or(GetArgument(0), GetArgument(1));

        public void EmitReturn()
        {
            EndAll();

            c.mov(rax, GetArgument(0));
            c.ret();
        }

        public void EmitShiftLeft()
        {
            dynamic des = GetArgument(0);

            Operand src = CurrentOperation.Operands[1];

            if (src.Type == OperandType.Immediate)
            {
                c.shl(des, (byte)src.Data);
            }
            else
            {
                int s = GetRegArgument(1);

                c.mov(rcx, _64[s]);

                c.shl(GetArgument(0), cl);
            }
        }

        public void EmitShiftRight()
        {
            dynamic des = GetArgument(0);

            Operand src = CurrentOperation.Operands[1];

            if (src.Type == OperandType.Immediate)
            {
                c.shr(des, (byte)src.Data);
            }
            else
            {
                int s = GetRegArgument(1);

                c.mov(rcx, _64[s]);

                c.shr(GetArgument(0), cl);
            }
        }

        public void EmitShiftRight_Singed()
        {
            dynamic des = GetArgument(0);

            Operand src = CurrentOperation.Operands[1];

            if (src.Type == OperandType.Immediate)
            {
                c.sar(des, (byte)src.Data);
            }
            else
            {
                int s = GetRegArgument(1);

                c.mov(rcx, _64[s]);

                c.sar(GetArgument(0), cl);
            }
        }

        public void EmitSignExtend16()
        {
            int des = GetRegArgument(0);

            c.movsx(_64[des], _16[des]);
        }

        public void EmitSignExtend32()
        {
            int des = GetRegArgument(0);

            c.movsxd(_64[des], _32[des]);
        }

        public void EmitSignExtend8()
        {
            int des = GetRegArgument(0);

            c.movsx(_64[des], _8[des]);
        }

        public void EmitStore16() => c.mov(__[GetArgument(0, RequestType.Read)], _16[GetRegArgument(1)]);
        public void EmitStore32() => c.mov(__[GetArgument(0, RequestType.Read)], _32[GetRegArgument(1)]);
        public void EmitStore64() => c.mov(__[GetArgument(0, RequestType.Read)], _64[GetRegArgument(1)]);
        public void EmitStore8() => c.mov(__[GetArgument(0, RequestType.Read)], _8[GetRegArgument(1)]);

        public void EmitSubtract() => c.sub(GetArgument(0), GetArgument(1));
        public void EmitXor() => c.xor(GetArgument(0), GetArgument(1));

        public void EmitNop() => c.nop();

        public void EmitVector_ClearVector()
        {
            c.mov(rcx, 0);

            c.movq(GetArgument(0, RequestType.Write), rcx);
        }

        public void EmitVector_SetVectorElement()
        {
            Operand index = CurrentOperation.Operands[2];
            Operand size = CurrentOperation.Operands[3];

            //Console.WriteLine(CurrentOperation);

            switch (size.Data)
            {
                case 0: c.pinsrb(GetArgument(0, RequestType.Read), GetArgument(1, RequestType.All, false), (byte)index.Data); break;
                case 1: c.pinsrw(GetArgument(0, RequestType.Read), GetArgument(1, RequestType.All, false), (byte)index.Data); break;
                case 2: c.pinsrd(GetArgument(0, RequestType.Read), GetArgument(1, RequestType.All, false), (byte)index.Data); break;
                case 3: c.pinsrq(GetArgument(0, RequestType.Read), GetArgument(1, RequestType.All, false), (byte)index.Data); break;
                default: throw new NotImplementedException();
            }
        }

        public void EmitVector_Extract()
        {
            Operand index = CurrentOperation.Operands[2];
            Operand size = CurrentOperation.Operands[3];

            switch (size.Data)
            {
                case 0: c.pextrb(GetArgument(0, RequestType.Write), GetArgument(1, RequestType.All, false), (byte)index.Data); break;
                case 1: c.pextrw(GetArgument(0, RequestType.Write), GetArgument(1, RequestType.All, false), (byte)index.Data); break;
                case 2: c.pextrd(GetArgument(0, RequestType.Write), GetArgument(1, RequestType.All, false), (byte)index.Data); break;
                case 3: c.pextrq(GetArgument(0, RequestType.Write), GetArgument(1, RequestType.All, false), (byte)index.Data); break;
                default: throw new NotImplementedException();
            }
        }

        public void EmitVector_Move()
        {
            c.movaps(GetArgument(0, RequestType.Read), GetArgument(1, RequestType.Read));
        }

        public void EmitVector_Load()
        {
            int size = (int)CurrentOperation.Operands[2].Data;

            switch (size)
            {
                case 4: c.vmovupd(GetArgument(0, RequestType.Write), __[GetArgument(1)]); break;
                default: throw new NotImplementedException();
            }
        }

        public void EmitVector_Store()
        {
            int size = (int)CurrentOperation.Operands[2].Data;

            switch (size)
            {
                case 4: c.vmovupd(__[GetArgument(0, RequestType.Read)], GetArgument(1)); break;
                default: throw new NotImplementedException();
            }
        }

        public void EmitVector_And() => c.vandps(GetArgument(0, RequestType.Write), GetArgument(0, RequestType.Read), GetArgument(1));
        public void EmitVector_Orr() => c.vorps(GetArgument(0, RequestType.Write), GetArgument(0, RequestType.Read), GetArgument(1));
        public void EmitVector_Xor() => c.vxorps(GetArgument(0, RequestType.Write), GetArgument(0, RequestType.Read), GetArgument(1));

        public void EmitVector_ConvertToFloat()
        {
            int to = (int)CurrentOperation.Operands[3].Data;
            bool singed = CurrentOperation.Operands[4].Data == 1 ? true : false;

            if (singed)
            {
                if (to == 2)
                {
                    //int -> float

                    c.cvtsi2ss(GetArgument(0, RequestType.Write), GetArgument(1));
                }
                else if (to == 3)
                {
                    //int -> double

                    c.cvtsi2sd(GetArgument(0, RequestType.Write), GetArgument(1));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void EmitVector_ConvertToInt()
        {
            int from = (int)CurrentOperation.Operands[2].Data;

            if (from == 2)
            {
                //float -> int

                c.cvttss2si(GetArgument(0, RequestType.Write), GetArgument(1));
            }
            else if (from == 3)
            {
                //double -> int

                c.cvttsd2si(GetArgument(0, RequestType.Write), GetArgument(1));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void EmitVector_ScalarOperation()
        {
            IntermediateRepresentation.Instruction instruction = (IntermediateRepresentation.Instruction)CurrentOperation.Operands[0].Data;

            int size = (int)CurrentOperation.Operands[3].Data;

            if (size == 3)
            {
                switch (instruction)
                {
                    case IntermediateRepresentation.Instruction.Vector_Fmul: c.vmulsd(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fdiv: c.vdivsd(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fadd: c.vaddsd(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fsub: c.vsubsd(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    default: throw new NotImplementedException();
                }
            }
            else if (size == 2)
            {
                switch (instruction)
                {
                    case IntermediateRepresentation.Instruction.Vector_Fmul: c.vmulss(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fdiv: c.vdivss(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fadd: c.vaddss(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fsub: c.vsubss(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    default: throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void EmitVector_FloatVectorOperation()
        {
            IntermediateRepresentation.Instruction instruction = (IntermediateRepresentation.Instruction)CurrentOperation.Operands[0].Data;

            int size = (int)CurrentOperation.Operands[3].Data;

            if (size == 3)
            {
                switch (instruction)
                {
                    case IntermediateRepresentation.Instruction.Vector_Fmul: c.vmulpd(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fadd: c.vaddpd(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fsub: c.vsubpd(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fdiv: c.vdivpd(GetArgument(1), GetArgument(1), GetArgument(2)); break;

                    case IntermediateRepresentation.Instruction.Vector_Fceq: c.cmppd(GetArgument(1), GetArgument(2), 0); break;
                    case IntermediateRepresentation.Instruction.Vector_Fcge: c.cmppd(GetArgument(1), GetArgument(2), 5); break;
                    case IntermediateRepresentation.Instruction.Vector_Fcgt: c.cmppd(GetArgument(1), GetArgument(2), 6); break;
                    default: throw new NotImplementedException();
                }
            }
            else
            {
                switch (instruction)
                {
                    case IntermediateRepresentation.Instruction.Vector_Fmul: c.vmulps(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fadd: c.vaddps(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fsub: c.vsubps(GetArgument(1), GetArgument(1), GetArgument(2)); break;
                    case IntermediateRepresentation.Instruction.Vector_Fdiv: c.vdivps(GetArgument(1), GetArgument(1), GetArgument(2)); break;

                    case IntermediateRepresentation.Instruction.Vector_Frsqrt: c.rsqrtps(GetArgument(1), GetArgument(1)); break;

                    case IntermediateRepresentation.Instruction.Vector_Fceq: c.cmpps(GetArgument(1), GetArgument(2), 0); break;
                    case IntermediateRepresentation.Instruction.Vector_Fcge: c.cmpps(GetArgument(1), GetArgument(2), 5); break;
                    case IntermediateRepresentation.Instruction.Vector_Fcgt: c.cmpps(GetArgument(1), GetArgument(2), 6); break;
                    default: throw new NotImplementedException();
                }
            }
        }

        public void EmitHardPC()
        {
            EndAll();

            c.mov(rdx, GetArgument(0));
            c.mov(rcx, r15);

            c.jmp(rdx);
        }

        #endregion
    }
}
