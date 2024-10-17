using Flee.PublicTypes;
using PVPlus.PVCALCULATOR;
using PVPlus.RULES;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace PVPlus
{
    class PV
    {
        private MainPVForm form;

        private StreamReader sr;
        private StreamWriter sw정상건;
        private StreamWriter sw오차건;
        private StreamWriter sw오차건원본;
        private StreamWriter sw오류건원본;
        private StreamWriter sw신계약비한도초과건;
        private StreamWriter sw부가보험료한도초과건;

        public static DataReader reader;
        public static RuleFinder finder;
        public static VariableCollection variables;

        public int 정상건Cnt = 0;
        public int 오차건Cnt = 0;
        public int 오류건Cnt = 0;
        public int 한도초과Cnt = 0;
        public string ProgressMsg = "";
        public bool IsCanceled = false;

        public PV(MainPVForm form)
        {
            this.form = form;
        }

        public void Run()
        {
            try
            {
                SetData();
                SetStreamWriters();

                using (sr)
                using (sw정상건)
                using (sw오차건)
                using (sw오차건원본)
                using (sw오류건원본)
                using (sw신계약비한도초과건)
                using (sw부가보험료한도초과건)
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        CheckLine(line);

                        ProgressMsg = $"\r정상건:{정상건Cnt}, 오차건:{오차건Cnt}, 오류건:{오류건Cnt}, 진행률:{100.0 * sr.BaseStream.Position / sr.BaseStream.Length:F2}%";
                        if (IsCanceled) break;
                    }
                }
            }
            catch (Exception ex)
            {
                form.ReportExeption(ex);
            }

            System.Threading.Thread.Sleep(100); //타이머 이벤트 종료대기
        }

        public void RunSample()
        {
            try
            {
                SetData();
                SetStreamWriters();

                using (sr)
                using (sw정상건)
                using (sw오차건)
                using (sw오차건원본)
                using (sw오류건원본)
                using (sw신계약비한도초과건)
                using (sw부가보험료한도초과건)
                {
                    string tableName = Configure.PVSTableInfo.Name;
                    string tableExtension = Configure.PVSTableInfo.Extension;
                    string tableNameWithoutExtension = tableName.Replace(tableExtension, "");

                    DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Configure.PVSTableInfo.DirectoryName, "Splited", tableNameWithoutExtension));
                    if(!directoryInfo.Exists) 
                    {
                        throw new Exception("간이검증을 위해 Split 작업이 선행되어야 합니다.");
                    }

                    FileInfo[] validFiles = directoryInfo.GetFiles().Where(x => !x.Name.Contains("_")).ToArray();
                    Random random = new Random();
                    List<string> randomLines = new List<string>();

                    foreach (FileInfo file in validFiles)
                    {
                        using (var reader = new StreamReader(file.FullName))
                        {
                            long randomPosition = 0;
                            long lineLength = 0;

                            for (int i = 0; i < 12; i++)
                            {
                                reader.DiscardBufferedData();
                                reader.BaseStream.Position = randomPosition;
                                reader.ReadLine();

                                string line = reader.ReadLine();
                                if (line != null)
                                {
                                    randomLines.Add(line);
                                    lineLength = line.Length;
                                    randomPosition += lineLength;
                                }
                                else
                                {
                                    break;
                                }

                                randomPosition += random.Next((int)file.Length / 12);
                                if (i == 12 -2)
                                {
                                    randomPosition = file.Length - lineLength * 2 + 2;
                                }
                            }

                        }
                    }

                    int totalCount = randomLines.Count();
                    for (int i = 0; i < totalCount; i++)
                    {
                        CheckLine(randomLines[i]);

                        ProgressMsg = $"\r정상건:{정상건Cnt}, 오차건:{오차건Cnt}, 오류건:{오류건Cnt}, 진행률:{100.0 * (i+1) / totalCount:F2}%";
                        if (IsCanceled) break;
                    }
                }
            }
            catch (Exception ex)
            {
                form.ReportExeption(ex);
            }

            System.Threading.Thread.Sleep(100); //타이머 이벤트 종료대기
        }

        public void CheckLine(string line)
        {
            try
            {
                reader.InitializeAllVariables();
                LineInfo lineInfo = new LineInfo(line);
                PVResult r = lineInfo.CalculateLine();
                r.SetResult();

                string 검토결과 = r.ResultType;

                if (검토결과 == "정상")
                {
                    if (정상건Cnt == 0) sw정상건.WriteLine(r.GetHeadLine());
                    sw정상건.WriteLine(r.GetLine());
                    정상건Cnt++;
                }
                else if (검토결과 == "오차")
                {
                    if (오차건Cnt == 0) sw오차건.WriteLine(r.GetHeadLine());
                    sw오차건.WriteLine(r.GetLine());
                    sw오차건원본.WriteLine(r.OrgLine);
                    오차건Cnt++;
                }
                if (Configure.TableType == TableType.StdAlpha && Configure.LimitExcessChecked)
                {
                    PVResult_LimitExcessA2 rA2 = new PVResult_LimitExcessA2(r.cal);
                    rA2.SetResult();

                    if (rA2.ResultType == "한도초과")
                    {
                        sw신계약비한도초과건.WriteLine(rA2.GetLine());
                        한도초과Cnt++;
                    }

                }
                if (Configure.TableType == TableType.P && Configure.LimitExcessChecked)
                {
                    PVResult_LimitExcessP rP = new PVResult_LimitExcessP(r.cal);
                    rP.SetResult();

                    if (rP.ResultType == "한도초과")
                    {
                        sw부가보험료한도초과건.WriteLine(rP.GetLine());
                        한도초과Cnt++;
                    }
                }
            }
            catch
            {
                sw오류건원본.WriteLine(line);
                오류건Cnt++;
            }
        }

        public void EvaluateSInfo()
        {
            string productCode = Configure.ProductCode;
            TableType tableType = Configure.TableType;

            try
            {
                SetData();
                finder.minSByMinSKey.Clear();

                List<SInfo> sList = DataReader.SInfos.ToList();
                var sDict = sList.OrderBy(x => x.VarAdd.Contains("S5->2")).GroupBy(x => x.SKey);

                int cnt = 0;
                int total = sList.Count();

                foreach (var sl in sDict)
                {
                    foreach (SInfo s in sl)
                    {
                        try
                        {
                            reader.InitializeAllVariables();
                            LineInfo lineInfo = new LineInfo(s);
                            lineInfo.CalculateSInfo();
                        }
                        catch (Exception ex)
                        {
                            s.ErrorMessage = ex.Message.Replace("\r", "").Replace("\n", " ");
                        }
                        finally
                        {
                            cnt++;
                            ProgressMsg = $"\r진행도:{cnt}/{total}";
                        }
                    }

                    finder.minSByMinSKey[sl.Key] = sl.Min(x => x.S);
                    sl.ToList().ForEach(x => x.Min_S = finder.minSByMinSKey[sl.Key]);
                }

                reader.GenerateEvaluatedSInfosText(sList);
                System.Threading.Thread.Sleep(100); //타이머 이벤트 종료대기
            }
            catch (Exception ex)
            {
                form.ReportExeption(ex);
            }
        }

        public void TestSample(string line, SamplePVForm form)
        {
            string productCode = Configure.ProductCode;
            TableType tableType = Configure.TableType;

            try
            {
                form.ClearMessage();
                SetData();
                Sample sample = new Sample(line);
                PVResult r = sample.result;
                r.SetResult();

                string 검증결과 = r.ResultType;

                if (검증결과 == "정상")
                {
                    form.ReportMessage("정상입니다.");
                    form.ReportMessage(sample.result.GetHeadLine());
                    form.ReportMessage(sample.result.GetLine());
                }
                else
                {
                    form.ReportMessage("오차발생.");
                    form.ReportMessage(sample.result.GetHeadLine());
                    form.ReportMessage(sample.result.GetLine());
                }

                sample.MakeSample();
                sample.WriteSample();
            }
            catch (Exception ex)
            {
                form.ReportExeption(ex);
            }
        }

        public void CheckExcess()
        {
            string productCode = Configure.ProductCode;
            TableType tableType = Configure.TableType;

            try
            {
                SetData();
                List<SInfo> sList = DataReader.EvaluatedSInfos;

                FileInfo ExcessFI = new FileInfo(Path.Combine(Configure.WorkingDI.FullName, "AlphaExcessCheck.txt"));
                StreamWriter swAlphaExcessCheck = new StreamWriter(ExcessFI.FullName, false, Encoding.UTF8);

                int cnt = 0;
                int total = sList.Count();

                using (swAlphaExcessCheck)
                {
                    foreach (SInfo s in sList)
                    {
                        try
                        {
                            reader.InitializeAllVariables();
                            LineInfo lineInfo = new LineInfo(s);
                            PVResult r = lineInfo.CalculateLine();

                            PVResult_LimitExcessA rA = new PVResult_LimitExcessA(r.cal);
                            rA.SetResult();

                            if (cnt == 0) swAlphaExcessCheck.WriteLine(rA.GetHeadLine());
                            swAlphaExcessCheck.WriteLine(rA.GetLine());

                            cnt++;
                        }
                        catch (Exception ex)
                        {
                            s.ErrorMessage = ex.Message.Replace("\r", "").Replace("\n", " ");
                            cnt++;
                        }
                        finally
                        {
                            ProgressMsg = $"\r진행도:{cnt}/{total}";
                        }
                    }

                    System.Threading.Thread.Sleep(100); //타이머 이벤트 종료대기
                }
            }
            catch (Exception ex)
            {
                form.ReportExeption(ex);
            }

        }

        private void SetStreamWriters()
        {
            FileInfo tableFI = Configure.PVSTableInfo;
            string tableAddName = "";

            string tableFullName = tableFI.FullName;
            string tableExtension = tableFI.Extension;
            string tableFullNameWithoutExtension = tableFullName.Replace(tableExtension, "");

            sr = new StreamReader(tableFullName, Encoding.UTF8);
            sw정상건 = new StreamWriter($"{tableFullNameWithoutExtension}_{tableAddName}정상건{tableExtension}");
            sw오차건 = new StreamWriter($"{tableFullNameWithoutExtension}_{tableAddName}오차건{tableExtension}");
            sw오차건원본 = new StreamWriter($"{tableFullNameWithoutExtension}_{tableAddName}오차건원본{tableExtension}");
            sw오류건원본 = new StreamWriter($"{tableFullNameWithoutExtension}_{tableAddName}오류건원본{tableExtension}");
            sw신계약비한도초과건 = (Configure.TableType == TableType.StdAlpha && Configure.LimitExcessChecked) ? new StreamWriter($"{tableFullNameWithoutExtension}_신계약비한도초과건{tableExtension}") : null;
            sw부가보험료한도초과건 = (Configure.TableType == TableType.P && Configure.LimitExcessChecked) ? new StreamWriter($"{tableFullNameWithoutExtension}_부가보험료한도초과건{tableExtension}") : null;

        }

        private void SetData()
        {
            if(reader == null)
            {
                reader = new DataReader();
            }
            else
            {
                reader.ReadOrUpdateAll();
            }
            
            finder = new RuleFinder(reader);
            variables = reader.Context.Variables;
        }
    }
}
