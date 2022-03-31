using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BingsuCodeEditor.LineColorDrawer
{
    public class ErrorUnderLine : DocumentColorizingTransformer
    {
        private readonly CodeAnalyzer codeAnalyzer;
        TextDecorationCollection myCollection;
        public ErrorUnderLine(CodeAnalyzer codeAnalyzer)
        {
            this.codeAnalyzer = codeAnalyzer;



            // Create an underline text decoration. Default is underline.
            TextDecoration myUnderline = new TextDecoration();

            // Create a linear gradient pen for the text decoration.
            Pen myPen = new Pen();
            myPen.Brush = Brushes.OrangeRed;
            //myPen.Brush.Opacity = 0.5;
            myPen.Thickness = 3;
            myPen.DashStyle = DashStyles.DashDot;
            myUnderline.Pen = myPen;
            myUnderline.PenThicknessUnit = TextDecorationUnit.FontRecommended;
            myUnderline.PenOffset = 2;

            // Set the underline decoration to a TextDecorationCollection and add it to the text block.
            myCollection = new TextDecorationCollection();
            myCollection.Add(myUnderline);
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            TokenAnalyzer tokenAnalyzer;
            if (codeAnalyzer is null)
            {
                return;
            }
            tokenAnalyzer = codeAnalyzer.tokenAnalyzer;

            if (tokenAnalyzer is null)
            {
                return;
            }



            int lineStartOffset = line.Offset;
            int lineEndOffset = line.Offset + line.Length;
            string text = CurrentContext.Document.GetText(line);






            ChangeLinePart(
              lineStartOffset, // startOffset
              lineEndOffset, // endOffset
              element => element.TextRunProperties.SetTextDecorations(myCollection)
            );


        }
    }
}
