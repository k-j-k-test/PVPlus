using Flee.PublicTypes;
using PVPlus.PVCALCULATOR;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PVPlus.RULES
{
    public class LineInfo
    {
        public string OriginalLine { get; set; }
        public string AdjustedLine { get; set; }
        public string[] Arr { get; set; }
        public SInfo sInfo { get; set; }

        public string ProductCode { get; set; }
        public string RiderCode { get; set; }

        public Dictionary<string, Layout> layouts { get; set; }
        public VariableCollection variables;

        public LineInfo(string line)
        {
            variables = PV.variables;

            OriginalLine = line;
            AdjustedLine = Configure.CompanyRule.AdjustLine(line);
            if (Configure.SeperationType == "1") Arr = AdjustedLine.Split(Configure.Delimiter);

            ProductCode = Configure.ProductCode;
            RiderCode = GetLayoutItem(PV.finder.FindRiderCodeLayout()).Trim();

            layouts = PV.finder.FindLayout(ProductCode, RiderCode);

            helper.variables = variables;
            helper.lineInfo = this;
            helper.pvCals = new Dictionary<string, PVCalculator>();

            SetVariables();
        }
        public LineInfo(SInfo sInfo)
        {
            variables = PV.variables;
            this.sInfo = sInfo;

            ProductCode = Configure.ProductCode;
            RiderCode = sInfo.담보코드;

            layouts = new Dictionary<string, Layout>();

            helper.variables = variables;
            helper.lineInfo = this;
            helper.pvCals = new Dictionary<string, PVCalculator>();

            SetVariables();
        }

        public PVResult CalculateLine()
        {
            AddLineSummary();

            string CurMP = GetLayoutMP();
            string PreMP = helper.PreMP;

            if (CurMP == PreMP && layouts.Any())
            {
                helper.PreResult.OrgLine = OriginalLine;
                helper.PreResult.ResultType = "";
                return helper.PreResult;
            }

            PVCalculator pvCal = GetPVCalculator();
            PVResult pVSResultFactory = new PVResult(pvCal);

            helper.PreMP = CurMP;
            helper.PreResult = pVSResultFactory;

            return pVSResultFactory;
        }
        public void CalculateSInfo()
        {
            PVCalculator pvCal = GetPVCalculator();
            helper.cal = pvCal;

            sInfo.위험보험료 = sInfo.위험보험료Expr.Evaluate();
            sInfo.정기위험보험료 = sInfo.정기위험보험료Expr.Evaluate();
            sInfo.S = sInfo.SExpr.Evaluate();
        }

        public void SetVariables()
        {
            ProductRule productRule = PV.finder.FindProductRule();
            RiderRule riderRule = PV.finder.FindRiderRule(RiderCode);

            variables["i"] = productRule.예정이율;
            variables["ii"] = productRule.평균공시이율;

            if (AdjustedLine != null)
            {
                SetLayoutVariables();

                if ((int)variables["nAge"] > 0) variables["n"] = (int)variables["nAge"] - (int)variables["Age"];
                if ((int)variables["mAge"] > 0) variables["m"] = (int)variables["mAge"] - (int)variables["Age"];

                ChangeVariables();
            }
            if (sInfo != null)
            {
                variables["Age"] = sInfo.x;
                variables["n"] = sInfo.n;
                variables["m"] = sInfo.m;

                variables["F1"] = sInfo.성별;
                variables["F2"] = sInfo.급수;
                variables["F3"] = sInfo.운전;
                variables["F4"] = sInfo.금액;
                variables["F5"] = sInfo.사고연령;
                variables["F6"] = sInfo.가변1;
                variables["F7"] = sInfo.가변2;
                variables["F8"] = sInfo.가변3;
                variables["F9"] = sInfo.가변4;

                //AddVariables
                if (sInfo.VarAdd != "")
                {
                    foreach (string kv in sInfo.VarAdd.Split(','))
                    {
                        try
                        {
                            string k = kv.Split(new string[] { "->" }, StringSplitOptions.None)[0].Trim();

                            if (variables[k].GetType() == typeof(int))
                            {
                                variables[k] = int.Parse(kv.Split(new string[] { "->" }, StringSplitOptions.None)[1].Trim());
                            }
                            else if (variables[k].GetType() == typeof(double))
                            {
                                variables[k] = double.Parse(kv.Split(new string[] { "->" }, StringSplitOptions.None)[1].Trim());
                            }
                            else if (variables[k].GetType() == typeof(string))
                            {
                                variables[k] = kv.Split(new string[] { "->" }, StringSplitOptions.None)[1].Trim();
                            }
                            else
                            {
                                throw new Exception($"VarAdd {sInfo.VarAdd}의 형식이 잘 못 되었습니다.");
                            }
                        }
                        catch
                        {
                            throw new Exception($"VarAdd {sInfo.VarAdd}의 형식이 잘 못 되었습니다.");
                        }
                    }
                }
            }

            string SKey = riderRule.SKeyExpr.Evaluate();
            double Min_S = PV.finder.FindMin_S(SKey);

            variables["v"] = 1 / (1 + (double)variables["i"]);
            variables["vv"] = 1 / (1 + (double)variables["ii"]);
            variables["Amount"] = riderRule.가입금액Expr.Evaluate();
            variables["Min_S"] = Min_S;
            variables["PV_Type"] = riderRule.PV_Type.Evaluate();
            variables["S_Type"] = riderRule.S_Type.Evaluate();
            variables["Company"] = Configure.CompanyRule.GetType().Name;
            variables["Channel"] = productRule.판매채널;
        }
        public void SetLayoutVariables()
        {

            foreach (var s in layouts)
            {
                try
                {
                    if (variables.ContainsKey(s.Key))
                    {
                        Type variableType = variables[s.Key].GetType();

                        if (variableType == typeof(int))
                        {
                            variables[s.Key] = int.Parse(GetLayoutItem(s.Value));
                        }
                        else if (variableType == typeof(double))
                        {
                            variables[s.Key] = double.Parse(GetLayoutItem(s.Value));
                        }
                        else if (variableType == typeof(string))
                        {
                            variables[s.Key] = GetLayoutItem(s.Value).Trim();
                        }
                        else if (variableType == typeof(double[]))
                        {
                            int size = 130;
                            Layout tempLayout = new Layout();
                            tempLayout.Index = s.Value.Index;
                            tempLayout.Start = s.Value.Start;
                            tempLayout.Length = s.Value.Length;

                            for (int i = 0; i < size; i++)
                            {
                                variables[$"{s.Key}[{i}]"] = double.Parse(GetLayoutItem(tempLayout));

                                if (Configure.SeperationType == "1")
                                {
                                    tempLayout.Index += 1;
                                }
                                else
                                {
                                    tempLayout.Start += s.Value.Length;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    throw new Exception($"변수({s.Key})의 할당 값({GetLayoutItem(s.Value)})이 {variables[s.Key].GetType()} 타입으로 변환 될 수 없습니다.");
                }
            }
        }
        public void ChangeVariables()
        {
            List<VarChg> VarChgList = PV.finder.FindVariableChanger(ProductCode, RiderCode);

            foreach (VarChg s in VarChgList)
            {
                variables[s.변수명] = s.값.Evaluate();
            }
        }

        public string GetLayoutItem(Layout layout)
        {
            string item = null;

            if (Configure.SeperationType == "1")
            {
                if (layout.Index >= 0 && layout.Index < Arr.Length)
                {
                    item = Arr[layout.Index];
                }
                else
                {
                    item = "";
                    //throw new Exception($"인덱스 값이 구분자로 분할 된 라인의 최대 인덱스 값을 초과하였습니다. 구분자 또는 원본 라인을 확인 바랍니다.");
                }
            }
            else
            {
                if (layout.Start + layout.Length <= AdjustedLine.Length)
                {
                    item = AdjustedLine.Substring(layout.Start, layout.Length);
                }
                else
                {
                    //범위 초과부분 자동보정
                    item = "";
                }
            }

            if (string.IsNullOrWhiteSpace(item))
            {
                return "0";
            }

            return item;
        }
        public string GetLayoutMP()
        {
            string item = "";

            foreach (var s in layouts)
            {
                if(s.Value.상품코드 == "Check" || s.Value.FactorName == "ElapseYear")
                {
                    continue;
                }
                else
                {
                    item += GetLayoutItem(s.Value) + "|";
                }
            }

            return item;
        }
        public void AddLineSummary()
        {
            if (!Configure.LineSummaryChecked) return;
            if (PV.sr.BaseStream == null || PV.sw라인요약.BaseStream == null) return;

            LineSummary lineSummary = new LineSummary();

            lineSummary.RiderCode = RiderCode;
            lineSummary.Jong = (int)variables["Jong"];
            lineSummary.S1 = (int)variables["S1"];
            lineSummary.n = (int)variables["n"];
            lineSummary.m = (int)variables["m"];
            lineSummary.Age = (int)variables["Age"];
            PV.lineSummariesTemp.Add(lineSummary);

            while (PV.lineSummariesTemp.Count() % 100000 == 99999 || (PV.sr.EndOfStream && PV.lineSummariesTemp.Count() > 0))
            {
                string selectedRiderCode = PV.lineSummariesTemp.First().RiderCode;
                List<LineSummary> selectedLineSummaries = PV.lineSummariesTemp.Where(x => x.RiderCode == selectedRiderCode).ToList();

                var group1 = selectedLineSummaries.GroupBy(x => x.n);
                var group2 = selectedLineSummaries.GroupBy(x => x.Age + x.n);

                if(group1.Count() > group2.Count())
                {
                    //세만기 가정
                    foreach (var line in selectedLineSummaries)
                    {
                        line.MaturityAge = line.Age + line.n;
                        string key = string.Join("\t", line.RiderCode, line.Jong, line.S1, line.m, line.MaturityAge + "세");

                        if (!PV.lineSummaries.ContainsKey(key))
                        {
                            PV.lineSummaries.Add(key, line);
                        }

                        PV.lineSummaries[key].Count++;
                        PV.lineSummaries[key].Ages.Add(line.Age);
                    }
                }
                else
                {
                    //연만기 가정
                    foreach (var line in selectedLineSummaries)
                    {
                        string key = string.Join("\t", line.RiderCode, line.Jong, line.S1, line.m, line.n) ;

                        if (!PV.lineSummaries.ContainsKey(key))
                        {
                            PV.lineSummaries.Add(key, line);
                        }

                        PV.lineSummaries[key].Count++;
                        PV.lineSummaries[key].Ages.Add(line.Age);
                    }
                }

                PV.lineSummariesTemp.RemoveAll(x => x.RiderCode == selectedRiderCode);
            }
        }

        public string GetPVGeneratorKey()
        {
            List<string> sb = new List<string>();

            string type1 = variables["Substandard_Mode"].ToString() == "sub" ? "할증체(S2)" : "표준체(S2)";
            string type2 = (int)variables["S5"] > 0 ? "저해지(S5)" : "표준형(S5)";
            string type3 = $"{variables["m"]}년납";

            string type = $"{type1}_{type2}_{type3}";

            sb.Add(type);
            sb.Add($"{RiderCode}");
            sb.Add($"Age:{variables["Age"]}");
            sb.Add($"n:{variables["n"]}");
            sb.Add($"m:{variables["m"]}");
            //sb.Add($"nAge:{variables["nAge"]}");
            //sb.Add($"mAge:{variables["mAge"]}");
            sb.Add($"Freq:{variables["Freq"]}");
            //sb.Add($"S1:{variables["S1"]}");
            //sb.Add($"S2:{variables["S2"]}");
            sb.Add($"S3:{variables["S3"]}");
            //sb.Add($"S4:{variables["S4"]}");
            //sb.Add($"S5:{variables["S5"]}");
            sb.Add($"S6:{variables["S6"]}");
            sb.Add($"S7:{variables["S7"]}");
            //sb.Add($"S8:{variables["S8"]}");
            //sb.Add($"S9:{variables["S9"]}");
            //sb.Add($"S10:{variables["S10"]}");

            return string.Join("|", sb);
        }
        public PVCalculator GetPVCalculator()
        {
            int PV_Type = (int)variables["PV_Type"];
            int substandardType = (int)variables["S2"];

            if (substandardType > 0 && (string)variables["Substandard_Mode"] == "None")
            {
                //위험률 1배 적용
                variables["Substandard_Mode"] = "norm"; 
                PVCalculator norm = GetPVCalculator();

                //위험률 k배 적용 S(k)
                variables["Substandard_Mode"] = "sub";
                PVCalculator sub = GetPVCalculator();

                variables["Substandard_Mode"] = "None";

                if (substandardType == 1) return new PVSubstandard(sub, norm);
                if (substandardType == 2) return new PVSubStandardRound(sub, norm);
                if (substandardType == 3) return new PVSubstandardSimple(sub, norm);
                if (substandardType == 4) return new PVSubstandardHanhwaEmergency(sub, norm);

                throw new Exception($"S2 변수의 값 {substandardType}이 적절하지 않습니다.");
            }

            try
            {
                Type type = Type.GetType($"PVPlus.PVCALCULATOR.PVType{PV_Type}");
                PVCalculator cal = (PVCalculator)Activator.CreateInstance(type, this);
                return cal;
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    throw new Exception($"PVType{PV_Type}을 찾을 수 없습니다.");
                }
                if(ex.InnerException is InvalidCastException)
                {
                    throw new Exception("캐스트 변환에 실패하였습니다. 레이아웃에서 값을 가져오거나 VarChg시트에 잘못된 cast지정이 있는지 확인바랍니다. ex)int타입의 변수에 double타입의 변수를 넣는 경우..." );
                }
                if (ex.InnerException is IndexOutOfRangeException)
                {
                    throw new Exception("인덱스가 배열 범위를 벗어났습니다. (Age+n) 값이 131을 초과하였는지 등 배열 인덱스 값 확인이 필요합니다.");
                }

                throw new Exception(ex.InnerException.Message);
            }

        }
        public PVCalculator GetPVCalculator(string otherRiderCode, Dictionary<string, object> otherVariables)
        {
            //helper클래스에서 다른 담보코드 및 변수의 값을 산출하는 메서드 구현시 사용
            string orgRiderCode = RiderCode;
            Dictionary<string, object> orgVariables = variables.Keys.ToDictionary(x => x, y => variables[y]);

            //RiderRule, Variables 변경
            if (otherRiderCode != null) RiderCode = otherRiderCode;

            //다른 담보의 변수를 끌어 올때 Ax메서드 등의 입력파라미터 뿐만 아니라 VarChg변수까지 적용
            //다른 담보의 Layout변수의 경우 상위담보의 Layout과 충돌가능성이 있어 제외하였음
            otherVariables.Keys.ToList().ForEach(x => variables[x] = otherVariables[x]);
            //ChangeVariables();

            PVCalculator cal = GetPVCalculator();

            //RiderRule, Variables 복원
            if (otherRiderCode != null) RiderCode = orgRiderCode;
            orgVariables.Keys.ToList().ForEach(x => variables[x] = orgVariables[x]);

            return cal;
        }
    }

    public class LineSummary
    {
        public string RiderCode { get; set; }
        public int Jong { get; set; }
        public int S1 { get; set; }
        public int n { get; set; }
        public int MaturityAge { get; set; }
        public int m { get; set; }
        public int Age { get; set; }

        public List<int> Ages = new List<int>();
        public int Count { get; set; }

        public string AgeRange()
        {
            return ConvertSequenceToRangeString(Ages);
        }

        public int MidAge()
        {
            if (!Ages.Any()) return 0;

            // MaturityAge가 0이고 Ages에 40이 포함된 경우
            if (MaturityAge == 0 && Ages.Contains(40))
                return 40;

            var distinctAges = Ages.Distinct().OrderBy(x => x).ToList();
            if (distinctAges.Count == 1)
                return distinctAges[0];

            // 중간 인덱스 계산 (짝수 개수일 경우 두 중간값의 평균)
            int middleIndex = distinctAges.Count / 2;
            if (distinctAges.Count % 2 == 0)
            {
                double average = (distinctAges[middleIndex - 1] + distinctAges[middleIndex]) / 2.0;
                return (int)Math.Ceiling(average);

            }
            return distinctAges[middleIndex];
        }

        public double AgeCount()
        {
            if (Ages == null)
                return 0;

            return Ages.Distinct().Count();
        }

        public double AgeMult()
        {
            var ageCount = AgeCount();
            if (ageCount == 0)
                return 0;

            return Count / ageCount;
        }

        public string ConvertSequenceToRangeString(IEnumerable<int> numbers)
        {
            if (numbers == null || !numbers.Any())
                return string.Empty;

            // 열거형을 정렬된 리스트로 변환
            var sortedNumbers = numbers.OrderBy(x => x).Distinct().ToList();

            var result = new StringBuilder();
            int rangeStart = sortedNumbers[0];
            int previous = sortedNumbers[0];

            // 첫 번째 숫자 추가
            result.Append(rangeStart);

            for (int i = 1; i < sortedNumbers.Count; i++)
            {
                int current = sortedNumbers[i];

                // 연속되지 않은 숫자인 경우
                if (current != previous + 1)
                {
                    // 이전 범위 마무리
                    if (previous != rangeStart)
                    {
                        result.Append('~').Append(previous);
                    }
                    result.Append(',').Append(current);
                    rangeStart = current;
                }
                // 마지막 숫자인 경우
                else if (i == sortedNumbers.Count - 1)
                {
                    result.Append('~').Append(current);
                }

                previous = current;
            }

            return result.ToString();
        }
    }

}
