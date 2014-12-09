using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = MasterCommon.M_Orbwalker;

namespace Master
{
    class Rammus : Program
    {
        public Rammus()
        {
            SkillQ = new Spell(SpellSlot.Q, 800);
            SkillW = new Spell(SpellSlot.W, 325);
            SkillE = new Spell(SpellSlot.E, 300);
            SkillR = new Spell(SpellSlot.R, 300);
            SkillE.SetTargetted(SkillE.Instance.SData.SpellCastTime, SkillE.Instance.SData.MissileSpeed);
            SkillR.SetSkillshot(SkillR.Instance.SData.SpellCastTime, SkillR.Instance.SData.LineWidth, SkillR.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);

            Config.AddSubMenu(new Menu("连招/骚扰", "csettings"));
            Config.SubMenu("csettings").AddItem(new MenuItem("qusage", "使用Q").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("wusage", "使用W").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("eusage", "使用E").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("euseMode", "E模式").SetValue(new StringList(new[] { "一直", "W冷却" })));
            Config.SubMenu("csettings").AddItem(new MenuItem("autoeusage", "血量大于X使用E").SetValue(new Slider(20, 1)));
            Config.SubMenu("csettings").AddItem(new MenuItem("rusage", "使用R").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("ruseMode", "R模式").SetValue(new StringList(new[] { "一直", "敌人大于" })));
            Config.SubMenu("csettings").AddItem(new MenuItem("rmulti", "敌人大于X使用R").SetValue(new Slider(2, 1, 4)));
            Config.SubMenu("csettings").AddItem(new MenuItem("ignite", "可杀点燃").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem("iusage", "使用物品").SetValue(true));

            Config.AddSubMenu(new Menu("清线", "LaneJungClear"));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearQ", "使用Q").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearW", "使用W").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearE", "使用E").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem("useClearEMode", "E模式").SetValue(new StringList(new[] { "一直", "W冷却" })));

            Config.AddSubMenu(new Menu("杂项", "miscs"));
            Config.SubMenu("miscs").AddItem(new MenuItem("useAntiQ", "使用Q防突").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem("useInterE", "使用E打断").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem("CustomSkin", "换肤").SetValue(new Slider(6, 0, 6))).ValueChanged += SkinChanger;

            Config.AddSubMenu(new Menu("显示", "DrawSettings"));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawE", "E范围").SetValue(true));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawR", "R范围").SetValue(true));

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen) return;
            if (Player.HasBuff("PowerBall", true)) Game.PrintChat("q");
            if (Player.HasBuff("DefensiveBallCurl", true)) Game.PrintChat("w");
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo)
            {
                NormalCombo();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo(true);
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear || Orbwalk.CurrentMode == Orbwalk.Mode.LaneFreeze)
            {
                LaneJungClear();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.Flee && SkillQ.IsReady() && !Player.HasBuff("PowerBall", true)) SkillQ.Cast(PacketCast());
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Config.Item("DrawE").GetValue<bool>() && SkillE.Level > 0) Utility.DrawCircle(Player.Position, SkillE.Range, SkillE.IsReady() ? Color.Green : Color.Red);
            if (Config.Item("DrawR").GetValue<bool>() && SkillR.Level > 0) Utility.DrawCircle(Player.Position, SkillR.Range, SkillR.IsReady() ? Color.Green : Color.Red);
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Config.Item("useAntiQ").GetValue<bool>()) return;
            if (gapcloser.Sender.IsValidTarget(SkillW.Range) && SkillQ.IsReady() && !Player.HasBuff("PowerBall", true)) SkillQ.Cast(PacketCast());
        }

        private void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("useInterE").GetValue<bool>()) return;
            if (unit.IsValidTarget(SkillE.Range) && SkillE.IsReady()) SkillE.CastOnUnit(unit, PacketCast());
        }

        private void NormalCombo(bool IsHarass = false)
        {
            if (targetObj == null) return;
            if (Config.Item("qusage").GetValue<bool>() && SkillQ.IsReady() && targetObj.IsValidTarget(IsHarass ? Orbwalk.GetAutoAttackRange(Player, targetObj) : SkillQ.Range) && !Player.HasBuff("PowerBall", true))
            {
                if (!SkillE.InRange(targetObj.Position))
                {
                    SkillQ.Cast(PacketCast());
                }
                else if (!Player.HasBuff("DefensiveBallCurl", true)) SkillQ.Cast(PacketCast());
            }
            if (Config.Item("wusage").GetValue<bool>() && SkillW.IsReady() && SkillW.InRange(targetObj.Position) && !Player.HasBuff("PowerBall", true)) SkillW.Cast(PacketCast());
            if (Config.Item("eusage").GetValue<bool>() && SkillE.IsReady() && SkillE.InRange(targetObj.Position) && Player.Health * 100 / Player.MaxHealth >= Config.Item("autoeusage").GetValue<Slider>().Value)
            {
                switch (Config.Item("euseMode").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        SkillE.CastOnUnit(targetObj, PacketCast());
                        break;
                    case 1:
                        if (Player.HasBuff("DefensiveBallCurl", true)) SkillE.CastOnUnit(targetObj, PacketCast());
                        break;
                }
            }
            if (Config.Item("rusage").GetValue<bool>() && SkillR.IsReady())
            {
                switch (Config.Item("ruseMode").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (SkillR.InRange(targetObj.Position)) SkillR.Cast(PacketCast());
                        break;
                    case 1:
                        if (Player.CountEnemysInRange((int)SkillR.Range) >= Config.Item("rmulti").GetValue<Slider>().Value) SkillR.Cast(PacketCast());
                        break;
                }
            }
            if (Config.Item("iusage").GetValue<bool>() && Items.CanUseItem(Rand) && Player.CountEnemysInRange(450) >= 1) Items.UseItem(Rand);
            if (Config.Item("ignite").GetValue<bool>()) CastIgnite(targetObj);
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(Player.Position, SkillQ.Range, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault();
            if (minionObj == null) return;
            if (Config.Item("useClearQ").GetValue<bool>() && SkillQ.IsReady() && !Player.HasBuff("PowerBall", true))
            {
                if (!SkillE.InRange(minionObj.Position))
                {
                    SkillQ.Cast(PacketCast());
                }
                else if (!Player.HasBuff("DefensiveBallCurl", true)) SkillQ.Cast(PacketCast());
            }
            if (Config.Item("useClearW").GetValue<bool>() && SkillW.IsReady() && SkillW.InRange(minionObj.Position) && !Player.HasBuff("PowerBall", true)) SkillW.Cast(PacketCast());
            if (Config.Item("useClearE").GetValue<bool>() && SkillE.IsReady() && SkillE.InRange(minionObj.Position))
            {
                switch (Config.Item("useClearEMode").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        SkillE.CastOnUnit(minionObj, PacketCast());
                        break;
                    case 1:
                        if (Player.HasBuff("DefensiveBallCurl", true)) SkillE.CastOnUnit(minionObj, PacketCast());
                        break;
                }
            }
        }
    }
}