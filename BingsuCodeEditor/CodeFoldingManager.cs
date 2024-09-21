using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static BingsuCodeEditor.CodeAnalyzer;

namespace BingsuCodeEditor
{
    public abstract class CodeFoldingManager
    {
        private FoldingManager foldingManager;
        protected TextEditor textEditor;

        public CodeFoldingManager(TextEditor aTextEditor)
        {
            foldingManager = FoldingManager.Install(aTextEditor.TextArea);
            textEditor = aTextEditor;
        }


        public IEnumerable<FoldingSection> GetFoldingData()
        {
            //저장도되야됨
            return foldingManager.AllFoldings;
        }

        public abstract void FodlingExec(List<TOKEN> Tokens, int len, List<NewFolding> Foldings);

        public List<int> SaveFodling()
        {
            List<int> result = new List<int>();
            int foldedindex = 0;
            
            while(foldedindex != -1)
            {
                foldedindex = foldingManager.GetNextFoldedFoldingStart(foldedindex);
                if(foldedindex == -1)
                {
                    break;
                }
                result.Add(foldedindex);
                foldedindex += 1;
            }

            return result;
        }

        public void LoadFodling(List<int> loadeddata)
        {
            this.execFolded = loadeddata;
        }

        public void FoldingFlip(int startoffset, int len)
        {
            //len이 0일 경우
            //하나만 선택한 것이므로 스타트오프셋이 가장 작은 하나만 여닫는다.
            if(len == 0)
            {
                ICollection<FoldingSection> t = foldingManager.GetFoldingsContaining(startoffset);
                if (t.Count == 0) return;

                List<FoldingSection> sorted = t.ToList();
                sorted.Sort((x, y) => x.StartOffset.CompareTo(y.StartOffset));

                if (sorted.Last().IsFolded)
                {
                    UnFolding(sorted.Last().StartOffset);
                }
                else
                {
                    Folding(sorted.Last().StartOffset);
                }

            }
            else
            {
                FoldingSection st = foldingManager.GetNextFolding(startoffset);
                if (st == null) return;

                FoldingSection sec = st;
                int foldoffset = sec.StartOffset;

                List<FoldingSection> folders = new List<FoldingSection>();
                
                while(sec != null)
                {
                    sec = foldingManager.GetNextFolding(foldoffset);
                    if (sec == null) break;
                    if (sec.StartOffset <= startoffset + len)
                    {
                        folders.Add(sec);
                        foldoffset = sec.StartOffset + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                //전부다 접혀(fold) 있어야 펼치기(unfold)
                bool IsExecUnFold = true;
                foreach (var item in folders)
                {
                    if (!item.IsFolded)
                    {
                        IsExecUnFold= false;
                        break;
                    }
                }

                foreach (var item in folders)
                {
                    if (IsExecUnFold)
                    {
                        UnFolding(item.StartOffset);
                    }
                    else
                    {
                        Folding(item.StartOffset);
                    }
                }
            }
        }

        public void Folding(int startindex)
        {
            if(execFolded == null)
            {
                execFolded = new List<int>();
            }
            execFolded.Add(startindex);
        }
        public void UnFolding(int startindex)
        {
            if (execUnFolded == null)
            {
                execUnFolded = new List<int>();
            }
            execUnFolded.Add(startindex);
        }


        private List<int> execFolded;
        private List<int> execUnFolded;
        public void FoldingUpdate(List<TOKEN> Tokens, int len)
        {
            List<NewFolding> newFoldings = new List<NewFolding>();
            FodlingExec(Tokens, len, newFoldings);

            newFoldings.Sort((x, y) => x.StartOffset.CompareTo(y.StartOffset));
          
            int lencash = len;
            try
            {
                textEditor.Dispatcher.Invoke(new Action(() => {
                    if (lencash == textEditor.Document.TextLength)
                    {
                        foldingManager.UpdateFoldings(newFoldings, -1);

                        if (execFolded != null)
                        {
                            foreach (var foldedindex in execFolded)
                            {
                                foreach (var item in foldingManager.GetFoldingsAt(foldedindex))
                                {
                                    item.IsFolded = true;
                                }
                            }
                            execFolded = null;
                        }
                        if (execUnFolded != null)
                        {
                            foreach (var foldedindex in execUnFolded)
                            {
                                foreach (var item in foldingManager.GetFoldingsAt(foldedindex))
                                {
                                    item.IsFolded = false;
                                }
                            }
                            execUnFolded = null;
                        }
                    }
                }), DispatcherPriority.Normal);
            }
            catch (Exception)
            {
            }
   
        }
    }
}
