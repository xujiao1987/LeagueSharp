﻿using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace AhriSharp
{
    class Program
    {
        private const string ChampionName = "Ahri";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Spell Q, W, E, R;
        private static Items.Item DFG;

        private static Menu Config;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != ChampionName)
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 860);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 960);
            R = new Spell(SpellSlot.R, 450); //Soon(TM)

            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            Q.SetSkillshot(0.50f, 100f, 1100f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.50f, 60f, 1200f, true, SkillshotType.SkillshotLine);


            //Menu
            Config = new Menu("阿狸", "Ahri", true);


            //Orbwalker & Target Selector
            Config.AddSubMenu(new Menu("走砍", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Combo
            Config.AddSubMenu(new Menu("连招", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("UseQ", "使用Q")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("UseW", "使用W")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("UseE", "使用E")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("UseDFG", "对魅惑目标使用冥火")).SetValue(true);
            //Config.SubMenu("combo").AddItem(new MenuItem("UseR", "使用R")).SetValue(false); Soon(TM)
            Config.SubMenu("combo").AddItem(new MenuItem("ActiveCombo", "热键").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harras
            Config.AddSubMenu(new Menu("骚扰", "harras"));
            Config.SubMenu("harras").AddItem(new MenuItem("UseQh", "使用Q")).SetValue(true);
            Config.SubMenu("harras").AddItem(new MenuItem("ActiveHarras", "热键").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //LaneClear
            Config.AddSubMenu(new Menu("清线", "laneClear")); //Add Mana Option.
            Config.SubMenu("laneClear").AddItem(new MenuItem("UseQc", "使用Q")).SetValue(true);
            Config.SubMenu("laneClear").AddItem(new MenuItem("UseWc", "使用W")).SetValue(false);
            Config.SubMenu("laneClear").AddItem(new MenuItem("LMana", "蓝量控制").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("laneClear").AddItem(new MenuItem("ActiveLane", "热键").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //JungleClear
            Config.AddSubMenu(new Menu("清野", "jungleClear"));
            Config.SubMenu("jungleClear").AddItem(new MenuItem("UseQj", "使用Q")).SetValue(true);
            Config.SubMenu("jungleClear").AddItem(new MenuItem("UseWj", "使用W")).SetValue(true);
            Config.SubMenu("jungleClear").AddItem(new MenuItem("ActiveJungle", "热键").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("显示", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("qRange", "Q范围").SetValue(new Circle(true, Color.FromArgb(255, 225, 70, 70))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("eRange", "E范围").SetValue(new Circle(true, Color.FromArgb(200, 225, 70, 230))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("rRange", "R范围").SetValue(new Circle(true, Color.FromArgb(200, 225, 70, 70))));

		
            Config.AddToMainMenu();

            Drawing.OnDraw += OnDraw;
            Game.OnGameUpdate += OnGameUpdate;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Config.Item("ActiveHarras").GetValue<KeyBind>().Active)
            {
                Harras();
            }
            if (Config.Item("ActiveJungle").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }
            if (Config.Item("ActiveLane").GetValue<KeyBind>().Active)
            {
                LaneClear();
            }
        }

        static void OnDraw(EventArgs args)
        {
            var qCircle = Config.Item("qRange").GetValue<Circle>();
            var eCircle = Config.Item("eRange").GetValue<Circle>();
            var rCircle = Config.Item("rRange").GetValue<Circle>();

            if (qCircle.Active && Q.IsReady())
                Utility.DrawCircle(Player.Position, Q.Range, qCircle.Color);
            if (eCircle.Active && E.IsReady())
                Utility.DrawCircle(Player.Position, E.Range, rCircle.Color);
            if (rCircle.Active && R.IsReady())
                Utility.DrawCircle(Player.Position, R.Range, rCircle.Color);
        }

        private static void Combo()
        {
            Obj_AI_Hero target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var useQ = Config.Item("UseQ").GetValue<bool>();
            var useW = Config.Item("UseW").GetValue<bool>();
            var useE = Config.Item("UseE").GetValue<bool>();
            if (target == null) return;


            if (useW && target.IsValidTarget(W.Range) & W.IsReady())
            {
                W.Cast();
            }
            if (useE && target.IsValidTarget(E.Range) && E.IsReady())
            {
                PredictionOutput ePred = E.GetPrediction(target);
                if (ePred.Hitchance >= HitChance.High)
                    E.Cast(ePred.CastPosition);
            }
            if (target.HasBuffOfType(BuffType.Charm) && DFG.IsReady())
            {
                DFG.Cast(target);
            }
            if (useQ && target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                PredictionOutput qPred = Q.GetPrediction(target);
                if (qPred.Hitchance >= HitChance.High)
                    Q.Cast(qPred.CastPosition);
            }
        }

        private static void Harras()
        {
            Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var useQh = Config.Item("UseQh").GetValue<bool>();
            if (target == null) return;

            if (useQh && target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                PredictionOutput qPred = Q.GetPrediction(target);
                if (qPred.Hitchance >= HitChance.High)
                    Q.Cast(qPred.CastPosition);
            }
        }
        private static void JungleClear()
        {
            var useQj = Config.Item("UseQj").GetValue<bool>();
            var useWj = Config.Item("UseWj").GetValue<bool>();
            var monster = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (useQj && Q.IsReady())
            {
                var qmonster = Q.GetLineFarmLocation(monster);
                if (qmonster.MinionsHit >= 1)
                    Q.Cast(qmonster.Position);
            }
            if (useWj && W.IsReady() && monster.Count >= 1)
            {
                W.Cast();
            }

        }
        private static void LaneClear()
        {
            var useQc = Config.Item("UseQc").GetValue<bool>();
            var useWc = Config.Item("UseWc").GetValue<bool>();
            var FMana = Config.Item("LMana").GetValue<Slider>().Value;
            var MPercent = Player.Mana * 100 / Player.MaxMana;
            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width);

            if (useQc && Q.IsReady() && MPercent >= FMana)
            {
                var qpos = Q.GetLineFarmLocation(minions);
                if (qpos.MinionsHit >= 2)
                    Q.Cast(qpos.Position);
            }
            if (useWc && W.IsReady() && minions.Count >= 1)
            {
                W.Cast();
            }
        }
    }
}