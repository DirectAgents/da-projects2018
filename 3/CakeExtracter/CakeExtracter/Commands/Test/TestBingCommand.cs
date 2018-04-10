using System;
using System.ComponentModel.Composition;
//using BingAds;
using CakeExtracter.Common;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestBingCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public TestBingCommand()
        {
            IsCommand("testBing");
        }

        //NOTE: Need to add "[STAThread]" to Main method in CakeExtracter.ConsoleApplication

        public override int Execute(string[] remainingArguments)
        {
            //var username = "dacoursehero"; //"leo.directagents@gmail.com";
            //var password = "Agentdan"; //"Agentdan1";
            //var clientId = "16227075"; // Customer ID ?

            //var bingAuth = new BingAuth(username, password, clientId);
            //var tokens = bingAuth.GetInitialTokens();

            return 0;
        }
    }
}
