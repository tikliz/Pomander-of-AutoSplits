using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

using PomanderoSplit.Utils;
using PomanderoSplit.Connection;

namespace PomanderoSplit.Windows;

public partial class ConfigWindow : Window, IDisposable
{
    public void DrawConnectionTab()
    {
        using var tab = ImRaii.TabItem("Connection");
        if (!tab) return;


        static bool DrawEnumRadioButtons<TEnum>(ref int value) where TEnum : Enum
        {
            var result = false;
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();

            foreach (TEnum enumValue in enumValues[..^1])
            {
                if (ImGui.RadioButton(enumValue.ToString(), ref value, Convert.ToInt32(enumValue))) result = true;
                ImGui.SameLine();
            }
            if (enumValues.Length == 0) return result;

            TEnum lastEnumValue = enumValues[^1];
            if (ImGui.RadioButton(lastEnumValue.ToString(), ref value, Convert.ToInt32(lastEnumValue))) result = true;

            return result;
        }

        ImGui.Text("Client");
        ImGui.SameLine();

        var clientValue = (int)Plugin.Configuration.ClientType;
        if (DrawEnumRadioButtons<ClientType>(ref clientValue) && (ClientType)clientValue != Plugin.Configuration.ClientType)
        {
            Plugin.Configuration.ClientType = (ClientType)clientValue;
            Plugin.Configuration.Save();
            Plugin.ConnectionManager.Init();
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var status = Plugin.ConnectionManager.Status();

        if (ImGui.Button(status == ClientStatus.Connected ? "Reconnect" : "Connect"))
        {
            if (status == ClientStatus.Connected) Plugin.ConnectionManager.Reconnect();
            else Plugin.ConnectionManager.Connect();
        }

        ImGui.SameLine();
        if (status != ClientStatus.Connected)
        {
            ImGui.BeginDisabled();
            ImGui.Button("Disconnect");
            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button("Disconnect")) Plugin.ConnectionManager.Disconnect();
        }
        
        ImGui.SameLine();
        Widget.StatusCircle(status);

        ImGui.Spacing();

        var autoConnectValue = Plugin.Configuration.AutoConnect;
        if (ImGui.Checkbox("Connect on start", ref autoConnectValue))
        {
            Plugin.Configuration.AutoConnect = autoConnectValue;
            Plugin.Configuration.Save();
        }

        ImGui.Spacing();

        if (Plugin.ConnectionManager.Client is LiveSplitSocket client)
        {
            ImGui.Text("Client Settings:");
            if (ImGui.InputTextWithHint("Address", "tcp://127.0.0.1:16834/", ref addressValue, 64)) {}

            var AddressHasChanged = addressValue != Plugin.Configuration.Address;
            ImGui.BeginDisabled(!AddressHasChanged);
            ImGui.SameLine();
            if (ImGui.Button("Save"))
            {
                Plugin.Configuration.Address = addressValue;
                Plugin.Configuration.Save();
                
                Task.Run(() => client.ChangeUri(addressValue));
            }
            ImGui.EndDisabled();
        }
    }
}