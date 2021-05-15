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
    public class CodeFoldingManager
    {
        private FoldingManager foldingManager;
        private TextEditor textEditor;

        public CodeFoldingManager(TextEditor aTextEditor)
        {
            foldingManager = FoldingManager.Install(aTextEditor.TextArea);
            textEditor = aTextEditor;
        }


        public void FoldingUpdate(List<TOKEN> Tokens, int len)
        {
            List<NewFolding> newFoldings = new List<NewFolding>();
            newFoldings.Add(new NewFolding(0, len));


            int lencash = len;

            textEditor.Dispatcher.Invoke(new Action(()=> {
                if(lencash == textEditor.Document.TextLength)
                    foldingManager.UpdateFoldings(newFoldings, -1);
            }), DispatcherPriority.Normal);
        }
    }
}
