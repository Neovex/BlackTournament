using System;
using System.ServiceModel;
namespace BlackTournament.Net.Contract
{
    [ServiceContract(CallbackContract = typeof(IClient))]
    public interface IServer
    {
        [OperationContract]
        SubscriptionResult Subscribe(String userName);

        [OperationContract]
        string ChangeUserName(string name);

        [OperationContract(IsOneWay = true)]
        void Message(string msg);
    }
}