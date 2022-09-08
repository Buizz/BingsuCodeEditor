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
                        foldingManager.UpdateFoldings(newFoldings, -1);
                }), DispatcherPriority.Normal);
            }
            catch (Exception)
            {
            }
   
        }
    }
}
