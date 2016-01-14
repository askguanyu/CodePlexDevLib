using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel.Web;

namespace DevLib.Samples
{
    [ServiceContract]
    public interface IWcfAnotherTest
    {
        [OperationContract]
        string MyAnotherOperation(string arg1, int arg2);
    }
}
