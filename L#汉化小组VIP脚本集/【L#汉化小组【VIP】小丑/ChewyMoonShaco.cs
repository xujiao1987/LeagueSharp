#region

using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

#endregion

namespace ChewyMoonsShaco
{
    internal class ChewyMoonShaco
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;

        public static Menu Menu;
        public static LXOrbwalker Orbwalker;

        public static List<Spell> SpellList;

        public static int TiamatId = 3077;
        public static int HydraId = 3074;

        public static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Shaco") return;

            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 425);
            E = new Spell(SpellSlot.E, 625);

            SpellList = new List<Spell> { Q, E, W };

            CreateMenu();

            Game.OnGameUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            LXOrbwalker.AfterAttack += LxOrbwalkerOnAfterAttack;

            Game.PrintChat("<font color=\"#6699ff\"><b>ChewyMoon's Shaco:</b></font> <font color=\"#FFFFFF\">" +
                           "loaded!" +
                           "</font>");
        }

        private static void LxOrbwalkerOnAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe) return;
            if (!target.IsValidTarget() || target.IsMinion) return;

            if (Items.HasItem(HydraId) && Items.CanUseItem(HydraId))
            {
                Items.UseItem(TiamatId);
                LXOrbwalker.ResetAutoAttackTimer();
            }
            else if (Items.HasItem(TiamatId) && Items.CanUseItem(TiamatId))
            {
                Items.UseItem(TiamatId);
                LXOrbwalker.ResetAutoAttackTimer();
            }
        }

        private static void CreateMenu()
        {
            (Menu = new Menu("小丑", "cmShaco", true)).AddToMainMenu();

            // Target Selector
            var tsMenu = new Menu("目标选择器", "cmShacoTS");
            SimpleTs.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalking
            var orbwalkingMenu = new Menu("走砍", "cmShacoOrbwalkin");
            Orbwalker = new LXOrbwalker();
            LXOrbwalker.AddToMenu(orbwalkingMenu);
            Menu.AddSubMenu(orbwalkingMenu);

            // Combo
            var comboMenu = new Menu("连招", "cmShacoCombo");
            comboMenu.AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useItems", "使用项目").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass
            var harassMenu = new Menu("骚扰", "cmShacoHarass");
            harassMenu.AddItem(new MenuItem("useEHarass", "使用 E").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            // Drawing
            var drawingMenu = new Menu("技能范围", "cmShacoDrawing");
            drawingMenu.AddItem(new MenuItem("drawQ", "范围 Q").SetValue(new Circle(true, Color.Khaki)));
            drawingMenu.AddItem(new MenuItem("drawQPos", "范围 Q Pos").SetValue(new Circle(true, Color.Magenta)));
            drawingMenu.AddItem(new MenuItem("drawW", "范围 W").SetValue(new Circle(true, Color.Khaki)));
            drawingMenu.AddItem(new MenuItem("drawE", "范围 E").SetValue(new Circle(true, Color.Khaki)));
            Menu.AddSubMenu(drawingMenu);

            // Misc
            var miscMenu = new Menu("杂项", "cmShacoMisc");
            miscMenu.AddItem(new MenuItem("usePackets", "使用封包").SetValue(true));
            Menu.AddSubMenu(miscMenu);
			
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var qCircle = Menu.Item("drawQ").GetValue<Circle>();
            var wCircle = Menu.Item("drawW").GetValue<Circle>();
            var eCircle = Menu.Item("drawE").GetValue<Circle>();
            var qPosCircle = Menu.Item("drawQPos").GetValue<Circle>();

            var pos = ObjectManager.Player.Position;

            if (qCircle.Active)
            {
                Utility.DrawCircle(pos, Q.Range, qCircle.Color);
            }

            if (wCircle.Active)
            {
                Utility.DrawCircle(pos, W.Range, wCircle.Color);
            }

            if (eCircle.Active)
            {
                Utility.DrawCircle(pos, E.Range, eCircle.Color);
            }

            if (qPosCircle.Active)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget()))
                {
                    Drawing.DrawLine(Drawing.WorldToScreen(enemy.Position),
                        Drawing.WorldToScreen(ShacoUtil.GetQPos(enemy, false)), 2, qPosCircle.Color);
                }
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    Combo();
                    break;

                case LXOrbwalker.Mode.Harass:
                    Harass();
                    break;
            }
        }

        private static void Combo()
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);

            var useQ = Menu.Item("useQ").GetValue<bool>();
            var useW = Menu.Item("useW").GetValue<bool>();
            var useE = Menu.Item("useE").GetValue<bool>();
            var packets = Menu.Item("usePackets").GetValue<bool>();

            foreach (var spell in SpellList.Where(x => x.IsReady()))
            {
                if (spell.Slot == SpellSlot.Q && useQ)
                {
                    if (!target.IsValidTarget(Q.Range)) continue;

                    var pos = ShacoUtil.GetQPos(target, true);
                    Q.Cast(pos, packets);
                }

                if (spell.Slot == SpellSlot.W && useW)
                {
                    if (!target.IsValidTarget(W.Range)) continue;

                    var pos = ShacoUtil.GetShortestWayPoint(target.GetWaypoints());
                    W.Cast(pos, packets);
                }

                if (spell.Slot != SpellSlot.E || !useE) continue;
                if (!target.IsValidTarget(E.Range)) continue;

                E.CastOnUnit(target);
            }
        }

        private static void Harass()
        {
            var useE = Menu.Item("useEHarass").GetValue<bool>();
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);

            if (!target.IsValidTarget(E.Range)) return;

            if (useE && E.IsReady())
            {
                E.CastOnUnit(target);
            }
        }
    }
}