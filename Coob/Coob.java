var latestVersion = 3;

var admin = {};
admin.whiteList = {};
admin.banList = {};
admin.opList = {};
admin.whiteListEnabled = false;

var io = {};
io.GetFileText = function (path)
{
    var file = System.IO.File.OpenText(path);
    var contents = file.ReadToEnd();
    file.Close();
    
    return contents;
};

function LoadInfo(path)
{
    var result = {};

    if (!System.IO.File.Exists(path)) {
        try {
            LogInfo("Creating " + path);
            System.IO.File.CreateText(path).Close();
        }
        catch (e) {
            LogError("Could not create " + path + "! (" + e + ")");
        }
    }
    else {
        try {
            var contents = io.GetFileText(path);
            contents = contents.Replace("\r\n", "\n");
            var lines = contents.split("\n");

            for (var i = 0; i < lines.length; ++i) {
                var line = lines[i];
                if (line === "")
                    continue;

                result[line] = true; // Make it not undefined.
                //LogInfo("Added " + line + " from " + path + ".");
            }
        }
        catch (e) {
            LogError("Could not load " + path + "! (" + e + ")\n");
        }
    }
    
    return result;
};

function onInitialize()
{
    admin.opList = LoadInfo("admins.txt");
    admin.banList = LoadInfo("bans.txt");
    admin.whiteList = LoadInfo("whitelist.txt");
    
    return true;
}

function onQuit()
{
    // Todo: Save admins/whitelist/bans.
}

function onClientConnect(ip)
{
    if (admin.banList[ip] != undefined)
    {
        LogInfo("Banned user kicked: " + ip);
        return false;
    }
    else if (admin.whiteListEnabled && admin.whiteList[ip] == undefined)
    {
        LogInfo("Non-whitelisted user kicked: " + ip);
        return false;
    }

    LogInfo("Client connecting from " + ip);
    return true;
}

function onClientDisconnect() {
}

function onClientVersion(version, client) {
}

function onClientJoin(client, ip) {
    LogInfo("Client " + client.Entity.Name + " has joined");
    return true;
}

function onEntityUpdate(entity, changed, client) {
    return true;
}

function onChatMessage(message, client) {
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
		}
	//} dont work =_=

    return true;
}
