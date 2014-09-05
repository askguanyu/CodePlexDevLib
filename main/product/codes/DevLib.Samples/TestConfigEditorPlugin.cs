using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevLib.Configuration;
using DevLib.ExtensionMethods;

namespace DevLib.Samples
{
    public class TestConfigEditorPlugin : IWinFormConfigEditorPlugin
    {
        public TestConfigEditorPlugin()
        {
            this.PluginName = "List<TestConfig>";
            this.ConfigObjectType = typeof(List<TestConfig>);
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

        public object Open(string filename)
        {
            return new List<TestConfig>();
        }

        public void Save(string filename, object configObject)
        {
            configObject.WriteXml(filename, true, true);
        }
    }
}
