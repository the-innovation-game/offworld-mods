using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Offworld.AppCore;
using Offworld.GameCore;
using UnityEngine.Events;
using Offworld.Algorithms;
using Offworld.MyGameCore;

public class UpdateAISettings : IMohawkEventHandler
{
    public static UpdateAISettings instance = new UpdateAISettings();
    public void OnMouseEnter(GuiActionInfo actionInfo, RectTransform rect) { }
    public void OnMouseExit(GuiActionInfo actionInfo, RectTransform rect) { }
    public bool OnHover(GuiActionInfo actionInfo, RectTransform rect) => false;
    public void OnClick(GuiActionInfo actionInfo, RectTransform rect) { }
    public void OnDisabledClick(GuiActionInfo actionInfo, RectTransform rect) { }
    public void OnRightClick(GuiActionInfo actionInfo, RectTransform rect) { }
    public void OnDoubleClick(GuiActionInfo actionInfo, RectTransform rect) { }
    public void OnSliderChange(GuiActionInfo actionInfo, RectTransform rect, float newValue) { }
    public void OnToggleChange(GuiActionInfo actionInfo, RectTransform rect, bool newValue) { }
    public void OnTextChange(GuiActionInfo actionInfo, RectTransform rect, string newValue) { }
    public void OnTextEndEdit(GuiActionInfo actionInfo, RectTransform rect, string newValue) { }
}
public class TheInnovationGameMod : ModEntryPointAdapter
{
    static TheInnovationGameMod()
    {
        RandManager.ApplyPatches();
        MyGameFactory.Setup();
    }

    public static int GetNumAIs()
    {
        foreach (DropdownControl dropDown in GameObject.FindObjectsOfType<DropdownControl>())
        {
            if (dropDown.GetActionInfo() != null && dropDown.GetActionInfo() != null && dropDown.GetActionInfo().data1 == 6)
                return dropDown.getSelectedIndex();
        }
        return -1;
    }
    public static Button? GetPlayGameButton()
    {
        foreach (TextButton btn in GameObject.FindObjectsOfType<TextButton>())
        {
            if (btn.GetActionInfo() != null && btn.GetActionInfo().data1 == 10 && btn.label.text == "Play Game")
                return btn.GetComponent<Button>();
        }
        return null;
    }

    static int textCount = 0;
    public static void DisplayText(string text)
    {
        GUI.Label(new Rect(Screen.width - 200, 25 * textCount++, 200, 50), text);
    }
    static int prevNumAIs = -1;
    static GameServer? prevGameServer = null;
    public override void OnGUI()
    {
        textCount = 0;
        DisplayText("The-Innovation-Game Mod");
        var gameServer = AppGlobals.GameGlobals.GameServer;
        if (gameServer == null)
        {
            if (prevGameServer != null)
                Globals.SetFactory(new GameFactory());

            var btn = GetPlayGameButton();
            var numAIs = GetNumAIs();
            DisplayText($"Skirmish Screen: {btn != null}");
            if (btn != null)
            {
                DisplayText($"#AI Players: {numAIs}");
                if (prevNumAIs != numAIs)
                {
                    prevNumAIs = numAIs;
                    UnityAction listener = delegate ()
                    {
                        btn.interactable = false;
                        Globals.SetFactory(new MyGameFactory());
                        MyGameFactory.LiveGame = true;
                        MyGameFactory.ResetPlayers();

                        var popup = PopupManager.addPopup(
                            "The-Innovation-Game",
                            $"Select Debug Mode:",
                            new List<string>() { "OK" },
                            new List<Action>(){
                                new Action(() => {
                                    var dropdown = PopupManager.GetTopPopup().GetComponentInChildren<DropdownControl>();
                                    MyGameFactory.DebugMode = dropdown.getSelectedIndex() != 0;
                                    PopupManager.removeTopPopup();
                                    btn.interactable = true;
                                    btn.onClick = new Button.ButtonClickedEvent();
                                })
                            },
                            null
                        );
                        var container = popup.description.transform.parent.gameObject;
                        MohawkUI.createDropdownControl(
                            UpdateAISettings.instance,
                            new GuiActionInfo(2),
                            new bool[] { false, true }.Select(b => new DropdownOption($"{b}", new GuiActionInfo(2))).ToList(),
                            0,
                            container,
                            true
                        );

                        for (int i = 0; i < numAIs; i++)
                        {
                            popup = PopupManager.addPopup(
                                "The-Innovation-Game",
                                $"AI {numAIs - i}",
                                new List<string>() { "OK" },
                                new List<Action>(){
                                new Action(() => {
                                    foreach(var dropdown in PopupManager.GetTopPopup().GetComponentsInChildren<DropdownControl>())
                                    {
                                        int idx = dropdown.getSelectedIndex();
                                        switch(dropdown.GetActionInfo().data1)
                                        {
                                            case 0:
                                                MyGameFactory.playerAI.Add(MyGameFactory.availableAIs[idx]);
                                                break;
                                            case 1:
                                                MyGameFactory.playerHandicap.Add((HandicapType)idx);
                                                break;
                                        }
                                    }
                                    PopupManager.removeTopPopup();
                                })
                                },
                                null
                            );
                            container = popup.description.transform.parent.gameObject;
                            MohawkUI.createLabel("Select Algorithm:", container);
                            MohawkUI.createDropdownControl(
                                UpdateAISettings.instance,
                                new GuiActionInfo(0),
                                MyGameFactory.availableAIs.Select(t => new DropdownOption(t.Name, new GuiActionInfo(0))).ToList(),
                                MyGameFactory.availableAIs.IndexOf(typeof(DefaultAI)),
                                container,
                                true
                            );
                            MohawkUI.createLabel("Select Handicap:", container);
                            MohawkUI.createDropdownControl(
                                UpdateAISettings.instance,
                                new GuiActionInfo(1),
                                MyGameFactory.handicaps.Select(h => new DropdownOption(h, new GuiActionInfo(1))).ToList(),
                                0,
                                container,
                                true
                            );
                        }
                    };
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(listener);
                }
                for (int i = 0; i < MyGameFactory.playerAI.Count; i++)
                    DisplayText($"{MyGameFactory.playerAI[i].Name}, {MyGameFactory.handicaps[(int)MyGameFactory.playerHandicap[i]]}");
            }
            else
            {
                prevNumAIs = -1;
            }
        }
        else
        {
            DisplayText($"Turn: {gameServer.getTurnCount()}");
        }
        prevGameServer = gameServer;
    }
}
