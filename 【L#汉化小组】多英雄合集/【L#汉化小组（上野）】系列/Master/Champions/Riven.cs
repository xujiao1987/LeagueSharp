using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterCommon.M_Orbwalker;

namespace Master
{
    class Riven : Program
    {
        private const String Version = "1.0.0";

        public Riven()
        {
            SkillQ = new Spell(SpellSlot.Q, 1100);//1300
            SkillW = new Spell(SpellSlot.W, 700);
            SkillE = new Spell(SpellSlot.E, 425);//575
            SkillR = new Spell(SpellSlot.R, 375);
            SkillE.SetSkillshot(SkillE.Instance.SData.SpellCastTime, SkillE.Instance.SData.LineWidth, SkillE.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);

            Config.AddSubMenu(new Menu("连招", "csettings"));
            Config.SubMenu("csettings").AddItem(new MenuItem("qusage", "使用Q").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("wusage", "使用W").SetValue(false));
            Config.SubMenu("csettings").AddItem(new MenuItem("eusage", "使用E").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("rusage", "使用R").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("ignite", "可杀点燃").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("iusage", "使用Item").SetValue(true));

            Config.AddSubMenu(new Menu("连招设置", "cexsettings"));
            Config.SubMenu("cexsettings").AddSubMenu(new Menu("W", "configW"));
            Config.SubMenu("cexsettings").SubMenu("configW").AddItem(new MenuItem("itemW", "W后使用物品").SetValue(true));
            Config.SubMenu("cexsettings").SubMenu("configW").AddItem(new MenuItem("autoW", "敌人大于X自动W").SetValue(new Slider(2, 1, 4)));
            Config.SubMenu("cexsettings").AddSubMenu(new Menu("R(第一段)", "configR1"));
            Config.SubMenu("cexsettings").SubMenu("configR1").AddItem(new MenuItem("modeR1", "模式").SetValue(new StringList(new[] { "敌人大于", "目标生命", "目标范围", "全部" }, 1)));
            Config.SubMenu("cexsettings").SubMenu("configR1").AddItem(new MenuItem("countR1", "敌人大于X使用R").SetValue(new Slider(1, 1, 4)));
            Config.SubMenu("cexsettings").SubMenu("configR1").AddItem(new MenuItem("healthR1", "目标生命低于X使用R").SetValue(new Slider(65, 1)));
            Config.SubMenu("cexsettings").SubMenu("configR1").AddItem(new MenuItem("rangeR1", "目标在范围内使用R").SetValue(new Slider(400, 125, 550)));
            Config.SubMenu("cexsettings").AddSubMenu(new Menu("R(第二段)", "configR2"));
            Config.SubMenu("cexsettings").SubMenu("configR2").AddItem(new MenuItem("modeR2", "模式").SetValue(new StringList(new[] { "最大伤害", "击杀" }, 1)));
            Config.SubMenu("cexsettings").SubMenu("configR2").AddItem(new MenuItem("ksR2", "使用R抢头l").SetValue(true));

            Config.AddSubMenu(new Menu("骚扰", "hsettings"));
            Config.SubMenu("hsettings").AddItem(new MenuItem("harMode", "血量大于X骚扰").SetValue(new Slider(20, 1)));
            Config.SubMenu("hsettings").AddItem(new MenuItem("useHarQ", "使用Q").SetValue(true));
            Config.SubMenu("hsettings").AddItem(new MenuItem("useHarW", "使用W").SetValue(true));
            Config.SubMenu("hsettings").AddItem(new MenuItem("useHarE", "使用E").SetValue(true));

            Config.AddSubMenu(new Menu("杂项", "miscs"));
            Config.SubMenu("miscs").AddItem(new MenuItem("cancelAni", "取消动画").SetValue(new StringList(new[] { "跳舞", "嘲笑", "移动" }, 2)));
            Config.SubMenu("miscs").AddItem(new MenuItem("useDodgeE", "使用E躲技能").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem("useAntiW", "使用W防突").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem("useInterW", "使用W打断").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem("CustomSkin", "换肤").SetValue(new Slider(4, 0, 6))).ValueChanged += SkinChanger;
            Config.SubMenu("miscs").AddItem(new MenuItem("packetCast", "封包").SetValue(true));

            Config.AddSubMenu(new Menu("清线", "LaneJungClear"));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearQ", "使用Q").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearW", "使用W").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearE", "使用E").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearI", "使用提亚马特/九头蛇").SetValue(true));

            Config.AddSubMenu(new Menu("显示", "DrawSettings"));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawQ", "Q范围").SetValue(false));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawW", "W范围").SetValue(true));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawE", "E范围").SetValue(true));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawR", "R范围").SetValue(false));

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            Game.OnGameProcessPacket += OnGameProcessPacket;
            Game.OnGameSendPacket += OnGameSendPacket;
            Game.PrintChat("<font color = \"#33CCCC\">Master of {0}</font> <font color = \"#00ff00\">v{1}</font>", Name, Version);
        }

        private void OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (targetObj != null && PacketCast() && (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass))
            {
                if (args.PacketData[0] != 100) return;
                var PacketData = new GamePacket(args.PacketData);
                PacketData.Position = 1;
                if (PacketData.ReadInteger() != targetObj.NetworkId) return;
                var PacketType = PacketData.ReadByte();
                PacketData.Position += 4;
                if (PacketData.ReadInteger() != Player.NetworkId) return;
                if (PacketType == 12) SkillQ.Cast(targetObj.Position, PacketCast());
                return;
            }
        }

        private void OnGameSendPacket(GamePacketEventArgs args)
        {
            if (!PacketCast()) return;
            if (args.PacketData[0] == 153)
            {
                var PacketData = new GamePacket(args.PacketData[0]);
                PacketData.Position = 1;
                if (PacketData.ReadFloat() != Player.NetworkId) return;
                if (PacketData.ReadByte() != 0) return;
                switch (Config.Item("cancelAni").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        Packet.C2S.Emote.Encoded(new Packet.C2S.Emote.Struct(0));
                        break;
                    case 1:
                        Packet.C2S.Emote.Encoded(new Packet.C2S.Emote.Struct(1));
                        break;
                    case 2:
                        if (targetObj != null)
                        {
                            var pos = targetObj.Position + Vector3.Normalize(Player.Position - targetObj.Position) * (Player.Distance(targetObj) + 50);
                            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(pos.X, pos.Y)).Process();
                        }
                        break;
                }
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            switch (Orbwalk.CurrentMode)
            {
                case Orbwalk.Mode.Combo:
                    //NormalCombo();
                    break;
                case Orbwalk.Mode.Harass:
                    //Harass();
                    break;
                case Orbwalk.Mode.LaneClear:
                    //LaneJungClear();
                    break;
                case Orbwalk.Mode.LaneFreeze:
                    break;
            }
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Config.Item("DrawQ").GetValue<bool>() && SkillQ.Level > 0) Utility.DrawCircle(Player.Position, SkillQ.Range, SkillQ.IsReady() ? Color.Green : Color.Red);
            if (Config.Item("DrawW").GetValue<bool>() && SkillW.Level > 0) Utility.DrawCircle(Player.Position, SkillW.Range, SkillW.IsReady() ? Color.Green : Color.Red);
            if (Config.Item("DrawE").GetValue<bool>() && SkillE.Level > 0) Utility.DrawCircle(Player.Position, SkillE.Range, SkillE.IsReady() ? Color.Green : Color.Red);
            if (Config.Item("DrawR").GetValue<bool>() && SkillR.Level > 0) Utility.DrawCircle(Player.Position, SkillR.Range, SkillR.IsReady() ? Color.Green : Color.Red);
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Config.Item("useAntiW").GetValue<bool>()) return;
            if (gapcloser.Sender.IsValidTarget(SkillW.Range) && SkillW.IsReady()) SkillW.Cast();
        }

        private void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("useInterW").GetValue<bool>()) return;
            if (SkillW.IsReady() && SkillE.IsReady() && !unit.IsValidTarget(SkillW.Range) && unit.IsValidTarget(SkillE.Range)) SkillE.Cast(unit.Position, PacketCast());
            if (unit.IsValidTarget(SkillW.Range) && SkillW.IsReady()) SkillW.Cast();
        }
    }
}