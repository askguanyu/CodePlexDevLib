using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevLib.Settings;

namespace DevLib.Samples
{
    public class TestConfigEditorPlugin : IWinFormConfigEditorPlugin
    {
        public TestConfigEditorPlugin()
        {
            this.PluginName = "Hello";
            this.ConfigObjectType = typeof(TestConfig);
        }

        public string PluginName
        {
            get;
            set;
        }

        public Type ConfigObjectType
        {
            get;
            set;
        }

        public object Open(string fileName)
        {
            return new TestConfig();
        }

        public void Save(string fileName, object configObject)
        {
            //saved
        }
    }
}
