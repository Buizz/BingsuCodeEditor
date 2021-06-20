using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingsuCodeEditor.EpScript
{
    class EpScriptAnalyzer : CodeAnalyzer
    {
        public EpScriptAnalyzer(TextEditor textEditor) : base(textEditor, false)
        {
            string[] keywords = {"object", "static", "once", "if", "else", "while", "for", "function", "foreach",
        "return", "switch", "case", "break", "var", "const", "import", "as", "continue" ,  "true", "True", "false", "False"};


            foreach (var item in keywords)
            {
                //토큰 입력
                AddSubType(item, TOKEN_TYPE.KeyWord);

                //자동완성 초기 입력
                completionDatas.Add(new KewWord(CompletionWordType.KeyWord, item));
            }
            


            codeFoldingManager = new EpScriptFoldingManager(textEditor);
        }

        public override void GetCompletionList(IList<ICompletionData> data)
        {
            for (int i = 0; i < completionDatas.Count; i++)
            {
                data.Add(new CodeCompletionData(completionDatas[i]));
                
            }
        }




        #region #############자동완성#############

        public class KewWord : PreCompletionData
        {
            public KewWord(CompletionWordType completionType, string name) : base(completionType, name)
            {
            }

            //키워드 이름
            public override string listheader
            {
                get
                {
                    return name;
                }
            }
            public override string ouputstring
            {
                get
                {
                    return name;
                }
            }
            public override string desc
            {
                get
                {
                    return name;
                }
            }
        }
        #endregion
    }
}
