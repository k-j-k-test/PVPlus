using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVPlus.UI.TextModifier
{
    public partial class HanwhaTextModifier : Form
    {
        public HanwhaTextModifier()
        {
            InitializeComponent();
        }

        private void HanwhaTextModifier_Load(object sender, EventArgs e)
        {
            JongtextBox.Text = "1 -> (Jong = 1 OR Jong = 2) AND S5 > 1\r\n2 -> (Jong = 1 OR Jong = 2) AND S5 = 0\r\n3 -> (Jong = 3 OR Jong = 4) AND S5 > 1\r\n4 -> (Jong = 3 OR Jong = 4) AND S5 = 0\r\n5 -> (Jong = 5 OR Jong = 6) AND S5 > 1\r\n6 -> (Jong = 5 OR Jong = 6) AND S5 = 0\r\n7 -> (Jong = 7 OR Jong = 8) AND S5 > 1\r\n8 -> (Jong = 7 OR Jong = 8) AND S5 = 0\r\n";
            prdCodeTextBox.Text = "LA";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PVPLUSExepnseTextBox.Clear();
            Task.Run(() => GenerateExepense());
        }

        private void GenerateExepense()
        {
            try
            {            
                string text = HanwhaExpenseTextBox.Text;

                List<string[]> list = text
                    .Replace("*", "0")
                    .Replace("\r", "")
                    .Split('\n')
                    .Select(x => x.Split('\t').ToArray())
                    .Where(x => x.Length >= 28 && !x[0].Any(c => c >= '\uAC00' && c <= '\uD7A3'))
                    .ToList();

                int TotalLineNums = list.Count;

                List<HanwhaExpense> HanwhaExpenses = list
                    .Select(x => ToHanwhaExpense(x))
                    .ToList();

                Dictionary<int, string> JongConditionMatch = new Dictionary<int, string>();

                foreach (string s in JongtextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.None))
                {
                    string[] arr = s.Split(new[] { "->" }, StringSplitOptions.None);
                    if (arr.Length == 2)
                    {
                        int jong = int.Parse(arr[0]);
                        string condition = arr[1];
                        JongConditionMatch[jong] = condition;
                    }
                }

                ILookup<string, HanwhaExpense> HanwhaExpenseLookup = HanwhaExpenses.ToLookup(x => x.담보코드);
                StringBuilder res = new StringBuilder();
                int currenLineNum = 0;

                foreach (var s in HanwhaExpenseLookup)
                {
                    List<HanwhaExpense> exList = s
                        .OrderBy(x => x.종코드)
                        .ThenBy(x => x.가변키1)
                        .ThenBy(x => x.가변키2)
                        .ThenBy(x => x.가변키3)
                        .ThenBy(x => x.가변키4)
                        .ThenBy(x => x.가변키5)
                        .ThenByDescending(x => x.최초갱신구분코드)
                        .ThenBy(x => x.보험기간)
                        .ThenBy(x => x.납입기간)
                        .ToList();

                    foreach (var t in exList)
                    {
                        PVPlusExpense ex = new PVPlusExpense();
                        List<string> condition1 = new List<string>();
                        List<string> condition2 = new List<string>();
                        List<string> condition3 = new List<string>();
                        List<string> condition4 = new List<string>();

                        if (t.종코드 > 0)
                        {
                            if (checkBox1.Checked && JongConditionMatch.ContainsKey(t.종코드) && t.만기구분코드 == "세") condition1.Add(JongConditionMatch[t.종코드]);
                            else condition1.Add($"Jong = {t.종코드}");
                        }
                        if (t.최초갱신구분코드 == "최초") condition2.Add($"S1=0");
                        if (t.최초갱신구분코드 == "갱신") condition2.Add($"S1>0");

                        if (t.보험기간 > 0)
                        {
                            if (t.만기구분코드 == "세") condition3.Add($"Age+n={t.보험기간}");
                            if (t.만기구분코드 == "연") condition3.Add($"n={t.보험기간}");
                        }
                        if (t.납입기간 > 0) condition3.Add($"m={t.납입기간}");

                        if (t.가변키1.Contains(";")) condition4.Add($"F6={t.가변키1.Split(';')[1]}");
                        if (t.가변키2.Contains(";")) condition4.Add($"F7={t.가변키2.Split(';')[1]}");
                        if (t.가변키3.Contains(";")) condition4.Add($"F8={t.가변키3.Split(';')[1]}");
                        if (t.가변키4.Contains(";")) condition4.Add($"F9={t.가변키4.Split(';')[1]}");
                        if (t.가변키5.Contains(";")) condition4.Add($"F10={t.가변키5.Split(';')[1]}");

                        ex.상품코드 = prdCodeTextBox.Text;
                        ex.담보코드 = s.Key;
                        ex.조건1 = string.Join(" AND ", condition1);
                        ex.조건2 = string.Join(" AND ", condition2);
                        ex.조건3 = string.Join(" AND ", condition3);
                        ex.조건4 = string.Join(" AND ", condition4);

                        ex.Alpha_S = (t.신계약비1 / 1000).ToString();
                        ex.Alpha_P = (t.신계약비2 / 100).ToString();
                        ex.Beta_P = (t.유지비1 / 100).ToString();
                        ex.Beta_S = (t.유지비2 / 1000).ToString();
                        ex.Betaprime_P = ((t.유지비3 + t.손해조사비2) / 100).ToString();
                        ex.Betaprime_S = (t.유지비4 / 1000).ToString();
                        ex.Gamma = (t.감마 / 100).ToString();
                        ex.Ce = (t.손해조사비1 / 100).ToString();

                        res.Append(AutoMapper.ClassToString(ex) + "\r\n");

                        currenLineNum++;

                        Invoke(new Action(() => 
                        {
                            if (currenLineNum % 1000 == 0) labelProgress.Text = $"진행중 : {currenLineNum}/{TotalLineNums}";
                        }));

                    }
                }
                
                Invoke(new Action(async () =>
                {
                    labelProgress.Text = $"결과 복사 중";
                    await Task.Delay(50);
                    PVPLUSExepnseTextBox.AppendText(res.ToString());
                    labelProgress.Text = $"완료 : {TotalLineNums}/{TotalLineNums}";
                }));
                
            }
            catch (Exception ex)
            {
                errorTextBox.Text = ex.Message;
            }
        }
    
        HanwhaExpense ToHanwhaExpense(string[] arr)
        {
            HanwhaExpense exp = new HanwhaExpense();

            exp.종코드 = int.Parse(arr[0]);
            exp.형코드 = arr[1];
            exp.담보코드 = arr[2];
            exp.만기구분코드 = arr[3];
            exp.최초갱신구분코드 = arr[4];
            exp.보험기간 = int.Parse(arr[5]);
            exp.납입기간 = int.Parse(arr[6]);
            exp.성별 = arr[7];
            exp.가변키1 = arr[8];
            exp.가변키2 = arr[9];
            exp.가변키3 = arr[10];
            exp.가변키4 = arr[11];
            exp.가변키5 = arr[12];
            exp.신계약비1 = double.Parse(arr[13]);
            exp.신계약비2 = double.Parse(arr[14]);
            exp.신계약비3 = double.Parse(arr[15]);
            exp.신계약비4 = double.Parse(arr[16]);
            exp.신계약비5 = double.Parse(arr[17]);
            exp.유지비1 = double.Parse(arr[18]);
            exp.유지비2 = double.Parse(arr[19]);
            exp.유지비3 = double.Parse(arr[20]);
            exp.유지비4 = double.Parse(arr[21]);
            exp.유지비5 = double.Parse(arr[22]);
            exp.감마 = double.Parse(arr[23]);
            exp.손해조사비1 = double.Parse(arr[24]);
            exp.손해조사비2 = double.Parse(arr[25]);

            return exp;
        }

        private void JongtextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }

    public class HanwhaExpense
    {
        public int 종코드 { get; set; }
        public string 형코드 { get; set; }
        public string 담보코드 { get; set; }
        public string 만기구분코드 { get; set; }
        public string 최초갱신구분코드 { get; set; }
        public int 보험기간 { get; set; }
        public int 납입기간 { get; set; }
        public string 성별 { get; set; }
        public string 가변키1 { get; set; }
        public string 가변키2 { get; set; }
        public string 가변키3 { get; set; }
        public string 가변키4 { get; set; }
        public string 가변키5 { get; set; }
        public double 신계약비1 { get; set; }
        public double 신계약비2 { get; set; }
        public double 신계약비3 { get; set; }
        public double 신계약비4 { get; set; }
        public double 신계약비5 { get; set; }
        public double 유지비1 { get; set; }
        public double 유지비2 { get; set; }
        public double 유지비3 { get; set; }
        public double 유지비4 { get; set; }
        public double 유지비5 { get; set; }
        public double 감마 { get; set; }
        public double 손해조사비1 { get; set; }
        public double 손해조사비2 { get; set; }
    }

    public class PVPlusExpense
    {
        public string 상품코드 { get; set; }
        public string 담보코드 { get; set; }
        public string 조건1 { get; set; }
        public string 조건2 { get; set; }
        public string 조건3 { get; set; }
        public string 조건4 { get; set; }

        public string Alpha_P { get; set; }
        public string Alpha2_P { get; set; }
        public string Alpha_S { get; set; }
        public string Alpha_Statutory { get; set; }
        public string Beta_P { get; set; }
        public string Beta_S { get; set; }
        public string Betaprime_P { get; set; }
        public string Betaprime_S { get; set; }
        public string Gamma { get; set; }
        public string Ce { get; set; }

        public string Refund_Rate_S { get; set; }
        public string Refund_Rate_P { get; set; }

        public string 가변1 { get; set; }
        public string 가변2 { get; set; }
    }
}
