using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace TAC_Kalista
{
    class MenuHandler
    {
        public static Menu Config;
        internal static Orbwalking.Orbwalker orb;
        public static void init()
        {
            Config = new Menu("复仇之矛", "Kalista", true);

            var targetselectormenu = new Menu("目标选择", "Common_TargetSelector");
            SimpleTs.AddToMenu(targetselectormenu);
            Config.AddSubMenu(targetselectormenu);

            Menu orbwalker = new Menu("走砍", "orbwalker");
            orb = new Orbwalking.Orbwalker(orbwalker);
            Config.AddSubMenu(orbwalker);

            Config.AddSubMenu(new Menu("设置", "ac"));
            
            Config.SubMenu("ac").AddSubMenu(new Menu("技能","skillUsage"));
            Config.SubMenu("ac").SubMenu("skillUsage").AddItem(new MenuItem("UseQAC", "使用Q").SetValue(true));
            Config.SubMenu("ac").SubMenu("skillUsage").AddItem(new MenuItem("UseEAC", "使用E").SetValue(true));
            
            Config.SubMenu("ac").AddSubMenu(new Menu("技能设置","skillConfiguration"));
            Config.SubMenu("ac").SubMenu("skillConfiguration").AddItem(new MenuItem("UseQACM", "使用Q当距离").SetValue(new StringList(new[] { "远", "中等", "近" }, 2)));
            Config.SubMenu("ac").SubMenu("skillConfiguration").AddItem(new MenuItem("E4K", "使用E击杀").SetValue(true));
            Config.SubMenu("ac").SubMenu("skillConfiguration").AddItem(new MenuItem("UseEACSlow", "对被减速目标使用E").SetValue(false));
            Config.SubMenu("ac").SubMenu("skillConfiguration").AddItem(new MenuItem("UseEACSlowT", "敌人小于").SetValue(new Slider(1, 1, 5)));
            Config.SubMenu("ac").SubMenu("skillConfiguration").AddItem(new MenuItem("minE", "被动大于X使用E").SetValue(new Slider(1, 1, 20)));
            Config.SubMenu("ac").SubMenu("skillConfiguration").AddItem(new MenuItem("minEE", "被动大于X启用E").SetValue(false));
            
            Config.SubMenu("ac").AddSubMenu(new Menu("物品","itemsAC"));
            Config.SubMenu("ac").SubMenu("itemsAC").AddItem(new MenuItem("useItems", "使用物品").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));

            Config.SubMenu("ac").SubMenu("itemsAC").AddItem(new MenuItem("allIn", "所有模式").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle)));
//            Config.SubMenu("ac").SubMenu("itemsAC").AddItem(new MenuItem("allInAt", "Auto All in when X hero").SetValue(new Slider(2, 1, 5)));
            
            Config.SubMenu("ac").SubMenu("itemsAC").AddItem(new MenuItem("BOTRK", "使用破败").SetValue(true));
            Config.SubMenu("ac").SubMenu("itemsAC").AddItem(new MenuItem("GHOSTBLADE", "使用幽梦").SetValue(true));
            Config.SubMenu("ac").SubMenu("itemsAC").AddItem(new MenuItem("SWORD", "使用神圣之剑").SetValue(true));

            Config.SubMenu("ac").SubMenu("itemsAC").AddSubMenu(new Menu("净化设置", "QSS"));
            Config.SubMenu("ac").SubMenu("itemsAC").SubMenu("QSS").AddItem(new MenuItem("AnyStun", "眩晕").SetValue(true));
            Config.SubMenu("ac").SubMenu("itemsAC").SubMenu("QSS").AddItem(new MenuItem("AnySnare", "陷阱").SetValue(true));
            Config.SubMenu("ac").SubMenu("itemsAC").SubMenu("QSS").AddItem(new MenuItem("AnyTaunt", "嘲讽").SetValue(true));
            foreach (var t in ItemHandler.BuffList)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                {
                    if (t.ChampionName == enemy.ChampionName)
                        Config.SubMenu("ac").SubMenu("itemsAC").SubMenu("QSS").AddItem(new MenuItem(t.BuffName, t.DisplayName).SetValue(t.DefaultValue));
                }
            }

            Config.AddSubMenu(new Menu("杂项", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("saveSould", "保留R").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("soulHP", "血量低于X使用").SetValue(new Slider(15,1,100)));
            Config.SubMenu("misc").AddItem(new MenuItem("soulEnemyCount", "敌军大于").SetValue(new Slider(3, 1, 5)));
            Config.SubMenu("misc").AddItem(new MenuItem("antiGap", "防突").SetValue(false));
            Config.SubMenu("misc").AddItem(new MenuItem("antiGapRange", "范围").SetValue(new Slider(300, 300, 400)));
            Config.SubMenu("misc").AddItem(new MenuItem("antiGapPrevent", "连招模式防突").SetValue(true));

            Config.AddSubMenu(new Menu("骚扰", "harass"));
            Config.SubMenu("harass").AddItem(new MenuItem("harassQ", "使用Q").SetValue(true));
            Config.SubMenu("harass").AddItem(new MenuItem("stackE", "使用E").SetValue(new Slider(1, 1, 10)));
            Config.SubMenu("harass").AddItem(new MenuItem("manaPercent", "蓝量控制").SetValue(new Slider(40, 1, 100)));

            Config.AddSubMenu(new Menu("清线", "wc"));
            Config.SubMenu("wc").AddItem(new MenuItem("wcQ", "使用Q").SetValue(true));
            Config.SubMenu("wc").AddItem(new MenuItem("wcE", "使用E").SetValue(true));
            Config.SubMenu("wc").AddItem(new MenuItem("enableClear", "开关").SetValue(false));
            
            Config.AddSubMenu(new Menu("惩戒", "smite"));
            Config.SubMenu("smite").AddItem(new MenuItem("SRU_Baron", "大龙").SetValue(true));
            Config.SubMenu("smite").AddItem(new MenuItem("SRU_Dragon", "小龙").SetValue(true));
            Config.SubMenu("smite").AddItem(new MenuItem("SRU_Gromp", "蛤蟆").SetValue(false));
            Config.SubMenu("smite").AddItem(new MenuItem("SRU_Murkwolf", "三狼").SetValue(false));
            Config.SubMenu("smite").AddItem(new MenuItem("SRU_Krug", "石像").SetValue(false));
            Config.SubMenu("smite").AddItem(new MenuItem("SRU_Razorbeak", "F4").SetValue(false));
            Config.SubMenu("smite").AddItem(new MenuItem("Sru_Crab", "河道螃蟹").SetValue(false));
            Config.SubMenu("smite").AddItem(new MenuItem("smite", "自动惩戒").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));

            Config.AddSubMenu(new Menu("穿墙", "wh"));
            Config.SubMenu("wh").AddItem(new MenuItem("JumpTo", "热键(按住)").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("显示", "Drawings"));
            Config.SubMenu("Drawings").AddSubMenu(new Menu("范围", "range"));

            Config.SubMenu("Drawings").SubMenu("range").AddItem(new MenuItem("QRange", "Q范围").SetValue(new Circle(true, Color.FromArgb(100, Color.Red))));
            Config.SubMenu("Drawings").SubMenu("range").AddItem(new MenuItem("WRange", "W范围").SetValue(new Circle(false, Color.FromArgb(100, Color.Coral))));
            Config.SubMenu("Drawings").SubMenu("range").AddItem(new MenuItem("ERange", "E范围").SetValue(new Circle(true, Color.FromArgb(100, Color.BlueViolet))));
            Config.SubMenu("Drawings").SubMenu("range").AddItem(new MenuItem("drawESlow", "E减速范围").SetValue(true));
            Config.SubMenu("Drawings").SubMenu("range").AddItem(new MenuItem("RRange", "R范围").SetValue(new Circle(false, Color.FromArgb(100, Color.Blue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawJumpPos", "显示穿墙点")).SetValue(new Circle(false, Color.HotPink));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawJumpPosRange", "技能穿墙范围").SetValue(new StringList(new[] { "Q", "E", "R", "AA" }, 2)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("drawHp", "显示伤害")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("drawStacks", "显示全部被动")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("enableDrawings", "显示开关").SetValue(true));          

            Config.AddItem(new MenuItem("Packets", "封包").SetValue(true));

            Config.AddItem(new MenuItem("debug", "调试").SetValue(false));
 
            Config.AddToMainMenu();

        }
    }
}
