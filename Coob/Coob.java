var latestVersion = 3;

AddHook("OnInitialize", function (args)
{
    var file = System.IO.File.OpenText(path);
    var contents = file.ReadToEnd();
    file.Close();
    
    args.WorldSeed = 1234567890;
});

AddHook("OnClientConnect", function (args)
{
    var ip = args.IP;
    LogInfo("Client connecting from " + ip);
});

AddHook("OnClientVersion", function (args)
{
    LogInfo(args.Client.IP + " version: " + args.Version + ".");
});

AddHook("OnClientJoin", function (args)
{
    var client = args.Client;
    LogInfo("Client " + client.Entity.Name + " has joined");
});

AddHook("OnEntityUpdate", function (args)
{
    
});

AddHook("OnChatMessage", function (args)
{
    var client = args.Client;
    var message = args.Message;
	/*if(message.charAt(0) == '.'){
		
		var parts = message.split(' ');
		coob.SendServerMessage("Time set to 1 hours.");
		LogInfo("<" + parts);

		if(parts[0] === ".time"){
			var time = parseFloat(parts[1]);
			
			if (!isNaN(time)) {
				coob.SetTime(1, time);
				coob.SendServerMessage("Time set to " + time + " hours.");
			}
		}
		return true;
	}else{*/
		LogInfo("<" + client.Entity.Name + "> " + message);

		var time = parseFloat(message);

		if (!isNaN(time)) {
			coob.SetTime(1, time);
			coob.SendServerMessage("Time set to " + time + " hours.");
			args.Canceled = true;
		}
	//} dont work =_=

});

AddHook("OnQuit", function (args)
{
    
});
