using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.PVCALCULATOR
{
    public class PVType111 : PVType1
    {
        //ABL 납후유지비 상품. 납입면제자에 대한 납후유지비 .
        public PVType111(LineInfo line) : base(line)
        {

        }

        public override double Get영업보험료(int n, int m, int t, int freq)
        {
            double payCnt = Get연납입횟수(freq);
            double NNx_유지자0 = GetNNx(c.Nx_유지자, c.Dx_유지자, freq, 0, n);
            double NNx_납입자0 = GetNNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double APV = Get연금현가(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPBeta = Get순보험료(n, m, t, freq) + ex.Betaprime_S / payCnt * (NNx_유지자0 - NNx_납입자0) / NNx_납입자0;
            double NPSTD = Get기준연납순보험료(n, m, t, 12);

            double 분자 = NPBeta + (ex.Alpha_S + NPSTD * ex.Alpha_Statutory) / (payCnt * APV) + (ex.Beta_S / payCnt);
            double 분모 = (1.0 - ex.Alpha_P / APV - ex.Alpha2_P - ex.Beta_P - ex.Gamma - ex.Ce);

            return 분자 / 분모;
        }

        public override double Get준비금(int n, int m, int t, int freq)
        {
            double payCnt = Get연납입횟수(freq);
            double NNx_유지자 = GetNNx(c.Nx_유지자, c.Dx_유지자, freq, t, n);
            double NNx_납입자 = GetNNx(c.Nx_납입자, c.Dx_납입자, freq, t, m);
            double NNx_유지자0 = GetNNx(c.Nx_유지자, c.Dx_유지자, freq, 0, n);
            double NNx_납입자0 = GetNNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPBeta = Get순보험료(n, m, t, freq) + ex.Betaprime_S * (NNx_유지자0 - NNx_납입자0) / NNx_납입자0;

            double 분자Out = (c.Mx_급부[t] - c.Mx_급부[n]) + ex.Betaprime_S * (NNx_유지자 - NNx_납입자);
            double 분자In = (m > 0 && t <= m) ? NPBeta * payCnt * NNx_납입자 : 0;

            double 분자 = 분자Out - 분자In;
            double 분모 = c.Dx_유지자[t];
           
            return 분자 / 분모;
        }

        public override double GetBeta순보험료(int n, int m, int t, int freq)
        {
            double payCnt = Get연납입횟수(freq);
            double NNx_유지자0 = GetNNx(c.Nx_유지자, c.Dx_유지자, freq, 0, n);
            double NNx_납입자0 = GetNNx(c.Nx_납입자, c.Dx_납입자, freq, 0, m);
            double NPBeta = Get순보험료(n, m, t, freq) + ex.Betaprime_S / payCnt * (NNx_유지자0 - NNx_납입자0) / NNx_납입자0;

            return NPBeta;
        }
    }
}
