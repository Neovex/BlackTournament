using System;
using System.ServiceModel;
using BlackTournament.System;

namespace BlackTournament.Net.Contract
{
    [ServiceContract(CallbackContract = typeof(IGameClient))]
    public interface IGameServer : IServer
    {
        [OperationContract]
        string GetLevel();

        [OperationContract(IsOneWay = true)]
        void ChangeLevel(int id, string lvl);

        [OperationContract(IsOneWay = true)]
        void StopServer(int id);

        [OperationContract(IsOneWay = true)]
        void ProcessGameAction(int id, GameAction action);
    }
}