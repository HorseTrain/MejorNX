using GalacticARM.CodeGen.Translation;
using GalacticARM.CodeGen.Translation.aarch64;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Decoding
{
    public class InstructionInfo
    {
        public bool IsSP { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public int Mask { get; set; }

        public InstructionInfo(string Name, bool IsSP, int Size)
        {
            if (IsSP)
            {
                Name = Name.Replace("|sp", "");
            }

            this.Name = Name;
            this.IsSP = IsSP;

            Mask = (1 << Size) - 1;
        }
    }

    class OpCodeTable
    {
        int mask;
        int instruction;
        public Emit emit;
        public List<InstructionInfo> Info;

        static List<OpCodeTable> Tables;

        public bool Compare(int Source) => (Source & mask) == instruction;
        public static bool IsBranch(int RawOpCode) => ((RawOpCode >> 26) & 0b111) == 0b101;

        static (int, int, List<InstructionInfo> Info) GetEncodingData(string Encoding)
        {
            int mask = 0;
            int instruction = 0;
            List<InstructionInfo> Info = new List<InstructionInfo>();

            //Build String
            string[] EncodingData = Encoding.Split(' ');

            EncodingData.Reverse();

            string FinalEncoding = "";

            foreach (string str in EncodingData)
            {
                string[] parts = str.Split('_');

                string Name = parts[0];

                if (Name == "enc")
                {
                    FinalEncoding = FinalEncoding + parts[1];
                }
                else
                {
                    int Size = int.Parse(parts[1]);

                    InstructionInfo info = new InstructionInfo(Name, str.Contains("|sp"), Size);

                    for (int i = 0; i < Size; i++)
                    {
                        FinalEncoding = FinalEncoding + "-";
                    }

                    info.Index = 32 - FinalEncoding.Length;

                    Info.Add(info);
                }
            }

            //Console.WriteLine(FinalEncoding);

            if (FinalEncoding.Length != 32)
            {
                Console.WriteLine($"Invalid Encoding {FinalEncoding}");

                throw new Exception();
            }

            for (int i = 0; i < 32; i++)
            {
                int rp = 31 - i;

                if (FinalEncoding[rp] != '-')
                {
                    mask |= 1 << i;
                }

                if (FinalEncoding[rp] == '1')
                {
                    instruction |= 1 << i;
                }
            }

            return (mask, instruction, Info);
        }

        static void Add(Emit emit, string Encoding)
        {
            (int mask, int instruction, List<InstructionInfo> Info) = GetEncodingData(Encoding);

            OpCodeTable Out = new OpCodeTable();

            Out.instruction = instruction;
            Out.mask = mask;
            Out.Info = Info;
            Out.emit = emit;

            Tables.Add(Out);
        }

        static OpCodeTable()
        {
            Tables = new List<OpCodeTable>();

            #region Aarch64
            Add(Emit64.Nop, "enc_1111100110 imm_12 rn_5 rt_5");

            Add(Emit64.AddsSubs_ExtendedReg, "sf_1 op_1 enc_1 enc_01011001 rm_5 option_3 shift_3 rn|sp_5 rd_5");
            Add(Emit64.AddsSubs_Imm, "sf_1 op_1 enc_1 enc_10001 shift_2 imm_12 rn|sp_5 rd_5");
            Add(Emit64.AddSub_ExtendedReg, "sf_1 op_1 enc_0 enc_01011001 rm_5 option_3 shift_3 rn|sp_5 rd|sp_5");
            Add(Emit64.AddSub_Imm, "sf_1 op_1 enc_0 enc_10001 shift_2 imm_12 rn|sp_5 rd|sp_5");
            Add(Emit64.AddSub_ShiftedReg_s, "sf_1 op_1 s_1 enc_01011 shift_2 enc_0 rm_5 imm_6 rn_5 rd_5");
            Add(Emit64.Adr, "enc_0 immlo_2 enc_10000 immhi_19 rd_5");
            Add(Emit64.Adrp, "enc_1 immlo_2 enc_10000 immhi_19 rd_5");
            Add(Emit64.And_Imm, "sf_1 enc_00 enc_100100 n_1 immr_6 imms_6 rn_5 rd|sp_5");
            Add(Emit64.And_ShiftedReg, "sf_1 enc_00 enc_01010 shift_2 n_1 rm_5 imm_6 rn_5 rd_5");
            Add(Emit64.Ands_Imm, "sf_1 enc_11 enc_100100 n_1 immr_6 imms_6 rn_5 rd_5");
            Add(Emit64.Ands_ShiftedReg, "sf_1 enc_11 enc_01010 shift_2 n_1 rm_5 imm_6 rn_5 rd_5");
            Add(Emit64.ASRV, "sf_1 enc_0011010110 rm_5 enc_001010 rn_5 rd_5");
            Add(Emit64.B, "enc_000101 imm_26");
            Add(Emit64.B_Cond, "enc_01010100 imm_19 enc_0 cond_4");
            Add(Emit64.Bfm, "sf_1 enc_01100110 n_1 immr_6 imms_6 rn_5 rd_5");
            Add(Emit64.BL, "enc_100101 imm_26");
            Add(Emit64.Blr, "enc_1101011000111111000000 rn_5 enc_00000");
            Add(Emit64.BrRet, "enc_1101011000011111000000 rn_5 enc_00000");
            Add(Emit64.BrRet, "enc_1101011001011111000000 rn_5 enc_00000");
            Add(Emit64.Cbnz, "sf_1 enc_0110101 imm_19 rt_5");
            Add(Emit64.Cbz, "sf_1 enc_0110100 imm_19 rt_5");
            Add(Emit64.Ccmm_Imm, "sf_1 enc_0111010010 imm_5 cond_4 enc_10 rn_5 enc_0 nzcv_4");
            Add(Emit64.Ccmn_Reg, "sf_1 enc_0111010010 rm_5 cond_4 enc_00 rn_5 enc_0 nzcv_4");
            Add(Emit64.Ccmp_Imm, "sf_1 enc_1111010010 imm_5 cond_4 enc_10 rn_5 enc_0 nzcv_4");
            Add(Emit64.Ccmp_Reg, "sf_1 enc_1111010010 rm_5 cond_4 enc_00 rn_5 enc_0 nzcv_4");
            Add(Emit64.Clrex, "enc_11010101000000110011 crm_4 enc_01011111");
            Add(Emit64.Clz, "sf_1 enc_101101011000000000100 rn_5 rd_5");
            Add(Emit64.Csel, "sf_1 enc_0011010100 rm_5 cond_4 enc_00 rn_5 rd_5");
            Add(Emit64.Csinc, "sf_1 enc_0011010100 rm_5 cond_4 enc_01 rn_5 rd_5");
            Add(Emit64.Csinv, "sf_1 enc_1011010100 rm_5 cond_4 enc_00 rn_5 rd_5");
            Add(Emit64.Csneg, "sf_1 enc_1011010100 rm_5 cond_4 enc_01 rn_5 rd_5");
            Add(Emit64.Eor_Imm, "sf_1 enc_10 enc_100100 n_1 immr_6 imms_6 rn_5 rd|sp_5");
            Add(Emit64.Eor_ShiftedReg, "sf_1 enc_10 enc_01010 shift_2 n_1 rm_5 imm_6 rn_5 rd_5");
            Add(Emit64.Extr, "sf_1 enc_00100111 n_1 enc_0 rm_5 imms_6 rn_5 rd_5");
            Add(Emit64.Ldar, "size_2 enc_001000110 enc_11111111111 rn|sp_5 rt_5");
            Add(Emit64.Ldaxr, "size_2 enc_00100001011111111111 rn|sp_5 rt_5");
            Add(Emit64.Ldp_ImmIndexed, "enc_-0 enc_10100 type_2 enc_1 imm_7 rt2_5 rn|sp_5 rt_5");
            Add(Emit64.Ldpsw_ImmIndexed, "enc_0110100 type_2 enc_1 imm_7 rt2_5 rn|sp_5 rt_5");
            Add(Emit64.Ldr_ImmIndexed, "size_2 enc_111000010 imm_9 type_2 rn|sp_5 rt_5");
            Add(Emit64.Ldr_Register, "size_2 enc_111000011 rm_5 option_3 s_1 enc_10 rn|sp_5 rt_5");
            Add(Emit64.Ldr_Unscaled, "size_2 enc_11100101 imm_12 rn|sp_5 rt_5");
            Add(Emit64.Ldrs_ImmIndexed, "size_2 enc_1110001 to_1 enc_0 imm_9 type_2 rn|sp_5 rt_5");
            Add(Emit64.Ldrs_Register, "size_2 enc_1110001 to_1 enc_1 rm_5 option_3 s_1 enc_10 rn|sp_5 rt_5");
            Add(Emit64.Ldrs_Unscaled, "size_2 enc_1110011 to_1 imm_12 rn|sp_5 rt_5");
            Add(Emit64.Ldxr, "size_2 enc_00100001011111011111 rn|sp_5 rt_5");
            Add(Emit64.LSLV, "sf_1 enc_0011010110 rm_5 enc_001000 rn_5 rd_5");
            Add(Emit64.LSRV, "sf_1 enc_0011010110 rm_5 enc_001001 rn_5 rd_5");
            Add(Emit64.Madd, "sf_1 enc_0011011000 rm_5 enc_0 ra_5 rn_5 rd_5");
            Add(Emit64.Movk, "sf_1 enc_11100101 hw_2 imm_16 rd_5");
            Add(Emit64.Movn, "sf_1 enc_00100101 hw_2 imm_16 rd_5");
            Add(Emit64.Movz, "sf_1 enc_10100101 hw_2 imm_16 rd_5");
            Add(Emit64.Mrs, "enc_110101010011 o0_1 op1_3 crn_4 crm_4 op2_3 rt_5");
            Add(Emit64.Msr, "enc_110101010001 o0_1 op1_3 crn_4 crm_4 op2_3 rt_5");
            Add(Emit64.Msub, "sf_1 enc_0011011000 rm_5 enc_1 ra_5 rn_5 rd_5");
            Add(Emit64.Nop, "enc_11010101000000110010000000011111");
            Add(Emit64.Nop, "enc_11010101000000110011 crn_4 enc_10111111");
            Add(Emit64.Nop, "enc_11010101000000110011----10011111");
            Add(Emit64.Nop, "enc_1101010100001-------------------");
            Add(Emit64.Orr_Imm, "sf_1 enc_01 enc_100100 n_1 immr_6 imms_6 rn_5 rd|sp_5");
            Add(Emit64.Orr_ShiftedReg, "sf_1 enc_01 enc_01010 shift_2 n_1 rm_5 imm_6 rn_5 rd_5");
            Add(Emit64.Rbit, "sf_1 enc_101101011000000000000 rn_5 rd_5");
            Add(Emit64.RORV, "sf_1 enc_0011010110 rm_5 enc_001011 rn_5 rd_5");
            Add(Emit64.Rev, "sf_1 enc_10110101100000000001 opc_1 rn_5 rd_5");
            Add(Emit64.Rev16, "sf_1 enc_101101011000000000001 rn_5 rd_5");
            Add(Emit64.Sbfm, "sf_1 enc_00100110 n_1 immr_6 imms_6 rn_5 rd_5");
            Add(Emit64.Sdiv, "sf_1 enc_0011010110 rm_5 enc_000011 rn_5 rd_5");
            Add(Emit64.Smaddl, "enc_10011011001 rm_5 enc_0 ra_5 rn_5 rd_5");
            Add(Emit64.Smsubl, "enc_10011011001 rm_5 enc_1 ra_5 rn_5 rd_5");
            Add(Emit64.Smulh, "enc_10011011010 rm_5 enc_011111 rn_5 rd_5");
            Add(Emit64.Stlr, "size_2 enc_00100010011111111111 rn|sp_5 rt_5");
            Add(Emit64.Stlxr, "size_2 enc_001000000 rs_5 enc_111111 rn|sp_5 rt_5");
            Add(Emit64.Stp_ImmIndexed, "opc_2 enc_10100 type_2 enc_0 imm_7 rt2_5 rn|sp_5 rt_5");
            Add(Emit64.Str_ImmIndexed, "size_2 enc_111000000 imm_9 type_2 rn|sp_5 rt_5");
            Add(Emit64.Str_Register, "size_2 enc_111000001 rm_5 option_3 s_1 enc_10 rn|sp_5 rt_5");
            Add(Emit64.Str_Unscaled, "size_2 enc_11100100 imm_12 rn|sp_5 rt_5");
            Add(Emit64.Stxr, "size_2 enc_001000000 rs_5 enc_011111 rn|sp_5 rt_5");
            Add(Emit64.Svc, "enc_11010100000 imm_16 enc_00001");
            Add(Emit64.Tbnz, "b5_1 enc_0110111 b40_5 imm_14 rt_5");
            Add(Emit64.Tbz, "b5_1 enc_0110110 b40_5 imm_14 rt_5");
            Add(Emit64.Ubfm, "sf_1 enc_10100110 n_1 immr_6 imms_6 rn_5 rd_5");
            Add(Emit64.Udiv, "sf_1 enc_0011010110 rm_5 enc_000010 rn_5 rd_5");
            Add(Emit64.Umaddl, "enc_10011011101 rm_5 enc_0 ra_5 rn_5 rd_5");
            Add(Emit64.Umsubl, "enc_10011011101 rm_5 enc_1 ra_5 rn_5 rd_5");
            Add(Emit64.Umulh, "enc_10011011110 rm_5 enc_011111 rn_5 rd_5");

            //Vector
            Add(Emit64.And_Vector, "enc_0 q_1 enc_001110001 rm_5 enc_000111 rn_5 rd_5");
            Add(Emit64.Bic_Vector, "enc_0 q_1 enc_001110011 rm_5 enc_000111 rn_5 rd_5");
            Add(Emit64.Eor_Vector, "enc_0 q_1 enc_101110001 rm_5 enc_000111 rn_5 rd_5");
            Add(Emit64.Orn_Vector, "enc_0 q_1 enc_001110111 rm_5 enc_000111 rn_5 rd_5");
            Add(Emit64.Orr_Vector, "enc_0 q_1 enc_001110101 rm_5 enc_000111 rn_5 rd_5");
            
            Add(Emit64.Dup_General, "enc_0 q_1 enc_001110000 imm_5 enc_000011 rn_5 rd_5");
            
            Add(Emit64.Fmov_General, "sf_1 enc_0011110 ftype_2 enc_10 rmode_1 enc_11 opcode_1 enc_000000 rn_5 rd_5");
            Add(Emit64.Fmov_Imm, "enc_00011110 ftype_2 enc_1 imm_8 enc_10000000 rd_5");
            Add(Emit64.Ins_General, "enc_01001110000 imm_5 enc_000111 rn_5 rd_5");
            Add(Emit64.Movi, "enc_0 q_1 op_1 enc_0111100000 a_1 b_1 c_1 cmode_4 enc_01 d_1 e_1 f_1 g_1 h_1 rd_5");
            
            Add(Emit64.vec_Ldp_Imm, "opc_2 enc_10110 type_2 enc_1 imm_7 rt2_5 rn|sp_5 rt_5");
            Add(Emit64.vec_Ldr_Imm, "size_2 enc_111101 opc_1 enc_1 imm_12 rn|sp_5 rt_5");
            Add(Emit64.vec_Ldr_ImmIndexed, "size_2 enc_111100 opc_1 enc_10 imm_9 type_2 rn|sp_5 rt_5");
            Add(Emit64.vec_Ldr_Register, "size_2 enc_111100 opc_1 enc_11 rm_5 option_3 s_1 enc_10 rn|sp_5 rt_5");
            Add(Emit64.vec_Stp_Imm, "opc_2 enc_10110 type_2 enc_0 imm_7 rt2_5 rn|sp_5 rt_5");
            Add(Emit64.vec_Str_Imm, "size_2 enc_111101 opc_1 enc_0 imm_12 rn|sp_5 rt_5");
            Add(Emit64.vec_Str_ImmIndexed, "size_2 enc_111100 opc_1 enc_00 imm_9 type_2 rn|sp_5 rt_5");
            Add(Emit64.vec_Str_Register, "size_2 enc_111100 opc_1 enc_01 rm_5 option_3 s_1 enc_10 rn|sp_5 rt_5");
            Add(Emit64.Ld1r, "enc_0 q_1 enc_001101010000001100 size_2 rn|sp_5 rt_5");
            
            Add(Emit64.Ucvtf_Scalar_Integer, "sf_1 enc_00111100 ftype_1 enc_100011000000 rn_5 rd_5");
            Add(Emit64.Scvtf_Scalar_Integer, "sf_1 enc_00111100 ftype_1 enc_100010000000 rn_5 rd_5");
            
            Add(Emit64.Fcvtzs_Scalar_Fixed, "sf_1 enc_00111100 ftype_1 enc_011000 scale_6 rn_5 rd_5");
            Add(Emit64.Fcvtzu_Scalar_Fixed, "sf_1 enc_00111100 ftype_1 enc_011001 scale_6 rn_5 rd_5");
            
            Add(Emit64.Fcvtzs_Scalar_Integer, "sf_1 enc_00111100 ftype_1 enc_111000000000 rn_5 rd_5");
            Add(Emit64.Fcvtzu_Scalar_Integer, "sf_1 enc_00111100 ftype_1 enc_111001000000 rn_5 rd_5");
            
            Add(Emit64.Ucvtf_Vector_Integer, "enc_011111100 sz_1 enc_100001110110 rn_5 rd_5");
            Add(Emit64.Scvtf_Vector_Integer, "enc_010111100 sz_1 enc_100001110110 rn_5 rd_5");
            
            Add(Emit64.Fcvtps_Scalar, "sf_1 enc_00111100 ftype_1 enc_101000000000 rn_5 rd_5");
            Add(Emit64.Fcvtms_Scalar, "sf_1 enc_00111100 ftype_1 enc_110000000000 rn_5 rd_5");
            
            Add(Emit64.Fcvt, "enc_000111100 ftype_1 enc_100010 opc_1 enc_10000 rn_5 rd_5");
            
            Add(Emit64.Fdiv_Scalar, "enc_00011110 ftype_2 enc_1 rm_5 enc_000110 rn_5 rd_5");
            Add(Emit64.Fadd_Scalar, "enc_00011110 ftype_2 enc_1 rm_5 enc_001010 rn_5 rd_5");
            Add(Emit64.Fsub_Scalar, "enc_00011110 ftype_2 enc_1 rm_5 enc_001110 rn_5 rd_5");
            Add(Emit64.Fmul_Scalar, "enc_00011110 ftype_2 enc_1 rm_5 enc_000010 rn_5 rd_5");
            Add(Emit64.Fnmul_Scalar, "enc_00011110 ftype_2 enc_1 rm_5 enc_100010 rn_5 rd_5");
            
            Add(Emit64.Fneg, "enc_00011110 ftype_2 enc_100001010000 rn_5 rd_5");
            Add(Emit64.Fabs, "enc_00011110 ftype_2 enc_100000110000 rn_5 rd_5");
            Add(Emit64.Fsqrt, "enc_00011110 ftype_2 enc_100001110000 rn_5 rd_5");
            
            Add(Emit64.Fcmp, "enc_00011110 ftype_2 enc_1 rm_5 enc_001000 rn_5 enc_0 opc_1 enc_000");
            Add(Emit64.Fccmp, "enc_00011110 ftype_2 enc_1 rm_5 cond_4 enc_01 rn_5 enc_0 nzcv_4");
            
            Add(Emit64.Cnt, "enc_0 q_1 enc_00111000100000010110 rn_5 rd_5");
            Add(Emit64.Uaddlv, "enc_0 q_1 enc_101110 size_2 enc_110000001110 rn_5 rd_5");
            
            Add(Emit64.Fmax, "enc_00011110 ftype_2 enc_1 rm_5 enc_010010 rn_5 rd_5");
            Add(Emit64.Fmax, "enc_00011110 ftype_2 enc_1 rm_5 enc_011010 rn_5 rd_5"); //FMAXNM 
            Add(Emit64.Fmin, "enc_00011110 ftype_2 enc_1 rm_5 enc_010110 rn_5 rd_5");
            Add(Emit64.Fmin, "enc_00011110 ftype_2 enc_1 rm_5 enc_011110 rn_5 rd_5"); //FMINNM 
            
            Add(Emit64.Fcsel, "enc_00011110 ftype_2 enc_1 rm_5 cond_4 enc_11 rn_5 rd_5");

            Add(Emit64.Sshll, "enc_0 q_1 enc_0011110 immh_4 immb_3 enc_101001 rn_5 rd_5");
            Add(Emit64.Ushll, "enc_0 q_1 enc_1011110 immh_4 immb_3 enc_101001 rn_5 rd_5");
            Add(Emit64.Xtn, "enc_0 q_1 enc_001110 size_2 enc_100001001010 rn_5 rd_5");
            
            Add(Emit64.Fmul_Vector, "enc_0 q_1 enc_1011100 sz_1 enc_1 rm_5 enc_110111 rn_5 rd_5");
            Add(Emit64.Fadd_Vector, "enc_0 q_1 enc_0011100 sz_1 enc_1 rm_5 enc_110101 rn_5 rd_5");
            Add(Emit64.Fsub_Vector, "enc_0 q_1 enc_0011101 sz_1 enc_1 rm_5 enc_110101 rn_5 rd_5");
            Add(Emit64.Fdiv_Vector, "enc_0 q_1 enc_1011100 sz_1 enc_1 rm_5 enc_111111 rn_5 rd_5");

            Add(Emit64.Fcmeq_VectorRegister, "enc_0 q_1 enc_0011100 sz_1 enc_1 rm_5 enc_111001 rn_5 rd_5");
            Add(Emit64.Fcmgt_VectorRegister, "enc_0 q_1 enc_1011101 sz_1 enc_1 rm_5 enc_111001 rn_5 rd_5");

            Add(Emit64.Fcmeq_VectorZero, "enc_0 q_1 enc_0011101 sz_1 enc_100000110110 rn_5 rd_5");
            Add(Emit64.Fcmge_VectorZero, "enc_0 q_1 enc_1011101 sz_1 enc_100000110010 rn_5 rd_5");

            Add(Emit64.Faddp_Vector, "enc_0 q_1 enc_1011100 sz_1 enc_1 rm_5 enc_110101 rn_5 rd_5");
            
            Add(Emit64.Fmul_VectorElement, "enc_010111111 sz_1 l_1 rm_5 enc_1001 h_1 enc_0 rn_5 rd_5");

            Add(Emit64.Fmul_VectorVectorElement, "enc_0 q_1 enc_0011111 sz_1 l_1 rm_5 enc_1001 h_1 enc_0 rn_5 rd_5");
            Add(Emit64.Fmla_VectorVectorElement, "enc_0 q_1 enc_0011111 sz_1 l_1 rm_5 enc_0001 h_1 enc_0 rn_5 rd_5");
            Add(Emit64.Fmls_VectorVectorElement, "enc_0 q_1 enc_0011111 sz_1 l_1 rm_5 enc_0101 h_1 enc_0 rn_5 rd_5");
            
            Add(Emit64.Frsqrte_Vector, "enc_0 q_1 enc_1011101 sz_1 enc_100001110110 rn_5 rd_5");
            
            Add(Emit64.Ins_Element, "enc_01101110000 imm5_5 enc_0 imm4_4 enc_1 rn_5 rd_5");

            Add(Emit64.Fmla_Vector, "enc_0 q_1 enc_0011100 sz_1 enc_1 rm_5 enc_110011 rn_5 rd_5");
            Add(Emit64.Fmls_Vector, "enc_0 q_1 enc_0011101 sz_1 enc_1 rm_5 enc_110011 rn_5 rd_5");

            Add(Emit64.Frsqrts_Vector, "enc_0 q_1 enc_0011101 sz_1 enc_1 rm_5 enc_111111 rn_5 rd_5");

            Add(Emit64.Neg_Vector, "enc_0 q_1 enc_101110 size_2 enc_100000101110 rn_5 rd_5");

            Add(Emit64.Ext_Vector, "enc_0 q_1 enc_101110000 rm_5 enc_0 imm_4 enc_0 rn_5 rd_5");

            Add(Emit64.Dup_ElementScalar, "enc_01011110000 imm_5 enc_000001 rn_5 rd_5");
            Add(Emit64.Dup_ElementVector, "enc_0 q_1 enc_001110000 imm_5 enc_000001 rn_5 rd_5");

            Add(Emit64.Shl, "enc_0 q_1 enc_0011110 immh_4 immb_3 enc_010101 rn_5 rd_5");
            Add(Emit64.Sshr, "enc_0 q_1 enc_0011110 immh_4 immb_3 enc_000001 rn_5 rd_5");

            Add(Emit64.Zip, "enc_0 q_1 enc_001110 size_2 enc_0 rm_5 enc_0 op_1 enc_1110 rn_5 rd_5");

            Add(Emit64.Bsl, "enc_0 q_1 enc_101110011 rm_5 enc_000111 rn_5 rd_5");

            Add(Emit64.Umov_ToGeneral, "enc_0 q_1 enc_001110000 imm_5 enc_001111 rn_5 rd_5");

            Add(Emit64.Frintp_Scalar, "enc_00011110 ftype_2 enc_100100110000 rn_5 rd_5");
            Add(Emit64.Frintm_Scalar, "enc_00011110 ftype_2 enc_100101010000 rn_5 rd_5");

            Add(Emit64.Scvtf_Vector, "enc_0 q_1 enc_0011100 sz_1 enc_100001110110 rn_5 rd_5");
            Add(Emit64.Ucvtf_Vector, "enc_0 q_1 enc_1011100 sz_1 enc_100001110110 rn_5 rd_5");

            #endregion


            Add(EmitUniversal.EmitUnicornFB, "imm_32");
        }

        public static (OpCodeTable, int) GetTable(ulong Address)
        {
            int RawOpcode = VirtualMemoryManager.ReadObject<int>(Address);

            for (int i = 0; i < Tables.Count; i++)
            {
                if (Tables[i].Compare(RawOpcode))
                {
                    return (Tables[i], RawOpcode);
                }
            }

            throw new NotImplementedException(VirtualMemoryManager.GetOpHex(Address));
        }
    }
}
