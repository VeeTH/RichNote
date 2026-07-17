using System.Collections.Generic;

namespace RichNote.Types
{
    internal class TabStateModel
    {
        public class TabData
        {
            public string Path { get; set; }
            public string Header { get; set; }
            public string Type { get; set; }
            public string Content { get; set; }
        }

        public class TabState
        {
            public List<TabData> Tabs { get; set; }
        }
    }
}
