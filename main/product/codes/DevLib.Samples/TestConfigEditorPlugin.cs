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

        public object Open(string fileName)
        {
            return new List<TestConfig>();
        }

        public void Save(string fileName, object configObject)
        {
            configObject.WriteXml(fileName, true, true);
        }
    }
}
