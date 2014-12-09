using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace CustomTS
{
    public static class CTS
    {
        private static bool DrawText;
        private static Menu Menu;
        private static Menu Config;
        private static String text = "Target Selector Mode is now: ";
        private static Obj_AI_Hero selectedTarget;
        private static float selectedRange = 1000;


        public static void addTSToMenu(this Menu MainMenu)
        {
            var menu = MainMenu.AddSubMenu(new Menu("目标选择", "Target Selector"));
            menu.AddItem(new MenuItem("Selected Target", "右键锁定")).SetValue(true);
            menu.AddItem(new MenuItem("STR", "锁定范围")).SetValue(new Slider(1500, 1, 3000));
            menu.AddItem(new MenuItem("STS", "选择惩戒目标")).SetValue(true).SetShared();
            menu.AddItem(new MenuItem("Draw Target", "显示目标")).SetValue(new Circle(true, Color.DodgerBlue));
            menu.AddItem(new MenuItem("Selected Mode", "锁定模式"))
                .SetValue(
                    new StringList(new[]
                    {
                        "自动", "最近", "最少攻击", "最少魔法", "低血量", "最高AD", "最高Ap", "靠近鼠标",
                        "优先"
                    }));
            var priorMenu = menu.AddSubMenu(new Menu("优先", "Priority"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(a => !a.IsAlly))
            {
                priorMenu.AddItem(new MenuItem(enemy.ChampionName, enemy.ChampionName)).SetValue(new Slider(1, 1, 5));
            }
            priorMenu.AddItem(new MenuItem("Lowest no. is Highest", "最低的优先"));
            Game.OnGameUpdate += a => UpdateTSMode(MainMenu);
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Target() != null && Menu.Item("Draw Target").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(Target().Position, 120,  Target() == selectedTarget ?  Color.Red : Menu.Item("Draw Target").GetValue<Circle>().Color);
            }
           
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (Menu.Item("Selected Target").GetValue<bool>() == false) return;
            if (MenuGUI.IsChatOpen || ObjectManager.Player.Spellbook.SelectedSpellSlot != SpellSlot.Unknown)
            {
                return;
            }
            if (args.WParam == 1) // LMouse
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (hero.IsValidTarget() &&
                        Vector2.Distance(Game.CursorPos.To2D(), hero.ServerPosition.To2D()) < 300)
                    {
                        selectedTarget = hero;
                        Utility.DelayAction.Add(5000, () => selectedTarget = null);
                    }
                }
            }
        }


        private static int fatness(this Obj_AI_Hero t)
        {
            return (int) (t.ChampionsKilled*1 + t.Assists*0.375 + t.MinionsKilled*0.067);
        }

        public static void UpdateTSMode(Menu Config)
        {
            Menu = Config;
            
            bool Priority = false;
            TargetSelector.TargetingMode mode = TargetSelector.GetTargetingMode();
            switch (Config.Item("Selected Mode").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    mode = TargetSelector.TargetingMode.AutoPriority;
                    break;
                case 1:
                    mode = TargetSelector.TargetingMode.Closest;
                    break;
                case 2:
                    mode = TargetSelector.TargetingMode.LessAttack;
                    break;
                case 3:
                    mode = TargetSelector.TargetingMode.LessCast;
                    break;
                case 4:
                    mode = TargetSelector.TargetingMode.LowHP;
                    break;
                case 5:
                    mode = TargetSelector.TargetingMode.MostAD;
                    break;
                case 6:
                    mode = TargetSelector.TargetingMode.MostAP;
                    break;
                case 7:
                    mode = TargetSelector.TargetingMode.NearMouse;
                    break;
                case 8:
                    Priority = true;
                    break;
            }
            if (TargetSelector.GetTargetingMode() != mode && Priority == false)
            {
                TargetSelector.SetTargetingMode(mode);
            }

            if (selectedTarget.IsDead && selectedTarget != null) { selectedTarget = null; }
        }

        public static Obj_AI_Hero Target()
        {
            var priorty = 5;
            Obj_AI_Hero target = null;
            if (selectedTarget != null && selectedTarget.IsValidTarget(Menu.Item("STR").GetValue<Slider>().Value)) return selectedTarget;
            if (Menu.Item("Selected Mode").GetValue<StringList>().SelectedIndex == 8)
            {

                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(a => !a.IsAlly && a.IsValidTarget(TargetSelector.GetRange()))
                            .OrderBy(a => a.Health))
                {
                    if (Menu.Item(enemy.ChampionName).GetValue<Slider>().Value < priorty)
                    {
                        priorty = Menu.Item(enemy.ChampionName).GetValue<Slider>().Value;
                        target = enemy;
                    }
                }
            }
            else
            {
                target = TargetSelector.Target;
            }
            return target;
        }

        public static void setRange(float range)
        {
            TargetSelector.SetRange(range);
        }

        public static void setSelectRange(float MaxRange)
        {
            Menu.Item("STR").SetValue(new Slider((int)MaxRange, 1, 3000));
        }

        private static TargetSelector TargetSelector = new TargetSelector(0, TargetSelector.TargetingMode.AutoPriority);
    }
}
