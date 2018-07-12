using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace ProgressRenderer
{

    public class SmallMessageBox : Window
    {

        string text;

        public SmallMessageBox(string text)
        {
            this.text = text;
            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(240f, 75f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            TextAnchor backupAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(inRect, text);
            Text.Anchor = backupAnchor;
        }

        public void Close()
        {
            Close(false);
        }

    }

}
