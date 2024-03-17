using System;
using System.IO;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Game.Command;
using ImGuiNET;

namespace Test;

public class Test: IDalamudPlugin {
	public string Name => "Penumbra Issue";
	
	[PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface Interface  {get; private set;} = null!;
	[PluginService][RequiredVersion("1.0")] public static ICommandManager        Commands   {get; private set;} = null!;
	[PluginService][RequiredVersion("1.0")] public static IPluginLog             Logger     {get; private set;} = null!;
	
	private const string command = "/penumbraissue";
	
	public Test() {
		Interface.UiBuilder.Draw += Draw;
		Commands.AddHandler(command, new CommandInfo((cmd, args) => {if(cmd == command) TheIssue();}) {
			HelpMessage = ""
		});
	}
	
	public void Dispose() {
		Interface.UiBuilder.Draw -= Draw;
		Commands.RemoveHandler(command);
	}
	
	private void Draw() {
		ImGui.Begin("Penumbra Issue");
		if(ImGui.Button("Issue")) TheIssue();
		ImGui.End();
	}
	
	private void TheIssue() {
		var root = $"{Interface.GetIpcSubscriber<string>("Penumbra.GetModDirectory").InvokeFunc()}/IssueExample";
		Directory.CreateDirectory(root);
		File.WriteAllText($"{root}/meta.json", """{"FileVersion":3,"Name":"IssueExample","Author":"","Description":"","Version":"1.0.0","Website": "","ModTags": []}""");
		File.WriteAllText($"{root}/default_mod.json", """{"Name":"","Description":"","Files":{},"FileSwaps":{},"Manipulations":[]}""");
		Interface.GetIpcSubscriber<string, byte>("Penumbra.AddMod").InvokeFunc("IssueExample");

		var option = """{"Name": "option","Description":"","Priority":1,"Type":"Single","DefaultSettings":0,"Options":["""; // ]}
		var sub_option1 = """{"Name":"1","Description":"","Files":{},"FileSwaps":{},"Manipulations":[]}""";
		var sub_option2 = """{"Name":"2","Description":"","Files":{},"FileSwaps":{},"Manipulations":[]}""";

		File.WriteAllText($"{root}/group_001_option.json", option + sub_option1 + "]}");
		Interface.GetIpcSubscriber<string, string, byte>("Penumbra.ReloadMod").InvokeFunc("IssueExample", "");
		// Interface.GetIpcSubscriber<string, string, string, string, string, byte>("Penumbra.TrySetModSetting").InvokeFunc("Default", "IssueExample", "", "option", "1");

		File.WriteAllText($"{root}/group_001_option.json", option + sub_option1 + "," + sub_option2 + "]}");
		Interface.GetIpcSubscriber<string, string, byte>("Penumbra.ReloadMod").InvokeFunc("IssueExample", "");

		try {
			Interface.GetIpcSubscriber<string, string, string, string, string, byte>("Penumbra.TrySetModSetting").InvokeFunc("Default", "IssueExample", "", "option", "1");
		} catch(Exception e) {
			Logger.Error(e, "");
		}
	}
}