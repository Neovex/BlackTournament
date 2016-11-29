using System.ServiceModel;

namespace BlackTournament.Net.Contract
{
    [ServiceContract]
    public interface IClient
    {
        [OperationContract(IsOneWay = true)]
        void Message(int from, string msg);

        [OperationContract(IsOneWay = true)]
        void ClientConnected(int clientId, string name);

        [OperationContract(IsOneWay = true)]
        void ClientDisconnected(int clientId);
    }
}